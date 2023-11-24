/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Editor {

    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Internal;

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

        private static void ClearSettings () {
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList();
            if (assets != null) {
                assets.RemoveAll(asset => asset && asset.GetType() == typeof(FunctionSettings));
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
            AssetDatabase.DeleteAsset(CachePath);
        }
    }
}