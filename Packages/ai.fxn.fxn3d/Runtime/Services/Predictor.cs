/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Function.Services {

    using System.Threading.Tasks;
    using API;
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
        public async Task<Predictor?> Retrieve (string tag) {
            var response = await client.Query<PredictorResponse>(
                @$"query ($input: PredictorInput!) {{
                    predictor (input: $input) {{
                        {Fields}
                    }}
                }}",
                new () {
                    ["input"] = new PredictorInput { tag = tag }
                }
            );
            return response!.predictor;
        }
        #endregion


        #region --Operations--
        private readonly FunctionClient client;
        private static string Fields = @$"
        tag
        owner {{
            username
            created
            name
            avatar
            bio
            website
            github
        }}
        name
        status
        access
        predictions
        created
        description
        card
        media
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

        internal PredictorService (FunctionClient client) => this.client = client;
        #endregion


        #region --Types--

        private sealed class PredictorInput {
            public string tag;
        }

        private sealed class PredictorResponse {
            public Predictor? predictor;
            [Preserve] public PredictorResponse () { }
        }
        #endregion
    }
}