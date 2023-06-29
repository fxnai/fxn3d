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
    public sealed class EnvironmentVariableService {

        #region --Client API--
        /// <summary>
        /// List the current user's environment variables.
        /// </summary>
        /// <param name="organization">Organization username.</param>
        public async Task<EnvironmentVariable[]?> List (
            string? organization = null
        ) {
            var user = await client.Query<UserWithEnvironmentVariables?>(
                @$"query ($input: UserInput) {{
                    user (input: $input) {{
                        ... on User {{
                            environmentVariables {{
                                {Fields}
                            }}
                        }}
                        ... on Organization {{
                            environmentVariables {{
                                {Fields}
                            }}
                        }}
                    }}
                }}",
                @"user",
                new () {
                    ["input"] = !string.IsNullOrEmpty(organization) ? new UserService.UserInput { username = organization } : null,
                }
            );
            return user?.environmentVariables;
        }

        /// <summary>
        /// Create an environment variables.
        /// </summary>
        /// <param name="name">Environment variable name.</param>
        /// <param name="value">Environment variable value.</param>
        /// <param name="organization">Organization username.</param>
        public Task<EnvironmentVariable> Create (
            string name,
            string value,
            string? organization = null
        ) => client.Query<EnvironmentVariable>(
            @$"mutation ($input: CreateEnvironmentVariableInput!) {{
                createEnvironmentVariable (input: $input) {{
                    {Fields}
                }}
            }}",
            @"createEnvironmentVariable",
            new () {
                ["input"] = new CreateEnvironmentVariableInput { name = name, value = value, organization = organization }
            }
        );

        /// <summary>
        /// Delete an environment variables.
        /// </summary>
        /// <param name="name">Environment variable name.</param>
        /// <param name="organization">Organization username.</param>
        public Task<bool> Delete (
            string name,
            string? organization = null
        ) => client.Query<bool>(
            @$"mutation ($input: DeleteEnvironmentVariableInput!) {{
                deleteEnvironmentVariable (input: $input)
            }}",
            @"deleteEnvironmentVariable",
            new () {
                ["input"] = new DeleteEnvironmentVariableInput { name = name, organization = organization }
            }
        );
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

        private sealed class UserWithEnvironmentVariables {
            public EnvironmentVariable[] environmentVariables;
        }
        #endregion
    }
}