/*
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Graph;

    /// <summary>
    /// Function API client.
    /// </summary>
    public interface IFunctionClient {

        /// <summary>
        /// Client identifier.
        /// </summary>
        public string? Id { get; }

        /// <summary>
        /// Query the Function graph API.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="query">Graph query.</param>
        /// <param name="key">Query result key.</param>
        /// <param name="input">Query inputs.</param>
        /// <returns>Deserialized query result.</returns>
        Task<T?> Query<T> (string query, string key, Dictionary<string, object?>? variables = default);

        /// <summary>
        /// Perform a streaming request to a Function REST endpoint.
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
        Task<MemoryStream> Download (string url);

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        Task Upload (Stream stream, string url, string? mime = null);
    }
}