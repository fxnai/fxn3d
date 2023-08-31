/*
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API.Graph {

    using System.Collections.Generic;
    using Internal;

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