/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Build.Reporting;

    internal sealed class WebGLBuildHandler : BuildHandler {

        private static string[] EM_ARGS => new [] {
            @"-Xlinker --features=mutable-globals,sign-ext,simd128",
            @"-Wl,--export=__stack_pointer",
            @"-Wl,-uFXN_WEBGL_INIT",
            @"-lembind",
            @"-sALLOW_TABLE_GROWTH=1",
            @"-sSTACK_OVERFLOW_CHECK=2",
            $"--embed-file {FxncPath}@libFunction.so",
        };
        private static string FxncPath => _FxncPath = _FxncPath ?? AssetDatabase.GetAllAssetPaths()
            .Select(path => new FileInfo(Path.GetFullPath(path)).FullName)
            .FirstOrDefault(path => path.EndsWith(@"Web/libFunction.so"))?
            .Replace(@"@", @"@@");
        private static string _FxncPath;

        protected override BuildTarget target => BuildTarget.WebGL;

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