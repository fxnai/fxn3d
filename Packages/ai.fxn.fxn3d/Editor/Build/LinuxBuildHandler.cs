/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using FunctionSettings = Internal.FunctionSettings;

    internal sealed class LinuxBuildHandler : BuildHandler {

        protected override BuildTarget target => BuildTarget.StandaloneLinux64;

        protected override FunctionSettings CreateSettings (BuildReport report) {
            // Create settings
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            // Return
            return settings;
        }
    }
}