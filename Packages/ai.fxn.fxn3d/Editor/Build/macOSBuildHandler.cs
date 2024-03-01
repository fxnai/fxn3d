/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using API;
    using Types;
    using CachedPrediction = Internal.FunctionSettings.CachedPrediction;

    internal sealed class macOSBuildHandler : BuildHandler, IPostprocessBuildWithReport { // INCOMPLETE

        private List<CachedPrediction> cache;
        private static readonly string[] Architectures = new [] { "arm64", "x86_64" };

        protected override BuildTarget target => BuildTarget.StandaloneOSX;

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
                    var platform = $"macos:{arch}";
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
            this.cache = cache;
            // Return
            return settings;
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) { // INCOMPLETE
            // Check platform
            if (report.summary.platform != target)
                return;

            UnityEngine.Debug.Log($"Build macOS: {report.summary.outputPath}");


            // Check cache
            if (cache == null)
                return;

        }
    }
}