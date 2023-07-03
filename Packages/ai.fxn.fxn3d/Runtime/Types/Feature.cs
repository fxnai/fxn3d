/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types {

    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Services;

    /// <summary>
    /// Prediction feature.
    /// </summary>
    [Preserve]
    public class Feature {

        /// <summary>
        /// Feature data URL.
        /// </summary>
        public string data;

        /// <summary>
        /// Feature data type.
        /// </summary>
        public Dtype type;

        /// <summary>
        /// Feature shape.
        /// This is `null` if shape information is not available or applicable.
        /// </summary>
        public int[]? shape;
    }
}