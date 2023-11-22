/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Graph;
    using Types;

    /// <summary>
    /// Function API client for .NET.
    /// </summary>
    public sealed class DotNetClient : IFunctionClient {

        #region --Client API--
        /// <summary>
        /// Client identifier.
        /// </summary>
        public string? ClientId { get; private set; }
        
        /// <summary>
        /// Device model identifier.
        /// </summary>
        public string? DeviceId { get; private set; }

        /// <summary>
        /// Cache path.
        /// </summary>
        public string CachePath { get; private set; }

        /// <summary>
        /// Create the .NET Function API client.
        /// </summary>
        /// <param name="url">Function API URL.</param>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="deviceId">Device model identifier.</param>
        /// <param name="cachePath">Prediction resource cache path.</param>
        public DotNetClient (
            string url,
            string? accessKey,
            string? clientId = null,
            string? deviceId = null,
            string? cachePath = null
        ) {
            this.url = url;
            this.accessKey = accessKey;
            this.ClientId = clientId;
            this.DeviceId = deviceId;
            this.CachePath = cachePath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".fxn"
            );
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
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("fxn-client", ClientId);
            client.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(accessKey) ? new AuthenticationHeaderValue(@"Bearer", accessKey) : null;
            // Request
            using var content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            using var response = await client.PostAsync($"{this.url}/graph", content);
            // Parse
            var responseStr = await response.Content.ReadAsStringAsync();
            var responsePayload = JsonConvert.DeserializeObject<GraphResponse<T>>(responseStr);
            // Check error
            if (responsePayload.errors != null)
                throw new InvalidOperationException(responsePayload.errors[0].message);
            // Return
            return responsePayload.data.TryGetValue(key, out var value) ? value : default;                
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
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(@"fxn-client", ClientId);
            client.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(accessKey) ? new AuthenticationHeaderValue(@"Bearer", accessKey) : null;
            // Request
            using var content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            using var message = new HttpRequestMessage(HttpMethod.Post, $"{this.url}{path}");
            message.Content = content;
            using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            // Consume stream
            using var stream = await response.Content.ReadAsStreamAsync();
            var buffer = new byte[65536]; // CHECK // Could be problemati with `dataUrlLimit`
            while (true) {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                var responseStr = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if ((int)response.StatusCode >= 400) {
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
        /// <param name="url">Data URL.</param>
        public async Task<Stream> Download (string url) {
            // Create client
            using var client = new HttpClient();
            var ua = new ProductInfoHeaderValue("FunctionDotNet", "1.0");
            client.DefaultRequestHeaders.UserAgent.Add(ua);
            // Download
            var stream = await client.GetStreamAsync(url);
            return stream;
        }

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        public async Task Upload (Stream stream, string url, string? mime = null) {
            using var client = new HttpClient();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(mime ?? @"application/octet-stream");
            using var response = await client.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        #endregion


        #region --Operations--
        private readonly string url;
        private readonly string? accessKey;
        #endregion
    }
}