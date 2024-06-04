/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Build.Reporting;

    internal sealed class WebGLBuildHandler : BuildHandler {

        protected override BuildTarget target => BuildTarget.WebGL;
        private static string[] EM_ARGS => new [] {
            @"-Xlinker --features=mutable-globals,sign-ext,simd128",
            @"-Wl,--export=__stack_pointer",
            @"-Wl,-uFXN_WEBGL_INIT",
            @"-lembind",
            @"-sALLOW_TABLE_GROWTH=1",
            @"-sSTACK_OVERFLOW_CHECK=2",
            $"--embed-file {FxncPath}@libFunction.so",
        };
        private static string FxncPath => GetNativeLibraryPath(@"Plugins/Web/libFunction.so", fullPath: true)!.Replace(@"@", @"@@");        
        private static string Function1JsPath => GetNativeLibraryPath(@"Plugins/Web/Function.1.jslib")!;
        private static string Function2JsPath => GetNativeLibraryPath(@"Plugins/Web/Function.2.jslib")!;
        private static string FunctionJsPath =>
        #if UNITY_2023_1_OR_NEWER
            Function2JsPath;
        #else
            Function1JsPath;
        #endif

        protected override Internal.FunctionSettings CreateSettings (BuildReport report) {
            // Set Emscripten args
            PlayerSettings.WebGL.emscriptenArgs = GetEmscriptenArgs();
            // Enable library
            foreach (var path in new [] { Function1JsPath, Function2JsPath }) {
                var importer = AssetImporter.GetAtPath(path) as PluginImporter;
                importer!.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                importer.SaveAndReimport();
            }
            var jsImporter = AssetImporter.GetAtPath(FunctionJsPath) as PluginImporter;
            jsImporter!.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
            jsImporter.SaveAndReimport();
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

        private static string? GetNativeLibraryPath (string prefix, bool fullPath = false) => AssetDatabase
            .GetAllAssetPaths()
            .Select(path => fullPath ? new FileInfo(Path.GetFullPath(path)).FullName : path)
            .FirstOrDefault(path => path.EndsWith(prefix));
    }
}