/*
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API.Graph {

    using System.Collections.Generic;
    using Internal;

    /// <summary>
    /// Function graph API request.
    /// </summary>
    [Preserve]
    public sealed class GraphRequest {

        public string query = string.Empty;
        public Dictionary<string, object?>? variables;
    }
}