/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Android;
    using UnityEditor.Build.Reporting;
    using API;
    using Services;
    using Types;
    using CachedPrediction = Internal.FunctionSettings.CachedPrediction;

    internal sealed class AndroidBuildHandler : BuildHandler, IPostGenerateGradleAndroidProject {

        private static List<CachedPrediction> cache;
        private static readonly string[] Architectures = new [] { "armeabi-v7a", "arm64-v8a", "x86", "x86_64" };

        protected override BuildTarget target => BuildTarget.Android;

        protected override Internal.FunctionSettings CreateSettings (BuildReport report) {
            // Create settings
            var settings =  FunctionProjectSettings.CreateSettings();
            // Embed predictors
            var embeds = GetEmbeds();
            var cache = new List<CachedPrediction>();
            foreach (var embed in embeds) {
                // Create client
                var url = embed.apiUrl ?? Function.URL;
                var accessKey = embed.accessKey ?? settings.accessKey;
                var client = new DotNetClient(url, accessKey);
                // Embed
                foreach (var arch in Architectures) {
                    // Create prediction
                    var platform = $"android:{arch}";
                    var prediction = Task.Run(() => client.Request<Prediction>(
                        @"POST",
                        $"/predict/{embed.tag}",
                        headers: new () { [@"fxn-client"] = platform }
                    )).Result;
                    // Check type
                    if (prediction.type == PredictorType.Edge)
                        cache.Add(new CachedPrediction {
                            platform = platform,
                            prediction = prediction
                        });
                }
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
                var arch = cachedPrediction.platform.Split(':')[1];
                var libDir = Path.Combine(projectPath, @"src", @"main", @"jniLibs", arch);
                if (!Directory.Exists(libDir))
                    continue;
                // Fetch dso
                var client = new DotNetClient(Function.URL);
                var dso = cachedPrediction.prediction.resources.First(res => res.type == @"dso");
                var dsoName = PredictionService.GetResourceName(dso.url);
                var dsoPath = Path.Combine(libDir, $"{dsoName}.so");
                using var dsoStream = Task.Run(async () => await client.Download(dso.url)).Result;
                using var fileStream = File.Create(dsoPath);
                dsoStream.CopyTo(fileStream);
            }
            // Unset cache
            cache = null;
        }
    }
}