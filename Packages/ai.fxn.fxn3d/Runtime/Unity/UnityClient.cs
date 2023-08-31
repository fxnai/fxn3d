/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine.Networking;
    using Newtonsoft.Json;
    using Graph;
    using Internal;
    using Services;

    /// <summary>
    /// Function API client for Unity Engine.
    /// This uses Unity APIs for performing web requests.
    /// </summary>
    public sealed class UnityClient : IFunctionClient {

        #region --Client API--
        /// <summary>
        /// Client identifier.
        /// </summary>
        public string? Id { get; private set; }

        /// <summary>
        /// Create the Unity Function API client.
        /// </summary>
        /// <param name="url">Function API URL.</param>
        /// <param name="accessKey">Function access key.</param>
        public UnityClient (string url, string? accessKey, string? id = null) {
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
            var payload = new GraphRequest { query = query, variables = variables };
            var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
            // Create client
            using var client = new UnityWebRequest($"{this.url}/graph", UnityWebRequest.kHttpVerbPOST) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadStr)),
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
            };
            client.SetRequestHeader(@"Content-Type",  @"application/json");
            client.SetRequestHeader(@"fxn-client", Id);
            if (!string.IsNullOrEmpty(accessKey))
                client.SetRequestHeader("Authorization", $"Bearer {accessKey}");
            // Request
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
        /// Perform a streaming request to a Function REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">POST request body.</param>
        /// <returns>Stream of deserialized responses.</returns>
        public async IAsyncEnumerable<T> Stream<T> (string path, Dictionary<string, object> payload) {
            // Serialize payload
            var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
            // Create client
            var downloadHandler = new DownloadHandlerAsyncIterable();
            using var client = new UnityWebRequest($"{this.url}{path}", UnityWebRequest.kHttpVerbPOST) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadStr)),
                downloadHandler = downloadHandler,
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
            };
            client.SetRequestHeader(@"Content-Type",  @"application/json");
            client.SetRequestHeader(@"fxn-client", Id);
            if (!string.IsNullOrEmpty(accessKey))
                client.SetRequestHeader(@"Authorization",  $"Bearer {accessKey}");
            // Request
            client.SendWebRequest();
            // Stream
            await foreach (var responseStr in downloadHandler.Stream()) {
                if (client.responseCode >= 400) {
                    var errorPayload = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseStr);
                    var error = errorPayload.TryGetValue(@"error", out var msg) ? msg : "An unknown error occurred";
                    throw new InvalidOperationException(error);
                }
                yield return JsonConvert.DeserializeObject<T>(responseStr);
            }
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