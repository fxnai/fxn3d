/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Function settings for the current Unity project.
    /// </summary>
    [DefaultExecutionOrder(Int32.MinValue)]
    internal sealed class FunctionSettings : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// Project-wide access key.
        /// </summary>
        [SerializeField, HideInInspector]
        internal string accessKey = string.Empty;

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