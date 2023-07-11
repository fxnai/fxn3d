/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Graph;
    using Types;

    /// <summary>
    /// Manage predictors.
    /// </summary>
    public sealed class PredictorService {

        #region --Client API--
        /// <summary>
        /// Retrieve a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        public Task<Predictor?> Retrieve (string tag) => client.Query<Predictor?>(
            @$"query ($input: PredictorInput!) {{
                predictor (input: $input) {{
                    {Fields}
                }}
            }}",
            @"predictor",
            new () {
                ["input"] = new PredictorInput { tag = tag }
            }
        );

        /// <summary>
        /// List the current user's predictors.
        /// </summary>
        /// <param name="owner">Predictor owner. This defaults to the current user.</param>
        /// <param name="status">Predictor status. This defaults to `ACTIVE`.</param>
        /// <param name="offset">Pagination offset.</param>
        /// <param name="count">Pagination count.</param>
        public async Task<Predictor[]?> List (
            string? owner = null,
            PredictorStatus? status = null,
            int? offset = null,
            int? count = null
        ) {
            var user = await client.Query<UserWithPredictors?>(
                @$"query ($user: UserInput, $predictors: UserPredictorsInput) {{
                    user (input: $user) {{
                        predictors (input: $predictors) {{
                            {Fields}
                        }}
                    }}
                }}",
                @"user",
                new () {
                    ["user"] = !string.IsNullOrEmpty(owner) ? new UserService.UserInput { username = owner } : null,
                    ["predictors"] = new UserPredictorsInput { status = status, offset = offset, count = count }
                }
            );
            return user?.predictors;
        }

        /// <summary>
        /// Search predictors.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <param name="offset">Pagination offset.</param>
        /// <param name="count">Pagination count.</param>
        public Task<Predictor[]> Search (
            string? query = null,
            int? offset = null,
            int? count = null
        ) => client.Query<Predictor[]>(
            @$"query ($input: PredictorsInput) {{
                predictors (input: $input) {{
                    {Fields}
                }}
            }}",
            @"predictors",
            new () {
                ["input"] = new PredictorsInput { query = query, offset = offset, count = count }
            }
        );

        /// <summary>
        /// Create a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="notebook">Predictor notebook URL.</param>
        /// <param name="type">Predictor type. This defaults to `CLOUD`.</param>
        /// <param name="access">Predictor access mode. This defaults to `PRIVATE`.</param>
        /// <param name="description">Predictor description. This must be under 200 characters long.</param>
        /// <param name="media">Predictor media path or URL.</param>
        /// <param name="acceleration">Predictor acceleration. This only applies for cloud predictors and defaults to `CPU`.</param>
        /// <param name="environment">Predictor environment variables.</param>
        /// <param name="license">Predictor license URL.</param>
        /// <param name="overwrite">Overwrite any existing predictor with the same tag. Existing predictor will be deleted.</param>
        /// <returns>Created predictor.</returns>
        public Task<Predictor> Create (
            string tag,
            string notebook,
            PredictorType? type = null,
            AccessMode? access = null,
            string? description = null,
            string? media = null,
            Acceleration? acceleration = null,
            Dictionary<string, string>? environment = null,
            string? license = null,
            bool? overwrite = null
        ) => client.Query<Predictor>(
            @$"mutation ($input: CreatePredictorInput!) {{
                createPredictor (input: $input) {{
                    {Fields}
                }}
            }}",
            @"createPredictor",
            new () {
                ["input"] = new CreatePredictorInput {
                    tag = tag,
                    notebook = notebook,
                    type = type,
                    access = access,
                    description = description,
                    media = media,
                    acceleration = acceleration,
                    environment = environment?.Select(pair => new EnvironmentVariableInput { name = pair.Key, value = pair.Value }).ToArray(),
                    license = license,
                    overwrite = overwrite
                }
            }
        );

        /// <summary>
        /// Delete a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <returns>Whether the predictor was successfully deleted.</returns>
        public Task<bool> Delete (string tag) => client.Query<bool>(
            @$"mutation ($input: DeletePredictorInput!) {{
                deletePredictor (input: $input)
            }}",
            @"deletePredictor",
            new () {
                ["input"] = new DeletePredictorInput { tag = tag }
            }
        );

        /// <summary>
        /// Archive a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <returns>Archived predictor.</returns>
        public Task<Predictor> Archive (string tag) => client.Query<Predictor>(
            @$"mutation ($input: ArchivePredictorInput!) {{
                archivePredictor (input: $input) {{
                    {Fields}
                }}
            }}",
            @"archivePredictor",
            new () {
                ["input"] = new ArchivePredictorInput { tag = tag }
            }
        );
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        public static string Fields = @$"
        tag
        owner {{
            {UserService.ProfileFields}
        }}
        name
        type
        status
        access
        predictions
        created
        description
        card
        media
        acceleration
        signature {{
            inputs {{
                name
                type
                description
                range
                optional
                enumeration {{
                    name
                    value
                }}
                defaultValue {{
                    data
                    type
                    shape
                }}
            }}
            outputs {{
                name
                type
                description
            }}
        }}
        error
        license
        ";

        internal PredictorService (IGraphClient client) => this.client = client;
        #endregion


        #region --Types--

        private sealed class PredictorInput {
            public string tag;
        }

        private sealed class PredictorsInput {
            public string? query;
            public int? offset;
            public int? count;
        }

        private sealed class UserPredictorsInput {
            public PredictorStatus? status;
            public int? offset;
            public int? count;
        }

        private sealed class CreatePredictorInput {
            public string tag;
            public string notebook;
            public PredictorType? type;
            public AccessMode? access;
            public string? description;
            public string? media;
            public Acceleration? acceleration;
            public EnvironmentVariableInput[]? environment;
            public string? license;
            public bool? overwrite;
        }

        private sealed class DeletePredictorInput {
            public string tag;
        }

        private sealed class ArchivePredictorInput {
            public string tag;
        }

        private sealed class EnvironmentVariableInput {
            public string name;
            public string value;
        }

        private sealed class UserWithPredictors {
            public Predictor[] predictors;
        }
        #endregion
    }
}