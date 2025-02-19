/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
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
    using Types;
    using FunctionSettings = Internal.FunctionSettings;

#if UNITY_IOS || UNITY_VISIONOS
    using UnityEditor.iOS.Xcode;
    using UnityEditor.iOS.Xcode.Extensions;
#endif

    internal sealed class iOSBuildHandler : BuildHandler, IPostprocessBuildWithReport {

        private List<CachedPrediction> cache;
        private const string iOSClientId = @"ios-arm64";
        private const string visionOSClientId = @"visionos-arm64";

        protected override BuildTarget[] targets => new [] {
            BuildTarget.iOS,
            BuildTarget.VisionOS
        };

        protected override FunctionSettings CreateSettings (BuildReport report) {
            var projectSettings = FunctionProjectSettings.instance;
            var settings = FunctionSettings.Create(projectSettings.accessKey);
            var embeds = GetEmbeds();
            var cache = new List<CachedPrediction>();
            var clientId = report.summary.platform == BuildTarget.VisionOS ? visionOSClientId : iOSClientId;
            foreach (var embed in embeds) {
                var client = new DotNetClient(embed.url, embed.accessKey);
                var fxn = new Function(client);
                var predictions = embed.tags
                    .Select(tag => {
                        try {
                            var prediction = Task.Run(() => fxn.Predictions.Create(
                                tag,
                                clientId: clientId,
                                configurationId: @""
                            )).Result;
                            var cached = new CachedPrediction(prediction, clientId);
                            return cached;
                        } catch (Exception ex) {
                            Debug.LogException(new InvalidOperationException(
                                $"Function: Failed to embed {tag} predictor. Predictions with this predictor will likely fail at runtime.",
                                ex
                            ));
                            return null;
                        }
                    })
                    .Where(pred => pred != null)
                    .ToArray();
                cache.AddRange(predictions);
            }
            settings.cache = cache;
            this.cache = cache;
            return settings;
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) {
            if (!targets.Contains(report.summary.platform))
                return;
            if (cache == null)
                return;
            var frameworkDir = Path.Combine(report.summary.outputPath, @"Frameworks", @"Function");
            Directory.CreateDirectory(frameworkDir);
            var client = new DotNetClient(Function.URL);
            var frameworks = new List<string>();
            foreach (var prediction in cache)
                foreach (var resource in prediction.resources) {
                    try {
                        if (resource.type != @"dso")
                            continue;
                        var dsoPath = Path.GetTempFileName();
                        {
                            using var dsoStream = Task.Run(() => client.Download(resource.url)).Result;
                            using var fileStream = File.Create(dsoPath);
                            dsoStream.CopyTo(fileStream);
                        }
                        ZipFile.ExtractToDirectory(dsoPath, frameworkDir, true);
                        frameworks.Add(resource.name);
                    } catch (AggregateException ex) {
                        Debug.LogException(new InvalidOperationException(
                            $"Function: Failed to embed prediction resource for {prediction.tag} predictor. Predictions with this predictor will likely fail at runtime.",
                            ex.InnerException
                        ));
                    }
                }
        #if UNITY_IOS || UNITY_VISIONOS
            var pbxPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            var project = new PBXProject();
            project.ReadFromFile(pbxPath);
            var targetGuid = project.GetUnityMainTargetGuid();
            foreach (var framework in frameworks) {
                var frameworkGuid = project.AddFile(
                    "Frameworks/Function/" + framework,
                    "Frameworks/" + framework,
                    PBXSourceTree.Source
                );
                project.AddFileToEmbedFrameworks(targetGuid, frameworkGuid);
            }
            project.WriteToFile(pbxPath);
        #endif
            cache = null;
        }
    }
}