/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.iOS.Xcode;
    using UnityEditor.iOS.Xcode.Extensions;
    using API;
    using Services;
    using Types;
    using CachedPrediction = Internal.FunctionSettings.CachedPrediction;
    

    internal sealed class macOSBuildHandler : BuildHandler, IPostprocessBuildWithReport {

        private List<CachedPrediction> cache;
        private static readonly string[] Platforms = new [] {
            "macos-arm64",
            "macos-x86_64"
        };

        protected override BuildTarget target => BuildTarget.StandaloneOSX;

        protected override Internal.FunctionSettings CreateSettings (BuildReport report) {
            // Create settings
            var settings =  FunctionProjectSettings.CreateSettings();
            // Embed predictors
            var embeds = GetEmbeds();
            var cache = new List<CachedPrediction>();
            foreach (var embed in embeds) {
                var embedFxn = embed.getFunction();
                var client = new DotNetClient(embedFxn.client.url, embedFxn.client.accessKey);
                var fxn = new Function(client);
                var predictions = (from tag in embed.tags from platform in Platforms select (platform, tag))
                    .Select((pair) => {
                        var (platform, tag) = pair;
                        var prediction = Task.Run(() => fxn.Predictions.Create(tag, rawOutputs: true, client: platform)).Result;
                        return prediction.type == PredictorType.Edge ?
                            new CachedPrediction { platform = platform, prediction = prediction } :
                            null;
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
            // Check if app or Xcode project
            var outputPath = report.summary.outputPath;
            var isApp = outputPath.EndsWith(@".app");
            var frameworkDir = isApp ?
                Path.Combine(outputPath, @"Contents", @"Frameworks") :
                Path.Combine(outputPath, Application.productName, @"Frameworks", @"Function");
            Directory.CreateDirectory(frameworkDir);
            // Embed
            var frameworks = new List<string>();
            var client = new DotNetClient(Function.URL);
            foreach (var cachedPrediction in cache) {
                var dso = cachedPrediction.prediction.resources.First(res => res.type == @"dso");
                var dsoName = PredictionService.GetResourceName(dso.url);
                var dsoPath = Path.Combine(frameworkDir, dsoName);
                using var dsoStream = Task.Run(async () => await client.Download(dso.url)).Result;
                using var fileStream = File.Create(dsoPath);
                dsoStream.CopyTo(fileStream);
                frameworks.Add(dsoName);
            }
            // Check Xcode project
            if (isApp)
                return;
            // Load Xcode project
            var projectName = new DirectoryInfo(outputPath).Name + ".xcodeproj";
            var pbxPath = Path.Combine(outputPath, projectName,  @"project.pbxproj");
            var project = new PBXProject();
            project.ReadFromFile(pbxPath);
            // Add frameworks
            var targetGuid = project.GetUnityMainTargetGuid();
            foreach (var framework in frameworks) {
                var frameworkGuid = project.AddFile(
                    $"{Application.productName}/Frameworks/Function/" + framework,
                    "Frameworks/" + framework,
                    PBXSourceTree.Source
                );
                project.AddFileToEmbedFrameworks(targetGuid, frameworkGuid);
            }
            // Write
            project.WriteToFile(pbxPath);
            // Empty cache
            cache = null;
        }
    }
}