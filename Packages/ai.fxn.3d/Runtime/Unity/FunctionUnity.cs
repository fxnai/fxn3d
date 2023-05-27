/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function {

    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Graph;
    using Internal;

    /// <summary>
    /// Utilities for working with Unity.
    /// </summary>
    public static class FunctionUnity {

        /// <summary>
        /// Create a Function client that won't break on WebGL.
        /// </summary>
        /// <param name="accessKey">Function access key.</param>
        /// <returns>Function client.</returns>
        public static Function Create (string? accessKey = null, string? url = null) {
            url ??= Function.URL;
            accessKey = !string.IsNullOrEmpty(accessKey) ? accessKey : FunctionSettings.Instance.accessKey;
            IGraphClient graphClient = Application.platform == RuntimePlatform.WebGLPlayer ?
                new UnityGraphClient(url, accessKey) :
                new DotNetClient(url, accessKey);
            var client = new Function(graphClient);
            return client;
        }

        /// <summary>
        /// Convert a `StreamingAssets` path to an absolute path accessible on the file system.
        /// This function will perform any necessary copying to ensure that the file is accessible.
        /// </summary>
        /// <param name="relativePath">Relative path to target file in `StreamingAssets` folder.</param>
        /// <returns>Absolute path to file or `null` if the file cannot be found.</returns>
        public static async Task<string?> StreamingAssetsToAbsolutePath (string relativePath) {
            // Check persistent
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            // Handle other platform
            if (Application.platform != RuntimePlatform.Android)
                return File.Exists(fullPath) ? fullPath : null;
            // Check persistent
            var persistentPath = Path.Combine(Application.persistentDataPath, relativePath);
            if (File.Exists(persistentPath))
                return persistentPath;
            // Create directories
            var directory = Path.GetDirectoryName(persistentPath);
            Directory.CreateDirectory(directory);
            // Download from APK/AAB
            using var request = UnityWebRequest.Get(fullPath);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                return null;
            // Copy
            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
            // Return
            return persistentPath;
        }
    }
}