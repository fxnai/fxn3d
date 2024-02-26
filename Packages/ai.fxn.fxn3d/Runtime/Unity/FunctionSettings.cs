/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Types;

    /// <summary>
    /// Function settings for the current Unity project.
    /// </summary>
    [DefaultExecutionOrder(Int32.MinValue)]
    internal sealed class FunctionSettings : ScriptableObject {

        #region --Types--
        /// <summary>
        /// Cached prediction.
        /// </summary>
        [Serializable, Preserve]
        public sealed class CachedPrediction {
            public string platform;
            public Prediction prediction;
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Project-wide access key.
        /// </summary>
        [SerializeField, HideInInspector]
        internal string accessKey = string.Empty;

        /// <summary>
        /// Predictor cache.
        /// </summary>
        [SerializeField, HideInInspector]
        internal List<CachedPrediction> cache = new();

        /// <summary>
        /// Settings instance for this project.
        /// </summary>
        internal static FunctionSettings Instance;
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