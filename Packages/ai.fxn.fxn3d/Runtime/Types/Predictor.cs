/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Function.Types {

    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Prediction function.
    /// </summary>
    [Preserve, Serializable]
    public class Predictor {

        /// <summary>
        /// Predictor tag.
        /// </summary>
        public string tag;

        /// <summary>
        /// Predictor owner.
        /// </summary>
        public User owner;

        /// <summary>
        /// Predictor name.
        /// </summary>
        public string name;
        
        /// <summary>
        /// Predictor status.
        /// </summary>
        public PredictorStatus status;

        /// <summary>
        /// Number of predictions made with this predictor.
        /// </summary>
        public int predictions;

        /// <summary>
        /// Date created.
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime created;

        /// <summary>
        /// Predictor description.
        /// </summary>
        public string? description;

        /// <summary>
        /// Predictor card.
        /// </summary>
        public string? card;

        /// <summary>
        /// Predictor media URL.
        /// We encourage animated GIFs where possible.
        /// </summary>
        public string? media;

        /// <summary>
        /// Predictor signature.
        /// </summary>
        public Signature? signature;

        /// <summary>
        /// Predictor provisioning error.
        /// This is populated when the predictor status is `INVALID`.
        /// </summary>
        public string? error;

        /// <summary>
        /// Predictor license URL.
        /// </summary>
        public string? license;
    }

    /// <summary>
    /// Predictor signature.
    /// </summary>
    [Preserve, Serializable]
    public class Signature {

        /// <summary>
        /// Prediction inputs.
        /// </summary>
        public Parameter[] inputs;

        /// <summary>
        /// Prediction outputs.
        /// </summary>
        public Parameter[] outputs;
    }

    /// <summary>
    /// Predictor parameter.
    /// </summary>
    [Preserve, Serializable]
    public class Parameter {

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string? name;

        /// <summary>
        /// Parameter type.
        /// </summary>
        public Dtype? type;

        /// <summary>
        /// Parameter description.
        /// </summary>
        public string? description;

        /// <summary>
        /// Parameter is optional.
        /// </summary>
        public bool? optional;

        /// <summary>
        /// Parameter value range for numeric parameters.
        /// </summary>
        public float[]? range;

        /// <summary>
        /// Parameter value choices for enumeration parameters.
        /// </summary>
        public EnumerationMember[]? enumeration;

        /// <summary>
        /// Parameter default value.
        /// </summary>
        public object? defaultValue;
    }

    /// <summary>
    /// Prediction parameter enumeration member.
    /// </summary>
    [Preserve, Serializable]
    public class EnumerationMember {
        /// <summary>
        /// Enumeration member name.
        /// </summary>
        public string name;
        /// <summary>
        /// Enumeration member value.
        /// This is usually a `string` or `int`.
        /// </summary>
        public object value;
    }

    /// <summary>
    /// Access mode.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccessMode : int {
        /// <summary>
        /// Resource can be accessed by any user.
        /// </summary>
        [EnumMember(Value = @"PUBLIC")]
        Public = 0,
        /// <summary>
        /// Resource can only be accessed by the owner.
        /// </summary>
        [EnumMember(Value = @"PRIVATE")]
        Private = 1,
    }

    /// <summary>
    /// Predictor status.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PredictorStatus : int {
        /// <summary>
        /// Predictor is being provisioned.
        /// </summary>
        [EnumMember(Value = @"PROVISIONING")]
        Provisioning = 0,
        /// <summary>
        /// Predictor is active.
        /// </summary>
        [EnumMember(Value = @"ACTIVE")]
        Active = 1,
        /// <summary>
        /// Predictor is invalid.
        /// </summary>
        [EnumMember(Value = @"INVALID")]
        Invalid = 2,
        /// <summary>
        /// Predictor is archived.
        /// </summary>
        [EnumMember(Value = @"ARCHIVED")]
        Archived = 3,
    }
}