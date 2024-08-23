/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System.Threading.Tasks;
    using API;
    using Types;

    /// <summary>
    /// Manage users.
    /// </summary>
    public sealed class UserService {

        #region --Client API--
        /// <summary>
        /// Retrieve the current user.
        /// </summary>
        public async Task<User?> Retrieve () {
            var response = await client.Query<UserResponse>(
                @$"query ($input: UserInput) {{
                    user (input: $input) {{
                        {ProfileFields}
                    }}
                }}"
            );
            return response!.user;
        }
        #endregion


        #region --Operations--
        private readonly FunctionClient client;
        private const string ProfileFields = @"
        username
        created
        name
        avatar
        bio
        website
        github
        ";

        internal UserService (FunctionClient client) => this.client = client;
        #endregion


        #region --Types--

        private sealed class UserResponse {
            public User? user;
            [Preserve] public UserResponse () { }
        }
        #endregion
    }
}