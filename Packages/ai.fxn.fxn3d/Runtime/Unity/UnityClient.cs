/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Newtonsoft.Json;
    using Internal;
    using Services;
    using Types;

    /// <summary>
    /// Function API client for Unity Engine.
    /// This uses Unity APIs for performing web requests.
    /// </summary>
    internal class UnityClient : IFunctionClient {

        #region --Client API--
        /// <summary>
        /// Create the client.
        /// </summary>
        /// <param name="url">Function API URL.</param>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="cache">Prediction cache.</param>
        public UnityClient (string url, string? accessKey) {
            this.url = url.TrimEnd('/');
            this.accessKey = accessKey;    
        }

        /// <summary>
        /// Perform a request to a Function REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request headers.</param>
        /// <returns>Deserialized response.</returns>
        public virtual async Task<T> Request<T> (
            string method,
            string path,
            object? payload = default,
            Dictionary<string, string>? headers = default
        ) {
            path = path.TrimStart('/');
            // Create client
            using var client = new UnityWebRequest($"{this.url}/{path}", method) {
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
            };
            // Add headers
            if (!string.IsNullOrEmpty(accessKey))
                client.SetRequestHeader(@"Authorization", $"Bearer {accessKey}");
            if (headers != null)
                foreach (var header in headers)
                    client.SetRequestHeader(header.Key, header.Value);
            // Add payload
            if (payload != null) {
                var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
                client.SetRequestHeader(@"Content-Type",  @"application/json");
                client.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadStr));
            }
            // Request
            client.SendWebRequest();
            while (!client.isDone)
                await Task.Yield();
            // Check error
            var responseStr = client.downloadHandler.text;
            if (client.responseCode >= 400) {
                var errorPayload = JsonConvert.DeserializeObject<ErrorResponse>(responseStr);
                throw new InvalidOperationException(errorPayload.errors[0].message);
            }
            // Return
            return JsonConvert.DeserializeObject<T>(responseStr);
        }

        /// <summary>
        /// Query the Function graph API.
        /// </summary>
        /// <param name="query">Graph query.</param>
        /// <param name="key">Query result key.</param>
        /// <param name="input">Query inputs.</param>
        public virtual async Task<T> Query<T> (
            string query,
            Dictionary<string, object?>? variables = default
        ) {
            var response = await Request<GraphResponse<T>>(
                @"POST",
                @"/graph",
                new GraphRequest { query = query, variables = variables }
            );
            return response.data;      
        }

        /// <summary>
        /// Perform a streaming request to a Function REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">POST request body.</param>
        /// <returns>Stream of deserialized responses.</returns>
        public virtual async IAsyncEnumerable<T> Stream<T> (string path, Dictionary<string, object> payload) {
            path = path.TrimStart('/');
            // Serialize payload
            var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
            // Create client
            var downloadHandler = new DownloadHandlerAsyncIterable();
            using var client = new UnityWebRequest($"{this.url}/{path}", UnityWebRequest.kHttpVerbPOST) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadStr)),
                downloadHandler = downloadHandler,
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
            };
            client.SetRequestHeader(@"Content-Type",  @"application/json");
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
        public virtual async Task<Stream> Download (string url) {
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
        public virtual async Task Upload (Stream stream, string url, string? mime = null) {
            // Create client
            using var client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT) {
                uploadHandler = new UploadHandlerRaw(stream.ToArray()),
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
        protected readonly string url;
        protected readonly string? accessKey;
        #endregion
    }
}