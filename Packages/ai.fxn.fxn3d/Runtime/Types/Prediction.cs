/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types {

    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Graph.Converters;
    using Internal;

    /// <summary>
    /// Prediction.
    /// </summary>
    [Preserve, Serializable, JsonConverter(typeof(PredictionConverter))]
    public abstract class Prediction {

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
        /// Date created
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime created;
    }

    /// <summary>
    /// Cloud prediction.
    /// </summary>
    [Preserve, Serializable]
    public class CloudPrediction : Prediction {
        
        /// <summary>
        /// Prediction results.
        /// </summary>
        public object[]? results;

        /// <summary>
        /// Prediction latency in milliseconds.
        /// </summary>
        public float? latency;

        /// <summary>
        /// Prediction error.
        /// This is `null` if the prediction completed successfully.
        /// </summary>
        public string? error;

        /// <summary>
        /// Prediction logs.
        /// </summary>
        public string? logs;
    }

    /// <summary>
    /// Edge prediction.
    /// </summary>
    [Preserve, Serializable]
    public class EdgePrediction : Prediction {

    }
}