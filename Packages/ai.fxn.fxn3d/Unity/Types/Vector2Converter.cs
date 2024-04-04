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
    /// JSON converter for Vector2.
    /// </summary>
    public class Vector2Converter : JsonConverter {
        
        private readonly Func<Vector2, Vector2>? transform;
        
        public Vector2Converter (Func<Vector2, Vector2>? transform = null) => this.transform = transform;

        public override bool CanConvert (Type objectType) => objectType == typeof(Vector2);

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
            var vector = (Vector2)value!;
            var array = new JArray(vector.x, vector.y);
            array.WriteTo(writer);
        }

        private Vector2 ReadObject (JsonReader reader) {
            var obj = JObject.Load(reader);
            return new Vector2(
                obj["x"]!.ToObject<float>(),
                obj["y"]!.ToObject<float>()
            );
        }

        private Vector2 ReadArray (JsonReader reader) {
            var array = JArray.Load(reader);
            return new Vector2(
                array[0].ToObject<float>(),
                array[1].ToObject<float>()
            );
        }
    }
}