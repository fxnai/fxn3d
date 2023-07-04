/*
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Graph.Converters {

    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Types;

    internal class PredictionConverter : JsonConverter {

        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType) => objectType == typeof(Prediction);

        public override void WriteJson (
            JsonWriter writer,
            object? value,
            JsonSerializer serializer
        ) => throw new InvalidOperationException("Use default serialization.");

        public override object ReadJson (
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
        ) {
            var jsonObject = JObject.Load(reader);
            var type = jsonObject["type"].Value<string>();
            Prediction prediction = type == "CLOUD" ? new CloudPrediction() : new EdgePrediction();
            serializer.Populate(jsonObject.CreateReader(), prediction);
            return prediction;
        }
    }
}