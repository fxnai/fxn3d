/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Editor {

    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using Internal;

    [FilePath(@"ProjectSettings/Function.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class FunctionProjectSettings : ScriptableSingleton<FunctionProjectSettings> {

        #region --Data--
        [SerializeField]
        private string accessKey;
        #endregion


        #region --Client API--
        /// <summary>
        /// Function access key.
        /// </summary>
        internal string AccessKey {
            get => accessKey;
            set {
                accessKey = value;
                if (FunctionSettings.Instance)
                    FunctionSettings.Instance.accessKey = value;
                Save(false);
            }
        }

        /// <summary>
        /// Create Function settings from the current project settings.
        /// </summary>
        internal static FunctionSettings CreateSettings () {
            var settings = ScriptableObject.CreateInstance<FunctionSettings>();
            settings.accessKey = instance.AccessKey;
            return settings;
        }
        #endregion


        #region --Operations--

        [InitializeOnLoadMethod]
        private static void OnLoad () => FunctionSettings.Instance = CreateSettings();

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor (EnterPlayModeOptions options) => FunctionSettings.Instance = CreateSettings();
        #endregion
    }
}