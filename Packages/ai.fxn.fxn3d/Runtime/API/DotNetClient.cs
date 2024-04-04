/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
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

    /// <summary>
    /// Function API client for .NET.
    /// </summary>
    public sealed class DotNetClient : FunctionClient {

        #region --Client API--
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
            string? accessKey = default
        ) : base(url.TrimEnd('/'), accessKey) { }

        /// <summary>
        /// Make a request to a REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request headers.</param>
        /// <returns>Deserialized response.</returns>
        public override async Task<T?> Request<T> (
            string method,
            string path,
            object? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class {
            path = path.TrimStart('/');
            // Create client and message
            using var client = new HttpClient();
            using var message = new HttpRequestMessage(new HttpMethod(method), $"{this.url}/{path}");
            // Add headers
            if (!string.IsNullOrEmpty(accessKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Bearer", accessKey);
            if (headers != null)
                foreach (var header in headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
            // Add payload
            if (payload != null) {
                var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
                message.Content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            }
            // Request
            using var response = await client.SendAsync(message);
            var responseStr = await response.Content.ReadAsStringAsync();
            // Check error
            if ((int)response.StatusCode >= 400) {
                var errorPayload = JsonConvert.DeserializeObject<ErrorResponse>(responseStr);
                var error = errorPayload?.errors?[0]?.message ?? @"An unknown error occurred";
                throw new InvalidOperationException(error);
            }
            // Return
            return JsonConvert.DeserializeObject<T>(responseStr)!;
        }

        /// <summary>
        /// Make a request to a REST endpoint and consume the response as a stream.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request headers.</param>
        /// <returns>Stream of deserialized responses.</returns>
        public override async IAsyncEnumerable<T?> Stream<T> (
            string method,
            string path,
            object? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class {
            path = path.TrimStart('/');
            // Create client and message
            using var client = new HttpClient();
            using var message = new HttpRequestMessage(new HttpMethod(method), $"{this.url}/{path}");
            // Add headers
            if (!string.IsNullOrEmpty(accessKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(@"Bearer", accessKey);
            if (headers != null)
                foreach (var header in headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
            // Add payload
            if (payload != null) {
                var serializationSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
                message.Content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            }
            // Stream
            using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            var buffer = new byte[65536]; // CHECK // Could be problematic with `dataUrlLimit`
            while (true) {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                var responseStr = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if ((int)response.StatusCode >= 400) {
                    var errorPayload = JsonConvert.DeserializeObject<ErrorResponse>(responseStr);
                    var error = errorPayload?.errors?[0]?.message ?? @"An unknown error occurred";
                    throw new InvalidOperationException(error);
                }
                yield return JsonConvert.DeserializeObject<T>(responseStr)!;
            }
        }

        /// <summary>
        /// Query the Function graph API.
        /// </summary>
        /// <param name="query">Graph query.</param>
        /// <param name="key">Query result key.</param>
        /// <param name="input">Query inputs.</param>
        public override async Task<T?> Query<T> (
            string query,
            Dictionary<string, object?>? variables = default
        ) where T : class {
            var response = await Request<GraphResponse<T>>(
                @"POST",
                @"/graph",
                new GraphRequest { query = query, variables = variables }
            );
            return response!.data;
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">Data URL.</param>
        public override async Task<Stream> Download (string url) {
            // Create client
            using var client = new HttpClient();
            var ua = new ProductInfoHeaderValue(@"FunctionDotNet", Function.Version);
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
        public override async Task Upload (Stream stream, string url, string? mime = null) {
            using var client = new HttpClient();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(mime ?? @"application/octet-stream");
            using var response = await client.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        #endregion
    }
}