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
        /// <param name="inputs">Input features. Only applies to `CLOUD` predictions.</param>
        /// <param name="rawOutputs">Skip parsing output features into Pythonic data types.</param>
        /// <param name="dataUrlLimit">Return a data URL if a given output feature is smaller than this size. Only applies to `CLOUD` predictions.</param>
        public async Task<Prediction> Create (
            string tag,
            Dictionary<string, object>? inputs = null,
            bool rawOutputs = false,
            int? dataUrlLimit = null
        ) {
            // Collect inputs
            var key = Guid.NewGuid().ToString();
            var features = inputs != null ? await Task.WhenAll(inputs.Select(async pair => {
                var name = pair.Key;
                var feature = await ToFeature(pair.Value, name, key: key);
                return new FeatureInput { name = name, data = feature.data, type = feature.type, shape = feature.shape };
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
                        inputs = features,
                        dataUrlLimit = dataUrlLimit,
                    }
                }
            );
            // Collect results
            if (prediction is CloudPrediction cloudPrediction && cloudPrediction.results != null) {
                cloudPrediction.results = await Task.WhenAll(cloudPrediction.results.Select(async r => {
                    var feature = (r as JObject).ToObject<Feature>();
                    var result = rawOutputs ? feature : await ToValue(feature);
                    return result;
                }));
            }
            // Return
            return prediction;
        }

        /// <summary>
        /// Convert a feature to a plain C# value.
        /// </summary>
        public async Task<object> ToValue (Feature feature) => ToValue(feature, await storage.Download(feature.data));

        /// <summary>
        /// Create a feature from a given value.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="name">Feature name.</param>
        /// <param name="type">Feature type. Use this to override the default type that will be assigned to the feature.</param>
        /// <param name="minUploadSize">Features larger than this size in bytes will be uploaded.</param>
        /// <returns>Created feature.</returns>
        public async Task<Feature> ToFeature (object value, string name, Dtype? type = null, int minUploadSize = 4096, string? key = null) => value switch {
            Feature     x => x,
            string      x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.String },
            float       x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Float32, shape = new int[0] },
            double      x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Float64, shape = new int[0] },
            sbyte       x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Int8, shape = new int[0] },
            short       x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Int16, shape = new int[0] },
            int         x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Int32, shape = new int[0] },
            long        x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Int64, shape = new int[0] },
            byte        x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Uint8, shape = new int[0] },
            ushort      x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Uint16, shape = new int[0] },
            uint        x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Uint32, shape = new int[0] },
            ulong       x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Uint64, shape = new int[0] },
            bool        x => new Feature { data = await storage.Upload(name, ToStream(new [] { x }), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Bool, shape = new int[0] },
            float[]     x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Float32, shape = new int[] { x.Length } },
            double[]    x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Float64, shape = new int[] { x.Length } },
            sbyte[]     x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Int8, shape = new int[] { x.Length } },
            short[]     x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Int16, shape = new int[] { x.Length } },
            int[]       x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Int32, shape = new int[] { x.Length } },
            long[]      x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Int64, shape = new int[] { x.Length } },
            byte[]      x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Uint8, shape = new int[] { x.Length } },
            ushort[]    x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Uint16, shape = new int[] { x.Length } },
            uint[]      x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Uint32, shape = new int[] { x.Length } },
            ulong[]     x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Uint64, shape = new int[] { x.Length } },
            bool[]      x => new Feature { data = await storage.Upload(name, ToStream(x), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Bool, shape = new int[] { x.Length } },
            Stream      x => new Feature { data = await storage.Upload(name, x, UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Binary },
            IList       x => new Feature { data = await storage.Upload(name, ToStream(JsonConvert.SerializeObject(x)), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.List },
            IDictionary x => new Feature { data = await storage.Upload(name, ToStream(JsonConvert.SerializeObject(x)), UploadType.Feature, dataUrlLimit: minUploadSize, key: key), type = type ?? Dtype.Dict },
            _             => throw new InvalidOperationException($"Cannot create a feature from value '{value}' of type {value.GetType()}"),
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

        private object ToValue (Feature feature, MemoryStream stream) => feature.type switch {
            Dtype.String    => new StreamReader(stream).ReadToEnd(),
            Dtype.Float32   => feature.shape.Length > 0 ? ToArray<float>(stream) : ToScalar<float>(stream),
            Dtype.Float64   => feature.shape.Length > 0 ? ToArray<double>(stream) : ToScalar<double>(stream),
            Dtype.Int8      => feature.shape.Length > 0 ? ToArray<sbyte>(stream) : ToScalar<sbyte>(stream),
            Dtype.Int16     => feature.shape.Length > 0 ? ToArray<short>(stream) : ToScalar<short>(stream),
            Dtype.Int32     => feature.shape.Length > 0 ? ToArray<int>(stream) : ToScalar<int>(stream),
            Dtype.Int64     => feature.shape.Length > 0 ? ToArray<long>(stream) : ToScalar<long>(stream),
            Dtype.Uint8     => feature.shape.Length > 0 ? ToArray<byte>(stream) : ToScalar<byte>(stream),
            Dtype.Uint16    => feature.shape.Length > 0 ? ToArray<ushort>(stream) : ToScalar<ushort>(stream),
            Dtype.Uint32    => feature.shape.Length > 0 ? ToArray<uint>(stream) : ToScalar<uint>(stream),
            Dtype.Uint64    => feature.shape.Length > 0 ? ToArray<ulong>(stream) : ToScalar<ulong>(stream),
            Dtype.Bool      => feature.shape.Length > 0 ? ToArray<bool>(stream) : ToScalar<bool>(stream),
            Dtype.Binary    => stream,
            Dtype.List      => JsonConvert.DeserializeObject<List<object>>(new StreamReader(stream).ReadToEnd()),
            Dtype.Dict      => JsonConvert.DeserializeObject<Dictionary<string, object>>(new StreamReader(stream).ReadToEnd()),
            _               => feature,
        };

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
            public FeatureInput[]? inputs;
            public int? dataUrlLimit;
        }

        private sealed class FeatureInput : Feature {
            public string name;
        }
        #endregion
    }
}