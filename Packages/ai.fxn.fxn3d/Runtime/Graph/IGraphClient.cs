/*
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Graph {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Internal;

    /// <summary>
    /// Function graph API client.
    /// </summary>
    public interface IGraphClient {
        
        /// <summary>
        /// Client identifier.
        /// </summary>
        public string? Id { get; }

        /// <summary>
        /// Query the Function graph API.
        /// </summary>
        /// <param name="query">Graph query.</param>
        /// <param name="key">Query result key.</param>
        /// <param name="input">Query inputs.</param>
        Task<T?> Query<T> (string query, string key, Dictionary<string, object?>? variables = default);

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">URL</param>
        Task<MemoryStream> Download (string url);

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

        public Dictionary<string, T>? data;
        public Error[]? errors;

        public sealed class Error {
            public string message;
        }
    }
}