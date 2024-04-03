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
    /// JSON converter for Rect.
    /// </summary>
    public class RectConverter : JsonConverter {
        
        private static readonly string[] XMinAliases = new [] { "x", "xmin", "xMin", "x_min", "x1" };
        private static readonly string[] YMinAliases = new [] { "y", "ymin", "yMin", "y_min", "y1" };
        private static readonly string[] XMaxAliases = new [] { "xmax", "xMax", "x_max", "x2" };
        private static readonly string[] YMaxAliases = new [] { "ymax", "yMax", "y_max", "y2" };
        private static readonly string[] WidthAliases = new [] { "w", "width" };
        private static readonly string[] HeightAliases = new [] { "h", "height" };

        public override bool CanConvert (Type objectType) => objectType == typeof(Rect);

        public override object ReadJson (
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
        ) {
            var obj = JObject.Load(reader);
            // Get min point
            if (!TryGetValue(obj, XMinAliases, out var x1))
                throw new InvalidOperationException(@"Cannot deserialize `Rect` because x min could not be extracted");
            if (!TryGetValue(obj, YMinAliases, out var y1))
                throw new InvalidOperationException(@"Cannot deserialize `Rect` because y min could not be extracted");
            if (TryGetValue(obj, XMaxAliases, out var x2) && TryGetValue(obj, YMaxAliases, out var y2))
                return Rect.MinMaxRect(x1, y1, x2, y2);
            else if (TryGetValue(obj, WidthAliases, out var w) && TryGetValue(obj, HeightAliases, out var h))
                return new Rect(x1, y1, w, h);
            throw new InvalidOperationException(@"Cannot deserialize `Rect` because size could not be extracted");
        }

        public override void WriteJson (
            JsonWriter writer,
            object? value,
            JsonSerializer serializer
        ) {
            var rect = (Rect)value!;
            var obj = new JObject {
                ["x"] = rect.x,
                ["y"] = rect.y,
                ["width"] = rect.width,
                ["height"] = rect.height
            };
            obj.WriteTo(writer);
        }

        private bool TryGetValue (JObject obj, string[] aliases, out float value) {
            value = default;
            foreach (var alias in aliases)
                if (obj.TryGetValue(alias, out var token)) {
                    value = token.ToObject<float>();
                    return true;
                }
            return false;
        }
    }
}