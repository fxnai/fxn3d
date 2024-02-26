/*
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Internal;
    using Types;

    /// <summary>
    /// Function API client.
    /// </summary>
    public interface IFunctionClient {

        /// <summary>
        /// Perform a request to a Function REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request body.</param>
        /// <returns>Deserialized response.</returns>
        Task<T> Request<T> (
            string method,
            string path,
            object? payload = default,
            Dictionary<string, string>? headers = default
        );

        /// <summary>
        /// Query the Function graph API.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="query">Graph query.</param>
        /// <param name="input">Query inputs.</param>
        /// <returns>Deserialized query result.</returns>
        Task<T> Query<T> (
            string query,
            Dictionary<string, object?>? variables = default
        );

        /// <summary>
        /// Perform a streaming POST request to a Function REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">POST request body.</param>
        /// <returns>Stream of deserialized responses.</returns>
        IAsyncEnumerable<T> Stream<T> (string path, Dictionary<string, object> payload);

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">URL</param>
        Task<Stream> Download (string url);

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        Task Upload (Stream stream, string url, string? mime = null);
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
        public T data;
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