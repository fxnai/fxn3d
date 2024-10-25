/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using API;
    using FunctionSettings = Internal.FunctionSettings;
    using CachedPrediction = Internal.FunctionSettings.CachedPrediction;

#if UNITY_IOS
    using UnityEditor.iOS.Xcode;
    using UnityEditor.iOS.Xcode.Extensions;
#endif

    internal sealed class iOSBuildHandler : BuildHandler, IPostprocessBuildWithReport {

        private List<CachedPrediction> cache;
        private const string Platform = @"ios-arm64";

        protected override BuildTarget target => BuildTarget.iOS;

        protected override FunctionSettings CreateSettings (BuildReport report) {
            // Create settings
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            // Embed predictors
            var embeds = GetEmbeds();
            var cache = new List<CachedPrediction>();
            foreach (var embed in embeds) {
                var client = new DotNetClient(embed.url, embed.accessKey);
                var fxn = new Function(client);
                var predictions = embed.tags
                    .Select(tag => {
                        try {
                            var prediction = Task.Run(() => fxn.Predictions.Create(tag, clientId: Platform, configurationId: @"")).Result;
                            return new CachedPrediction { platform = Platform, prediction = prediction };
                        } catch (Exception ex) {
                            Debug.LogWarning($"Function: Failed to embed {tag} with error: {ex.Message}. Edge predictions with this predictor will likely fail at runtime.");
                            return null;
                        }
                    })
                    .Where(pred => pred != null)
                    .ToArray();
                cache.AddRange(predictions);
            }
            // Cache
            settings.cache = cache;
            this.cache = cache;
            // Return
            return settings;
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) {
            // Check platform
            if (report.summary.platform != target)
                return;
            // Check cache
            if (cache == null)
                return;
            // Get frameworks path
            var frameworkDir = Path.Combine(report.summary.outputPath, @"Frameworks", @"Function");
            Directory.CreateDirectory(frameworkDir);
            // Copy dsos
            var client = new DotNetClient(Function.URL);
            var frameworks = new List<string>();
            foreach (var cachedPrediction in cache) {
                foreach (var resource in cachedPrediction.prediction.resources) {
                    // Check
                    if (resource.type != @"dso")
                        continue;
                    // Download
                    var dsoPath = Path.GetTempFileName();
                    {
                        using var dsoStream = Task.Run(async () => await client.Download(resource.url)).Result;
                        using var fileStream = File.Create(dsoPath);
                        dsoStream.CopyTo(fileStream);
                    }
                    ZipFile.ExtractToDirectory(dsoPath, frameworkDir, true);
                    frameworks.Add(resource.name);
                }
            }
        #if UNITY_IOS
            // Load Xcode project
            var pbxPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            var project = new PBXProject();
            project.ReadFromFile(pbxPath);
            // Add frameworks
            var targetGuid = project.GetUnityMainTargetGuid();
            foreach (var framework in frameworks) {
                var frameworkGuid = project.AddFile(
                    "Frameworks/Function/" + framework,
                    "Frameworks/" + framework,
                    PBXSourceTree.Source
                );
                project.AddFileToEmbedFrameworks(targetGuid, frameworkGuid);
            }
            // Write
            project.WriteToFile(pbxPath);
        #endif
            // Empty cache
            cache = null;
        }
    }
}