/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types.Converters {

    using System;
    using UnityEngine;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// JSON converter for Vector3.
    /// </summary>
    public class Vector3Converter : JsonConverter {

        private readonly Func<Vector3, Vector3>? transform;
        
        public Vector3Converter (Func<Vector3, Vector3>? transform = null) => this.transform = transform;

        public override bool CanConvert (Type objectType) => objectType == typeof(Vector3);

        public override object ReadJson (
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
        ) {
            var vector = reader.TokenType == JsonToken.StartObject ? ReadObject(reader) : ReadArray(reader);
            var result = transform?.Invoke(vector) ?? vector;
            return result;
        }

        public override void WriteJson (
            JsonWriter writer,
            object? value,
            JsonSerializer serializer
        ) {
            var vector = (Vector3)value!;
            var array = new JArray(vector.x, vector.y, vector.z);
            array.WriteTo(writer);
        }

        private Vector3 ReadObject (JsonReader reader) {
            var obj = JObject.Load(reader);
            return new Vector3(
                obj["x"]!.ToObject<float>(),
                obj["y"]!.ToObject<float>(),
                obj["z"]!.ToObject<float>()
            );
        }

        private Vector3 ReadArray (JsonReader reader) {
            var array = JArray.Load(reader);
            return new Vector3(
                array[0].ToObject<float>(),
                array[1].ToObject<float>(),
                array[2].ToObject<float>()
            );
        }
    }
}