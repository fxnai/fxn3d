/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Types {

    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Upload URL type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UploadType : int {
        /// <summary>
        /// Prediction feature.
        /// </summary>
        [EnumMember(Value = @"FEATURE")]
        Feature = 1,
        /// <summary>
        /// Predictor media.
        /// </summary>
        [EnumMember(Value = @"MEDIA")]
        Media = 2,
        /// <summary>
        /// Predictor notebook.
        /// </summary>
        [EnumMember(Value = @"NOTEBOOK")]
        Notebook = 3,
    }
}