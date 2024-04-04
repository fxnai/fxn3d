/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using UnityEditor;
    using UnityEditor.Build.Reporting;

    internal sealed class WindowsBuildHandler : BuildHandler {

        protected override BuildTarget target => BuildTarget.StandaloneWindows64;

        protected override Internal.FunctionSettings CreateSettings (BuildReport report) {
            // Create settings
            var settings = FunctionProjectSettings.CreateSettings();
            // Return
            return settings;
        }
    }
}