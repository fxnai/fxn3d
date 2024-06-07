/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Build.Reporting;

    internal sealed class WebGLBuildHandler : BuildHandler {

        protected override BuildTarget target => BuildTarget.WebGL;
        private static string[] EM_ARGS => new [] {
            @"-Wl,-uFXN_WEBGL_INIT",
            @"-Xlinker --features=mutable-globals,sign-ext,simd128",
            @"-lembind",
        };

        protected override Internal.FunctionSettings CreateSettings (BuildReport report) {
            // Set Emscripten args
            PlayerSettings.WebGL.emscriptenArgs = GetEmscriptenArgs();
            // Create settings
            var settings = FunctionProjectSettings.CreateSettings();
            // Return
            return settings;
        }

        private static string GetEmscriptenArgs () {
            var cleanedArgs = Regex.Replace(
                PlayerSettings.WebGL.emscriptenArgs,
                @"-Wl,-uFXN_WEBGL_PUSH.*?-Wl,-uFXN_WEBGL_POP",
                string.Empty,
                RegexOptions.Singleline
            ).Split(' ');
            var args = new List<string>();
            args.AddRange(cleanedArgs);
            args.Add(@"-Wl,-uFXN_WEBGL_PUSH");
            args.AddRange(EM_ARGS);
            args.Add(@"-Wl,-uFXN_WEBGL_POP");
            return string.Join(@" ", args);
        }
    }
}