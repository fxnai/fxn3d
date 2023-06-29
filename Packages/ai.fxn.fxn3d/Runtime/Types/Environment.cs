/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types {

    /// <summary>
    /// Predictor environment variable.
    /// </summary>
    public class EnvironmentVariable {

        /// <summary>
        /// Environment variable name.
        /// </summary>
        public string name;

        /// <summary>
        /// Environment variable value.
        /// </summary>
        public string? value;
    }
}