/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using UnityEditor;
    using UnityEditor.Build.Reporting;

    internal sealed class LinuxBuildHandler : BuildHandler {

        protected override BuildTarget target => BuildTarget.StandaloneLinux64;

        protected override Internal.FunctionSettings CreateSettings (BuildReport report) {
            // Create settings
            var settings = FunctionProjectSettings.CreateSettings();
            // Return
            return settings;
        }
    }
}