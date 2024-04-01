/*
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Function.API {

    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Internal;

    /// <summary>
    /// Function API client.
    /// </summary>
    public abstract class FunctionClient {

        #region --Client API--
        /// <summary>
        /// Function API URL.
        /// </summary>
        public readonly string url;

        /// <summary>
        /// Make a request to a REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request headers.</param>
        /// <returns>Deserialized response.</returns>
        public abstract Task<T?> Request<T> (
            string method,
            string path,
            object? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class;

        /// <summary>
        /// Make a request to a REST endpoint and consume the response as a stream.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request headers.</param>
        /// <returns>Stream of deserialized responses.</returns>
        public abstract IAsyncEnumerable<T?> Stream<T> (
            string method,
            string path,
            object? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class;

        /// <summary>
        /// Query the Function graph API.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="query">Graph query.</param>
        /// <param name="input">Query inputs.</param>
        /// <returns>Deserialized query result.</returns>
        public abstract Task<T?> Query<T> (
            string query,
            Dictionary<string, object?>? variables = default
        ) where T : class;

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">URL</param>
        public abstract Task<Stream> Download (string url);

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        public abstract Task Upload (Stream stream, string url, string? mime = null);
        #endregion


        #region --Operations--
        /// <summary>
        /// Function access key.
        /// </summary>
        protected internal readonly string? accessKey;
    
        protected FunctionClient (string url, string? accessKey) {
            this.url = url;
            this.accessKey = accessKey;
        }
        #endregion
    }

    /// <summary>
    /// Function graph API request.
    /// </summary>
    [Preserve]
    public sealed class GraphRequest {
        public string query = string.Empty;
        public Dictionary<string, object?>? variables;
    }

    /// <summary>
    /// Function graph API response.
    /// </summary>
    [Preserve]
    public sealed class GraphResponse<T> {
        public T? data;
    }

    /// <summary>
    /// Function API error response.
    /// </summary>
    [Preserve]
    public sealed class ErrorResponse {
        public Error[] errors;
        public sealed class Error {
            public string message;
        }
    }
}