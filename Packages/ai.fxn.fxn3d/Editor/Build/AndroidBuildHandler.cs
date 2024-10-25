/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Android;
    using UnityEditor.Build.Reporting;
    using API;
    using Services;
    using FunctionSettings = Internal.FunctionSettings;
    using CachedPrediction = Internal.FunctionSettings.CachedPrediction;

    internal sealed class AndroidBuildHandler : BuildHandler, IPostGenerateGradleAndroidProject {

        private static List<CachedPrediction> cache;
        private static readonly string[] Platforms = new [] {
            "android-armeabi-v7a",
            "android-arm64-v8a",
            "android-x86",
            "android-x86_64"
        };

        protected override BuildTarget target => BuildTarget.Android;

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
                var predictions = (from tag in embed.tags from platform in Platforms select (platform, tag))
                    .Select((pair) => {
                        var (platform, tag) = pair;
                        try {
                            var prediction = Task.Run(() => fxn.Predictions.Create(tag, clientId: platform, configurationId: @"")).Result;
                            return new CachedPrediction { platform = platform, prediction = prediction };
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
            AndroidBuildHandler.cache = cache;
            // Return
            return settings;
        }

        void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject (string projectPath) {
            // Check cache
            if (cache == null)
                return;
            // Embed
            foreach (var cachedPrediction in cache) {
                // Check
                var arch = cachedPrediction.platform.Replace("android-", string.Empty).Replace(":", string.Empty);
                var libDir = Path.Combine(projectPath, @"src", @"main", @"jniLibs", arch);
                if (!Directory.Exists(libDir))
                    continue;
                // Fetch resources
                var client = new DotNetClient(Function.URL);
                var resources = cachedPrediction.prediction.resources.Where(res => res.type == @"dso");
                foreach (var resource in resources) {
                    var libName = !string.IsNullOrEmpty(resource.name) ? resource.name : PredictionService.GetResourceName(resource.url);
                    var path = Path.ChangeExtension(Path.Combine(libDir, libName), @".so");
                    using var dsoStream = Task.Run(async () => await client.Download(resource.url)).Result;
                    using var fileStream = File.Create(path);
                    dsoStream.CopyTo(fileStream);
                }
            }
            // Unset cache
            cache = null;
        }
    }
}