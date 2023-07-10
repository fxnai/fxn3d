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
    using Graph;
    using Types;

    /// <summary>
    /// Make predictions.
    /// </summary>
    public sealed class PredictionService {

        #region --Client API--
        /// <summary>
        /// Create a prediction.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="inputs">Input values. Only applies to `CLOUD` predictions.</param>
        /// <param name="rawOutputs">Skip parsing output values into plain values.</param>
        /// <param name="dataUrlLimit">Return a data URL if a given output value is smaller than this size. Only applies to `CLOUD` predictions.</param>
        public async Task<Prediction> Create (
            string tag,
            Dictionary<string, object>? inputs = null,
            bool rawOutputs = false,
            int? dataUrlLimit = null
        ) {
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
                        client = client.Id,
                        inputs = values,
                        dataUrlLimit = dataUrlLimit,
                    }
                }
            );
            // Collect results
            if (prediction is CloudPrediction cloudPrediction && cloudPrediction.results != null) {
                cloudPrediction.results = await Task.WhenAll(cloudPrediction.results.Select(async r => {
                    var value = (r as JObject).ToObject<Value>();
                    var result = rawOutputs ? value : await ToObject(value);
                    return result;
                }));
            }
            // Return
            return prediction;
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
            int[] shape = null,
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
        private readonly IGraphClient client;
        private readonly StorageService storage;
        public const string Fields = @"
        id
        tag
        type
        created
        ... on CloudPrediction {
            results {
                data
                type
                shape
            }
            latency
            error
            logs
        }
        ";

        internal PredictionService (IGraphClient client, StorageService storage) {
            this.client = client;
            this.storage = storage;
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
        }

        private sealed class ValueInput : Value {
            public string name;
        }
        #endregion
    }
}