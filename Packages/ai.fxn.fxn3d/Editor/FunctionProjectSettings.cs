/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor {

    using UnityEngine;
    using UnityEditor;
    using Internal;

    /// <summary>
    /// Function settings for the current Unity project.
    /// </summary>
    [FilePath(@"UserSettings/Function.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class FunctionProjectSettings : ScriptableSingleton<FunctionProjectSettings> {

        #region --Client API--
        public string accessKey;

        public void Save () => Save(false);
        #endregion


        #region --Operations--

        [InitializeOnLoadMethod]
        private static void OnLoad () => FunctionSettings.Instance = FunctionSettings.Create(instance.accessKey);

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor (EnterPlayModeOptions options) => FunctionSettings.Instance = FunctionSettings.Create(instance.accessKey);
        #endregion
    }
}