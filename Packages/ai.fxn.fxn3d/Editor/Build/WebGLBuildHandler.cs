/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build.Reporting;

    internal sealed class WebGLBuildHandler : BuildHandler {

        private static string[] EM_ARGS => new [] {
            @"-Wl,--features=mutable-globals",
            @"-Wl,--export=__stack_pointer",
            @"-Wl,-u,FXN_WEBGL_INIT",
            @"-lembind",
            @"-sALLOW_TABLE_GROWTH=1",
            @"-sSTACK_OVERFLOW_CHECK=2",
            $"--embed-file {FxncPath}@libFunction.so"
        };
        private static string FxncPath => _FxncPath = _FxncPath ?? AssetDatabase.GetAllAssetPaths()
            .Select(path => new FileInfo(Path.GetFullPath(path)).FullName)
            .FirstOrDefault(path => path.EndsWith(@"Web/libFunction.so"));
        private static string _FxncPath;

        protected override BuildTarget target => BuildTarget.WebGL;

        protected override Internal.FunctionSettings CreateSettings (BuildReport report) {
            // Set Emscripten args
            PlayerSettings.WebGL.emscriptenArgs = GetEmscriptenArgs();
            UnityEngine.Debug.Log(PlayerSettings.WebGL.emscriptenArgs);
            // Create settings
            var settings = FunctionProjectSettings.CreateSettings();
            // Return
            return settings;
        }

        private static string GetEmscriptenArgs () {
            // Get current args
            var tokens = PlayerSettings.WebGL.emscriptenArgs 
                .Split(' ')
                .Select(arg => arg.Trim())
                .Where(arg => !string.IsNullOrEmpty(arg))
                .ToArray();
            var args = tokens
                .Aggregate(new List<string>(), (acc, current) => {
                    if (current.StartsWith("-"))
                        acc.Add(current);
                    else if (acc.Count > 0)
                        acc[acc.Count - 1] += " " + current;
                    return acc;
                })
                .Where(arg => !IsEmscriptenArgVolatile(arg));
            // Add new args
            var result = new HashSet<string>(args);
            result.UnionWith(EM_ARGS);
            // Return
            return string.Join(' ', result);
        }

        private static bool IsEmscriptenArgVolatile (string arg) {
            if (arg.StartsWith("--embed-file") && arg.Contains("libFunction.so"))
                return true;
            return false;
        }
    }
}