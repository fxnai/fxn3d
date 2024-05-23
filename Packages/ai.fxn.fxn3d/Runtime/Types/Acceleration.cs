/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Types {

    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Predictor acceleration.
    /// </summary>
    [Flags, JsonConverter(typeof(StringEnumConverter))]
    public enum Acceleration : int {
        /// <summary>
        /// Use the default acceleration for the given platform.
        /// This is only valid for `EDGE` predictors.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Predictions run on the CPU.
        /// This is valid for `EDGE` and `CLOUD` predictors.
        /// </summary>
        CPU = 1 << 0,
        /// <summary>
        /// Predictions run on the GPU.
        /// This is only valid for `EDGE` predictors.
        /// </summary>
        GPU = 1 << 1,
        /// <summary>
        /// Predictions run on the neural processor.
        /// This is only valid for `EDGE` predictors.
        /// </summary>
        NPU = 1 << 2,
        /// <summary>
        /// Predictions run on an Nvidia A40 GPU.
        /// This is only valid for `CLOUD` predictors.
        /// </summary>
        A40 = 1 << 12,
        /// <summary>
        /// Predictions run on an Nvidia A100 GPU.
        /// This is only valid for `CLOUD` predictors.
        /// </summary>
        A100 = 1 << 13,
    }
}