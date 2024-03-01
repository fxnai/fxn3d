/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using UnityEditor;
    using UnityEditor.Build.Reporting;

    internal sealed class WebGLBuildHandler : BuildHandler {

        private static readonly string[] EM_ARGS = new [] {
            @"-lembind",
            @"-sALLOW_TABLE_GROWTH=1",
        };

        protected override BuildTarget target => BuildTarget.WebGL;

        protected override Internal.FunctionSettings CreateSettings (BuildReport report) {
            // Set Emscripten args
            foreach (var arg in EM_ARGS) {
                var standaloneArg = $" {arg} ";
                if (!PlayerSettings.WebGL.emscriptenArgs.Contains(standaloneArg))
                    PlayerSettings.WebGL.emscriptenArgs += standaloneArg;
            }
            // Create settings
            var settings = FunctionProjectSettings.CreateSettings();
            // Return
            return settings;
        }
    }
}