/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using API;
    using Internal;
    using Types;
    using Dtype = Types.Dtype;
    using ValueFlags = Internal.Function.ValueFlags;

    /// <summary>
    /// Make predictions.
    /// </summary>
    public sealed class PredictionService {

        #region --Client API--
        /// <summary>
        /// Create a prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values. This only applies to `CLOUD` predictions.</param>
        /// <param name="rawOutputs">Skip parsing output values into plain values. This only applies to `CLOUD` predictions.</param>
        /// <param name="dataUrlLimit">Return a data URL if a given output value is smaller than this size. This only applies to `CLOUD` predictions.</param>
        /// <param name="acceleration">Prediction acceleration. This only applies to `EDGE` predictions.</param>
        /// <param name="device">Prediction device. Do not set this unless you know what you are doing. This only applies to `EDGE` predictions.</param>
        public async Task<Prediction> Create (
            string tag,
            Dictionary<string, object>? inputs = null,
            bool rawOutputs = false,
            int? dataUrlLimit = null,
            Acceleration acceleration = default,
            IntPtr device = default
        ) {
            // Check cache
            if (cache.TryGetValue(tag, out var p))
                return Predict(tag, p, inputs);
            // Collect inputs
            var key = Guid.NewGuid().ToString();
            var values = inputs != null ? await Task.WhenAll(inputs.Select(async pair => {
                var name = pair.Key;
                var value = await ToValue(pair.Value, name, key: key);
                return new ValueInput { name = name, data = value.data, type = value.type, shape = value.shape };
            })) : null;
            // Query
            var prediction = await client.Query<Prediction?>(
                @$"mutation ($input: CreatePredictionInput!) {{
                    createPrediction (input: $input) {{
                        {Fields}
                    }}
                }}",
                @"createPrediction",
                new () {
                    ["input"] = new CreatePredictionInput {
                        tag = tag,
                        client = client.ClientId,
                        inputs = values,
                        dataUrlLimit = dataUrlLimit,
                        configuration = ConfigurationId,
                        device = client.DeviceId,
                    }
                }
            );
            // Parse
            var predictor = prediction.type == PredictorType.Edge ? await Load(prediction, acceleration, device) : IntPtr.Zero;
            var edgePrediction = predictor != IntPtr.Zero && inputs != null ? Predict(tag, predictor, inputs) : null;
            prediction.results = edgePrediction?.results ?? await ParseResults(prediction.results, rawOutputs);
            prediction.latency = edgePrediction?.latency ?? prediction.latency;
            prediction.error = edgePrediction?.error ?? prediction.error;
            prediction.logs = edgePrediction?.error ?? prediction.logs;
            // Return
            return prediction;
        }

        /// <summary>
        /// Create a streaming prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values. Only applies to `CLOUD` predictions.</param>
        /// <param name="rawOutputs">Skip parsing output values into plain values.</param>
        /// <param name="dataUrlLimit">Return a data URL if a given output value is smaller than this size. Only applies to `CLOUD` predictions.</param>
        public async IAsyncEnumerable<Prediction> Stream (
            string tag,
            Dictionary<string, object>? inputs = null,
            bool rawOutputs = false,
            int? dataUrlLimit = null
        ) {
            // Collect inputs
            var key = Guid.NewGuid().ToString();
            var values = inputs != null ?
                (await Task.WhenAll(inputs.Select(async pair => (name: pair.Key, value: await ToValue(pair.Value, pair.Key, key: key))))).ToDictionary(pair => pair.name, pair => pair.value as object) :
                null;
            // Stream
            var path = $"/predict/{tag}?stream=true&rawOutputs=true&dataUrlLimit={dataUrlLimit}";
            await foreach (var prediction in client.Stream<Prediction?>(path, values)) {
                // Collect results
                prediction.results = await ParseResults(prediction.results, rawOutputs);
                // Yield
                yield return prediction;
            }
        }

        /// <summary>
        /// Delete an edge predictor that is loaded in memory.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <returns>Whether the edge predictor was successfully deleted from memory.</returns>
        public async Task<bool> Delete (string tag) {
            // Check
            if (!cache.TryGetValue(tag, out var predictor))
                return false;
            // Release
            predictor.ReleasePredictor().CheckStatus();
            // Return
            return true;
        }

        /// <summary>
        /// Convert a Function value to a plain object.
        /// </summary>
        /// <param name="value">Function value.</param>
        /// <returns>Plain object or `Value` if the value cannot be converted to a plain object.</returns>
        public async Task<object> ToObject (Value value) {
            // Null
            if (value.type == Dtype.Null)
                return null;
            // Download
            var stream = await storage.Download(value.data);
            // Switch
            switch (value.type) {
                case Dtype.String:  return new StreamReader(stream).ReadToEnd();
                case Dtype.Float32: return value.shape.Length > 0 ? ToArray<float>(stream) : ToScalar<float>(stream);
                case Dtype.Float64: return value.shape.Length > 0 ? ToArray<double>(stream) : ToScalar<double>(stream);
                case Dtype.Int8:    return value.shape.Length > 0 ? ToArray<sbyte>(stream) : ToScalar<sbyte>(stream);
                case Dtype.Int16:   return value.shape.Length > 0 ? ToArray<short>(stream) : ToScalar<short>(stream);
                case Dtype.Int32:   return value.shape.Length > 0 ? ToArray<int>(stream) : ToScalar<int>(stream);
                case Dtype.Int64:   return value.shape.Length > 0 ? ToArray<long>(stream) : ToScalar<long>(stream);
                case Dtype.Uint8:   return value.shape.Length > 0 ? ToArray<byte>(stream) : ToScalar<byte>(stream);
                case Dtype.Uint16:  return value.shape.Length > 0 ? ToArray<ushort>(stream) : ToScalar<ushort>(stream);
                case Dtype.Uint32:  return value.shape.Length > 0 ? ToArray<uint>(stream) : ToScalar<uint>(stream);
                case Dtype.Uint64:  return value.shape.Length > 0 ? ToArray<ulong>(stream) : ToScalar<ulong>(stream);
                case Dtype.Bool:    return value.shape.Length > 0 ? ToArray<bool>(stream) : ToScalar<bool>(stream);
                case Dtype.Binary:  return stream;
                case Dtype.List:    return JsonConvert.DeserializeObject<List<object>>(new StreamReader(stream).ReadToEnd());
                case Dtype.Dict:    return JsonConvert.DeserializeObject<Dictionary<string, object>>(new StreamReader(stream).ReadToEnd());
                default:            return value;
            }
        }

        /// <summary>
        /// Create a Function prediction value from an input object.
        /// </summary>
        /// <param name="value">Input object.</param>
        /// <param name="name">Value name.</param>
        /// <param name="type">Value type. This only applies to `Stream` input values.</param>
        /// <param name="shape">Value shape for tensor values.</param>
        /// <param name="minUploadSize">Values larger than this size in bytes will be uploaded.</param>
        /// <returns>Function value.</returns>
        public async Task<Value> ToValue (
            object value,
            string name,
            Dtype? type = null,
            int[]? shape = null,
            int minUploadSize = 4096,
            string? key = null
        ) => value switch {
            Value       x => x,
            string      x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.String },
            float       x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Float32, shape = new int[0] },
            double      x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Float64, shape = new int[0] },
            sbyte       x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Int8, shape = new int[0] },
            short       x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Int16, shape = new int[0] },
            int         x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Int32, shape = new int[0] },
            long        x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Int64, shape = new int[0] },
            byte        x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Uint8, shape = new int[0] },
            ushort      x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Uint16, shape = new int[0] },
            uint        x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Uint32, shape = new int[0] },
            ulong       x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Uint64, shape = new int[0] },
            bool        x => new Value { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Bool, shape = new int[0] },
            float[]     x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Float32, shape = shape ?? new int[] { x.Length } },
            double[]    x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Float64, shape = shape ?? new int[] { x.Length } },
            sbyte[]     x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Int8, shape = shape ?? new int[] { x.Length } },
            short[]     x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Int16, shape = shape ?? new int[] { x.Length } },
            int[]       x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Int32, shape = shape ?? new int[] { x.Length } },
            long[]      x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Int64, shape = shape ?? new int[] { x.Length } },
            byte[]      x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Uint8, shape = shape ?? new int[] { x.Length } },
            ushort[]    x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Uint16, shape = shape ?? new int[] { x.Length } },
            uint[]      x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Uint32, shape = shape ?? new int[] { x.Length } },
            ulong[]     x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Uint64, shape = shape ?? new int[] { x.Length } },
            bool[]      x => new Value { data = await storage.Upload(name, ToStream(x), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Bool, shape = shape ?? new int[] { x.Length } },
            Stream      x => new Value { data = await storage.Upload(name, x, UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Binary },
            IList       x => new Value { data = await storage.Upload(name, ToStream(JsonConvert.SerializeObject(x)), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.List },
            IDictionary x => new Value { data = await storage.Upload(name, ToStream(JsonConvert.SerializeObject(x)), UploadType.Value, dataUrlLimit: minUploadSize, key: key), type = Dtype.Dict },
            null          => new Value { type = Dtype.Null },
            _             => throw new InvalidOperationException($"Cannot create a Function value from value '{value}' of type {value.GetType()}"),
        };
        #endregion


        #region --Operations--
        private readonly IFunctionClient client;
        private readonly StorageService storage;
        private readonly Dictionary<string, IntPtr> cache;
        public const string Fields = @"
        id
        tag
        type
        created
        implementation
        configuration
        resources {
            id
            url
        }
        results {
            data
            type
            shape
        }
        latency
        error
        logs
        ";

        internal PredictionService (IFunctionClient client, StorageService storage) {
            this.client = client;
            this.storage = storage;
            this.cache = new Dictionary<string, IntPtr>();
        }

        private string ConfigurationId {
            get {
                var sb = new StringBuilder(2048);
                Function.GetConfigurationUniqueID(sb, sb.Capacity);
                return sb.ToString();
            }
        }

        private async Task<IntPtr> Load (Prediction prediction, Acceleration acceleration, IntPtr device) {
            // Create configuration
            Function.CreateConfiguration(out var configuration).CheckStatus();
            configuration.SetConfigurationToken(prediction.configuration).CheckStatus();
            configuration.SetConfigurationAcceleration(acceleration).CheckStatus();
            configuration.SetConfigurationDevice(device).CheckStatus();            
            await Task.WhenAll(prediction.resources.Select(async resource => {
                if (resource.id == @"fxn")
                    return;
                var path = await Retrieve(resource);
                lock (prediction)
                    configuration.SetConfigurationResource(resource.id, path).CheckStatus();
            }));
            // Create predictor
            Function.CreatePredictor(prediction.tag, configuration, out var predictor).CheckStatus();
            configuration.ReleaseConfiguration().CheckStatus();
            // Return
            return predictor;
        }

        private async Task<string> Retrieve (PredictionResource resource) {
            // Check cache
            Directory.CreateDirectory(client.CachePath);
            var path = Path.Combine(client.CachePath, resource.id); // INCOMPLETE // Different predictors and their tags
            if (File.Exists(path))
                return path;
            // Download
            using var dataStream = await client.Download(resource.url);
            using var fileStream = File.Create(path);
            if (client.ClientId == @"browser")
                dataStream.CopyTo(fileStream); // Workaround for lack of pthreads on browser
            else
                await dataStream.CopyToAsync(fileStream);
            // Return
            return path;
        }

        private Prediction Predict ( // INCOMPLETE // Latency, error, logs
            string tag,
            IntPtr predictor,
            Dictionary<string, object> inputs
        ) {
            // Marshal inouts
            Function.CreateValueMap(out var inputMap).CheckStatus();
            foreach (var pair in inputs)
                inputMap.SetValueMapValue(pair.Key, ToValue(pair.Value)).CheckStatus();
            // Predict
            predictor.Predict(inputMap, out var outputMap).CheckStatus();
            // Marshal outputs
            outputMap.GetValueMapSize(out var count).CheckStatus();
            var results = new List<object?>();
            var name = new StringBuilder(1024);
            for (var idx = 0; idx < count; ++idx) {
                name.Clear();
                outputMap.GetValueMapKey(idx, name, name.Capacity).CheckStatus();
                outputMap.GetValueMapValue(name.ToString(), out var value).CheckStatus();
                results.Add(ToObject(value));
            }
            // Create prediction
            var prediction = new Prediction {
                id = Guid.NewGuid().ToString("N"),
                tag = tag,
                type = PredictorType.Edge,
                created = DateTime.Now,
                results = results.ToArray(),
                latency = 0, 
                error = null,
                logs = null,
            };
            // Return
            return prediction;
        }

        private async Task<object?[]?> ParseResults (object[]? values, bool raw) {
            // Check
            if (values == null)
                return null;
            // Convert
            var results = await Task.WhenAll(values.Select(async r => {
                var value = (r as JObject).ToObject<Value>();
                return raw ? value : await ToObject(value);
            }));
            // Return
            return results;
        }

        private static unsafe IntPtr ToValue (object? value) {
            switch (value) {
                case float x:       return ToValue(&x);
                case double x:      return ToValue(&x);
                case sbyte x:       return ToValue(&x);
                case short x:       return ToValue(&x);   
                case int x:         return ToValue(&x);
                case long x:        return ToValue(&x);
                case byte x:        return ToValue(&x);
                case ushort x:      return ToValue(&x);
                case uint x:        return ToValue(&x);
                case ulong x:       return ToValue(&x);
                case bool x:        return ToValue(&x);
                case float[] x:     return ToValue(x);
                case double[] x:    return ToValue(x);
                case sbyte[] x:     return ToValue(x);
                case short[] x:     return ToValue(x);   
                case int[] x:       return ToValue(x);
                case long[] x:      return ToValue(x);
                case byte[] x:      return ToValue(x);
                case ushort[] x:    return ToValue(x);
                case uint[] x:      return ToValue(x);
                case ulong[] x:     return ToValue(x);
                case bool[] x:      return ToValue(x);
                case string x:
                    Function.CreateStringValue(x, out var str).CheckStatus();
                    return str;
                case IList x:
                    Function.CreateListValue(JsonConvert.SerializeObject(x), out var list).CheckStatus();
                    return list;
                case IDictionary x:
                    Function.CreateListValue(JsonConvert.SerializeObject(x), out var dict).CheckStatus();
                    return dict;
                case Stream stream:
                    Function.CreateBinaryValue(stream.ToArray(), stream.Length, ValueFlags.CopyData, out var binary).CheckStatus();
                    return binary;
                case null:
                    Function.CreateNullValue(out var nullptr).CheckStatus();
                    return nullptr;
                default: throw new InvalidOperationException($"Cannot create a Function value from value '{value}' of type {value.GetType()}");
            }
        }

        private static unsafe IntPtr ToValue<T> (T* data, int[] shape = null) where T : unmanaged {
            Function.CreateArrayValue(
                data,
                shape,
                shape?.Length ?? 0,
                typeof(T).ToDtype(),
                ValueFlags.CopyData,
                out var result
            ).CheckStatus();
            return result;
        }

        private static unsafe IntPtr ToValue<T> (T[] array) where T : unmanaged {
            fixed (T* data = array)
                return ToValue(data, new [] { array.Length });
        }

        private static object? ToObject (IntPtr value) { // INCOMPLETE
            // Null
            value.GetValueType(out var dtype).CheckStatus();
            if (dtype == Dtype.Null)
                return null;
            // Get data and shape
            value.GetValueData(out var data).CheckStatus();
            value.GetValueDimensions(out var dims).CheckStatus();
            var shape = new int[dims];
            value.GetValueShape(shape, dims).CheckStatus();
            // Deserialize
            

            return default;
        }

        private static Stream ToStream (string data) {
            var buffer = Encoding.UTF8.GetBytes(data);
            return new MemoryStream(buffer);
        }

        private static unsafe Stream ToStream<T> (T[] data) where T : unmanaged {
            if (data is byte[] raw)
                return new MemoryStream(raw);
            var size = data.Length * sizeof(T);
            var array = new byte[size];
            fixed (void* src = data, dst = array)
                Buffer.MemoryCopy(src, dst, size, size);
            return new MemoryStream(array);
        }

        private static unsafe T[] ToArray<T> (MemoryStream stream) where T : unmanaged {
            var rawData = stream.ToArray();
            var data = new T[rawData.Length / sizeof(T)];
            Buffer.BlockCopy(rawData, 0, data, 0, rawData.Length);
            return data;
        }

        private static T ToScalar<T> (MemoryStream stream) where T : unmanaged => ToArray<T>(stream)[0];
        #endregion


        #region --Types--

        private sealed class CreatePredictionInput {
            public string tag;
            public string client;
            public ValueInput[]? inputs;
            public int? dataUrlLimit;
            public string? configuration;
            public string? device;
        }

        private sealed class ValueInput : Value {
            public string name;
        }
        #endregion
    }
}