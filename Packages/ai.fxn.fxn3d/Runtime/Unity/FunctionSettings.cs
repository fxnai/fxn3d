/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System;
    using System.Collections.Generic;
    using UnityEngine;

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

        private void OnEnable () => Instance = this;
        #endregion
    }
}