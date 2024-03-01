/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Editor.Build {

    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.iOS.Xcode;
    using UnityEditor.iOS.Xcode.Extensions;
    using API;
    using Types;
    using CachedPrediction = Internal.FunctionSettings.CachedPrediction;

    internal sealed class iOSBuildHandler : BuildHandler, IPostprocessBuildWithReport {

        private List<CachedPrediction> cache;
        private const string Platform = @"ios:arm64";

        protected override BuildTarget target => BuildTarget.iOS;

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
                // Create prediction
                var prediction = Task.Run(() => client.Request<Prediction>(
                    @"POST",
                    $"/predict/{embed.tag}",
                    headers: new () { [@"fxn-client"] = Platform }
                )).Result;
                // Add to settings
                if (prediction.type == PredictorType.Edge)
                    cache.Add(new CachedPrediction {
                        platform = Platform,
                        prediction = prediction
                    });
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
            var frameworkDir = Path.Combine(report.summary.outputPath, "Frameworks", "Function");
            Directory.CreateDirectory(frameworkDir);
            // Get dso
            var client = new DotNetClient(Function.URL);
            var frameworks = new List<string>();
            foreach (var cachedPrediction in cache) {
                var dso = cachedPrediction.prediction.resources.First(res => res.type == @"dso");
                var dsoPath = Path.GetTempFileName();
                {
                    using var dsoStream = Task.Run(async () => await client.Download(dso.url)).Result;
                    using var fileStream = File.Create(dsoPath);
                    dsoStream.CopyTo(fileStream);
                }
                ZipFile.ExtractToDirectory(dsoPath, frameworkDir, true);
                using var archive = ZipFile.Open(dsoPath, ZipArchiveMode.Read);
                var frameworkName = archive.Entries.First(e => !e.FullName.TrimEnd('/').Contains("/")).FullName.TrimEnd('/');
                frameworks.Add(frameworkName);
            }
            // Load Xcode project
            var pbxPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            var project = new PBXProject();
            project.ReadFromFile(pbxPath);
            // Add frameworks
            var targetGuid = project.GetUnityMainTargetGuid();
            foreach (var framework in frameworks) {
                var frameworkGuid = project.AddFile("Frameworks/Function/" + framework, "Frameworks/" + framework, PBXSourceTree.Source);
                project.AddFileToEmbedFrameworks(targetGuid, frameworkGuid);
            }
            // Write
            project.WriteToFile(pbxPath);
            // Empty cache
            cache = null;
        }
    }
}