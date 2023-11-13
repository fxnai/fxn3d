/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System.Threading.Tasks;
    using Types;

    /// <summary>
    /// Manage predictor resources.
    /// </summary>
    internal class ResourceService {

        #region --Client API--
        /// <summary>
        /// Create a prediction cache.
        /// </summary>
        public ResourceService () { // INCOMPLETE

        }

        /// <summary>
        /// Retrieve a predictor resource.
        /// </summary>
        /// <param name="resource">Prediction resource.</param>
        /// <returns>Resource path.</returns>
        public virtual async Task<string> Retrieve (PredictionResource resource) { // INCOMPLETE
            return default;
        }
        #endregion
    }
}