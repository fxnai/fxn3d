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
    using CachedPrediction = API.PredictionCacheClient.CachedPrediction;
    using FunctionSettings = Internal.FunctionSettings;

    internal sealed class AndroidBuildHandler : BuildHandler, IPostGenerateGradleAndroidProject {

        private static List<CachedPrediction> cache;
        private static readonly string[] ClientIds = new [] {
            "android-armeabi-v7a",
            "android-arm64-v8a",
            "android-x86_64"
        };

        protected override BuildTarget[] targets => new [] { BuildTarget.Android };

        protected override FunctionSettings CreateSettings (BuildReport report) {
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            var embeds = GetEmbeds();
            var cache = new List<CachedPrediction>();
            foreach (var embed in embeds) {
                var client = new DotNetClient(embed.url, embed.accessKey);
                var fxn = new Function(client);
                var predictions = (from tag in embed.tags from clientId in ClientIds select (clientId, tag))
                    .Select((pair) => {
                        var (clientId, tag) = pair;
                        try {
                            var prediction = Task.Run(() => fxn.Predictions.Create(
                                tag,
                                clientId: clientId,
                                configurationId: @""
                            )).Result;
                            var cached = CachedPrediction.FromPrediction(prediction);
                            cached.clientId = clientId;
                            return cached;
                        } catch (Exception ex) {
                            Debug.LogWarning($"Function: Failed to embed {tag} with error: {ex.Message}. Edge predictions with this predictor will likely fail at runtime.");
                            return null;
                        }
                    })
                    .Where(pred => pred != null)
                    .ToArray();
                cache.AddRange(predictions);
            }
            settings.cache = cache;
            AndroidBuildHandler.cache = cache;
            return settings;
        }

        void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject (string projectPath) {
            if (cache == null)
                return;
            foreach (var prediction in cache) {
                // Check
                var arch = prediction.clientId.Replace("android-", string.Empty).Replace(":", string.Empty);
                var libDir = Path.Combine(projectPath, @"src", @"main", @"jniLibs", arch);
                if (!Directory.Exists(libDir))
                    continue;
                // Fetch resources
                var client = new DotNetClient(Function.URL);
                var resources = prediction.resources.Where(res => res.type == @"dso");
                foreach (var resource in resources) {
                    var baseName = Path.GetFileName(PredictionService.GetResourcePath(resource, libDir));
                    var libName = $"lib{baseName}.so";
                    var path = Path.Combine(libDir, libName);
                    using var dsoStream = Task.Run(async () => await client.Download(resource.url)).Result;
                    using var fileStream = File.Create(path);
                    dsoStream.CopyTo(fileStream);
                }
            }
            cache = null;
        }
    }
}