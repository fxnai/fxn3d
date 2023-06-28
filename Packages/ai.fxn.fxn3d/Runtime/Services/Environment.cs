/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Graph;
    using Types;

    /// <summary>
    /// Manage predictor environment variables.
    /// </summary>
    public sealed class EnvironmentVariableService { // INCOMPLETE

        #region --Client API--

        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        public static string Fields = @$"
        name
        ";

        internal EnvironmentVariableService (IGraphClient client) => this.client = client;
        #endregion


        #region --Types--

        private sealed class CreateEnvironmentVariableInput {
            public string name;
            public string value;
            public string? organization;
        }

        private sealed class DeleteEnvironmentVariableInput {
            public string name;
            public string? organization;
        }
        #endregion
    }
}