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
    /// JSON converter for Vector4.
    /// </summary>
    public class Vector4Converter : JsonConverter {

        private readonly Func<Vector4, Vector4>? transform;
        
        public Vector4Converter (Func<Vector4, Vector4>? transform = null) => this.transform = transform;

        public override bool CanConvert (Type objectType) => objectType == typeof(Vector4);

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
            var vector = (Vector4)value!;
            var array = new JArray(vector.x, vector.y, vector.z, vector.w);
            array.WriteTo(writer);
        }

        private Vector4 ReadObject (JsonReader reader) {
            var obj = JObject.Load(reader);
            return new Vector4(
                obj["x"]!.ToObject<float>(),
                obj["y"]!.ToObject<float>(),
                obj["z"]!.ToObject<float>(),
                obj["w"]!.ToObject<float>()
            );
        }

        private Vector4 ReadArray (JsonReader reader) {
            var array = JArray.Load(reader);
            return new Vector4(
                array[0].ToObject<float>(),
                array[1].ToObject<float>(),
                array[2].ToObject<float>(),
                array[3].ToObject<float>()
            );
        }
    }
}