/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System.Threading.Tasks;
    using Graph;
    using Types;

    /// <summary>
    /// Manage users.
    /// </summary>
    public sealed class UserService {

        #region --Client API--
        /// <summary>
        /// Retrieve the current user.
        /// </summary>
        public Task<User?> Retrieve () => Retrieve<User?>();

        /// <summary>
        /// Retrieve a user.
        /// </summary>
        /// <param name="username">Username.</param>
        public Task<Profile?> Retrieve (string username) => Retrieve<Profile?>(username);
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        public const string ProfileFields = @"
        username
        created
        name
        avatar
        bio
        website
        github
        ";
        public const string UserFields = @"
        ... on User {
            email
        }
        ";

        internal UserService (IGraphClient client) => this.client = client;
        
        private Task<T?> Retrieve<T> (string? username = null) => client.Query<T?>(
            @$"query ($input: UserInput) {{
                user (input: $input) {{
                    {ProfileFields}
                    {(string.IsNullOrEmpty(username) ? UserFields : string.Empty)}
                }}
            }}",
            @"user",
            new () {
                ["input"] = !string.IsNullOrEmpty(username) ? new UserInput { username = username } : null
            }
        );
        #endregion


        #region --Types--

        internal sealed class UserInput {
            public string username;
        }
        #endregion
    }
}