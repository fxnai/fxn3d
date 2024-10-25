/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Internal {

    using System.Collections.Generic;
    using UnityEngine;
    using CachedPrediction = API.PredictionCacheClient.CachedPrediction;

    /// <summary>
    /// Function settings for the current Unity project.
    /// </summary>
    [DefaultExecutionOrder(int.MinValue)]
    internal sealed class FunctionSettings : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// Project-wide access key.
        /// </summary>
        [field: SerializeField, HideInInspector]
        public string accessKey { get; private set; } = string.Empty;

        /// <summary>
        /// Predictor cache.
        /// </summary>
        [field: SerializeField, HideInInspector]
        public List<CachedPrediction> cache { get; internal set; } = new();

        /// <summary>
        /// Settings instance for this project.
        /// </summary>
        public static FunctionSettings Instance;

        /// <summary>
        /// Create Function settings.
        /// </summary>
        /// <param name="accessKey">Function access key.</param>
        public static FunctionSettings Create (string accessKey) {
            var settings = CreateInstance<FunctionSettings>();
            settings.accessKey = accessKey;
            return settings;
        }
        #endregion


        #region --Operations--

        private void Awake () {
            // Set singleton in player
            if (!Application.isEditor)
                Instance = this;
        }
        #endregion
    }
}