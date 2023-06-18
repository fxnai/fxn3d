/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Graph {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine.Networking;
    using Newtonsoft.Json;
    using Services;

    /// <summary>
    /// Function graph API client for .NET.
    /// This uses Unity APIs for performing web requests.
    /// As such it should be used on WebGL.
    /// </summary>
    public sealed class UnityGraphClient : IGraphClient {

        #region --Client API--
        /// <summary>
        /// Client identifier.
        /// </summary>
        public string? Id { get; private set; }

        /// <summary>
        /// Create the Unity web request client.
        /// </summary>
        /// <param name="url">Function graph API URL.</param>
        /// <param name="accessKey">Function access key.</param>
        public UnityGraphClient (string url, string? accessKey, string? id = null) {
            this.url = url;
            this.accessKey = accessKey;
            this.Id = id;
        }

        /// <summary>
        /// Query the Function graph API.
        /// </summary>
        /// <param name="query">Graph query.</param>
        /// <param name="key">Query result key.</param>
        /// <param name="input">Query inputs.</param>
        public async Task<T?> Query<T> (string query, string key, Dictionary<string, object?>? variables = default) {
            // Serialize payload
            var payload = new GraphRequest {
                query = query,
                variables = variables
            };
            var payloadStr = JsonConvert.SerializeObject(payload);
            // Create client
            using var client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadStr)),
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
            };
            client.SetRequestHeader(@"Content-Type",  @"application/json");
            // Add auth token
            if (!string.IsNullOrEmpty(accessKey))
                client.SetRequestHeader("Authorization", $"Bearer {accessKey}");
            // Post
            client.SendWebRequest();
            while (!client.isDone)
                await Task.Yield();
            // Parse
            var responseStr = client.downloadHandler.text;
            var response = JsonConvert.DeserializeObject<GraphResponse<T>>(responseStr);
            // Check error
            if (response.errors != null)
                throw new InvalidOperationException(response.errors[0].message);
            // Return
            return response.data.TryGetValue(key, out var value) ? value : default;                
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">URL</param>
        public async Task<MemoryStream> Download (string url) {
            using var request = UnityWebRequest.Get(url);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException(request.error);
            var data = request.downloadHandler.data;
            var stream = new MemoryStream(data, 0, data.Length, false, false);
            return stream;
        }

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        public async Task Upload (Stream stream, string url, string? mime = null) {
            // Create client
            using var client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT) {
                uploadHandler = new UploadHandlerRaw(StorageService.ReadStream(stream)),
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
            };
            client.SetRequestHeader(@"Content-Type", mime ?? @"application/octet-stream");
            // Put
            client.SendWebRequest();
            while (!client.isDone)
                await Task.Yield();
            // Check
            if (client.error != null)
                throw new InvalidOperationException(@"Failed to upload stream with error: {error}");
        }
        #endregion


        #region --Operations--
        private readonly string url;
        private readonly string? accessKey;
        #endregion
    }
}