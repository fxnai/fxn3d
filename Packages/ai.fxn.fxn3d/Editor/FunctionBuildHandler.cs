/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

using FunctionClient = Function.Function;
using EmbedAttribute = Function.Function.EmbedAttribute;

namespace Function.Editor {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using API;
    using Internal;
    using Services;
    using Types;

    internal sealed class FunctionBuildHandler : IPreprocessBuildWithReport, IPostprocessBuildWithReport {

        private const string CachePath = @"Assets/__FXN_DELETE_THIS__";
        private static readonly string[] EM_ARGS = new [] {
            @"-lembind",
            @"-sALLOW_TABLE_GROWTH=1",
        };

        int IOrderedCallback.callbackOrder => -1_000_000; // run very early, but not too early ;)

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            // Register failure listener
            EditorApplication.update += FailureListener;
            // WebGL
            if (report.summary.platform == BuildTarget.WebGL)
                SetWebGLArgs();
            // Embed settings
            EmbedSettings();
            // Embed predictors
            EmbedPredictors(report);
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) => ClearSettings();

        private void FailureListener () {
            if (BuildPipeline.isBuildingPlayer)
                return;
            EditorApplication.update -= FailureListener;
            (this as IPostprocessBuildWithReport).OnPostprocessBuild(null);
        }

        private static void SetWebGLArgs () {
            foreach (var arg in EM_ARGS) {
                var standaloneArg = $" {arg} ";
                if (!PlayerSettings.WebGL.emscriptenArgs.Contains(standaloneArg))
                    PlayerSettings.WebGL.emscriptenArgs += standaloneArg;
            }
        }

        private static void EmbedSettings () {
            // Clear
            ClearSettings();
            // Create asset
            var settings = FunctionProjectSettings.CreateSettings();
            Directory.CreateDirectory(CachePath);
            AssetDatabase.CreateAsset(settings, $"{CachePath}/Function.asset");
            // Add to build
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList() ?? new List<UnityEngine.Object>();
            assets.Add(settings);
            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        private static void EmbedPredictors (BuildReport report) { // INCOMPLETE
            // Get embeds
            var embeds = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly
                    .GetTypes()
                    .SelectMany(type => Attribute.GetCustomAttributes(type, typeof(EmbedAttribute)))
                )
                .Cast<EmbedAttribute>()
                .ToArray();
            var defaultAccessKey = FunctionProjectSettings.instance.AccessKey;
            // Embed
            var platforms = GetPlatforms(report.summary.platform);
            foreach (var embed in embeds) {
                // Create client
                var url = embed.apiUrl ?? FunctionClient.URL;
                var accessKey = embed.accessKey ?? defaultAccessKey;
                var client = new DotNetClient(url, accessKey);
                // Embed
                foreach (var platform in platforms) {
                    // Create prediction
                    var prediction = Task.Run(() => client.Request<Prediction>(
                        @"POST",
                        $"/predict/{embed.tag}",
                        headers: new () {
                            [@"fxn-client"] = platform,
                        }
                    )).Result;
                    // Check type
                    if (prediction.type != PredictorType.Edge)
                        continue;
                    // Embed resources
                    foreach (var resource in prediction.resources) {
                        if (!resource.type.StartsWith(@"dso"))
                            continue;
                        // Write resource
                        var resourceData = Task.Run(async () => (await client.Download(resource.url)).ToArray()).Result;
                        var resourceName = PredictionService.GetResourceName(resource.url);
                        var resourcePath = Path.Combine(CachePath, $"{resourceName}.so");
                        File.WriteAllBytes(resourcePath, resourceData);
                        // Import
                        AssetDatabase.Refresh();
                        // Get plugin importer
                        PluginImporter importer = PluginImporter.GetAtPath(resourcePath) as PluginImporter;
                        if (importer == null)
                            throw new InvalidOperationException($"Function failed to embed {embed.tag} resource {resourceName} because PluginImporter could not be retrieved");
                        //  Configure importer
                        importer.SetCompatibleWithAnyPlatform(false);
                        importer.SetCompatibleWithPlatform(report.summary.platform, true);
                        importer.SetPlatformData(report.summary.platform, @"CPU", ToAndroidArch(platform));
                        importer.SetPlatformData(report.summary.platform, @"AndroidSharedLibraryType", @"Executable");
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        private static void ClearSettings () {
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList();
            if (assets != null) {
                assets.RemoveAll(asset => asset && asset.GetType() == typeof(FunctionSettings));
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
            AssetDatabase.DeleteAsset(CachePath);
        }

        private static string[] GetPlatforms (BuildTarget target) => target switch {
            BuildTarget.Android             => new [] { @"android:armeabi-v7a", @"android:arm64-v8a", @"android:x86", @"android:x86_64" }, 
            BuildTarget.iOS                 => new [] { @"ios:arm64" },
            BuildTarget.StandaloneLinux64   => new [] { @"linux:x86_64" },
            BuildTarget.StandaloneOSX       => new [] { @"macos:arm64", @"macos:x86_64" },
            BuildTarget.StandaloneWindows64 => new [] { @"windows:x86_64" },
            BuildTarget.WebGL               => new [] { @"browser" },
            _                               => new string[0],
        };

        private static string ToAndroidArch (string resourceId) => resourceId switch {
            var x when x.Contains(@"armeabi-v7a")   => @"ARMV7",
            var x when x.Contains(@"arm64-v8a")     => @"ARM64",
            var x when x.Contains(@"x86")           => @"X86",
            var x when x.Contains(@"x86_64")        => @"X86_64",
            _                                       => null,
        };
    }
}