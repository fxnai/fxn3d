/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types {

    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Internal;

    /// <summary>
    /// Prediction.
    /// </summary>
    [Preserve, Serializable]
    public class Prediction {

        /// <summary>
        /// Prediction ID.
        /// </summary>
        public string id;

        /// <summary>
        /// Predictor tag.
        /// </summary>
        public string tag;
        
        /// <summary>
        /// Predictor type.
        /// </summary>
        public PredictorType type;

        /// <summary>
        /// Date created.
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime created;

        /// <summary>
        /// Prediction results.
        /// </summary>
        public object?[]? results;

        /// <summary>
        /// Prediction latency in milliseconds.
        /// </summary>
        public double? latency;

        /// <summary>
        /// Prediction error.
        /// This is `null` if the prediction completed successfully.
        /// </summary>
        public string? error;

        /// <summary>
        /// Prediction logs.
        /// </summary>
        public string? logs;

        /// <summary>
        /// Predictor resources.
        /// This is only populated for `EDGE` predictions.
        /// </summary>
        public PredictionResource[]? resources;

        /// <summary>
        /// Prediction configuration token.
        /// This is only populated for `EDGE` predictions.
        /// </summary>
        public string? configuration;
    }

    /// <summary>
    /// Prediction resource.
    /// </summary>
    [Preserve, Serializable]
    public class PredictionResource {

        /// <summary>
        /// Resource type.
        /// </summary>
        public string type;

        /// <summary>
        /// Resource URL.
        /// </summary>
        public string url;
    }
}