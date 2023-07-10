/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types {

    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Internal;

    /// <summary>
    /// Prediction value.
    /// </summary>
    [Preserve, Serializable]
    public class Value {

        /// <summary>
        /// Value URL.
        /// </summary>
        public string? data;

        /// <summary>
        /// Value type.
        /// </summary>
        public Dtype type;

        /// <summary>
        /// Value shape.
        /// This is `null` if shape information is not available or applicable.
        /// </summary>
        public int[]? shape;
    }
}