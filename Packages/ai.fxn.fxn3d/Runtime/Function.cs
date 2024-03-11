/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function {

    using System;
    using API;
    using Services;

    /// <summary>
    /// Function client.
    /// </summary>
    public sealed class Function {

        #region --Attributes--
        /// <summary>
        /// Embed an edge predictor at build time to improve initial prediction latency.
        /// Note that this is required to use edge predictors on Android and iOS.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
        public sealed class EmbedAttribute : Attribute {

            #region --Client API--
            /// <summary>
            /// Embed an edge predictor at build time.
            /// </summary>
            /// <param name="tag">Predictor tag.</param>
            /// <param name="accessKey">Function access key. If `null` the project access key will be used.</param>
            /// <param name="apiUrl">Function API URL.</param>
            public EmbedAttribute (string tag, string? accessKey = null, string? apiUrl = null) {
                this.tag = tag;
                this.accessKey = accessKey;
                this.apiUrl = apiUrl;
            }
            #endregion


            #region --Operations--
            internal readonly string tag;
            internal readonly string? accessKey;
            internal readonly string? apiUrl;
            #endregion
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Manage users.
        /// </summary>
        public readonly UserService Users;

        /// <summary>
        /// Manage predictors.
        /// </summary>
        public readonly PredictorService Predictors;

        /// <summary>
        /// Manage predictor environment variables.
        /// </summary>
        public readonly EnvironmentVariableService EnvironmentVariables;

        /// <summary>
        /// Make predictions.
        /// </summary>
        public readonly PredictionService Predictions;

        /// <summary>
        /// Upload and download files.
        /// </summary>
        public readonly StorageService Storage;

        /// <summary>
        /// Create a Function client.
        /// </summary>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="url">Function API URL.</param>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="cachePath">Predictor cache path.</param>
        public Function (
            string? accessKey = null,
            string? url = null,
            string? clientId = null,
            string? cachePath = null
        ) : this(new DotNetClient(url ?? URL, accessKey: accessKey), clientId, cachePath) { }

        /// <summary>
        /// Create a Function client.
        /// </summary>
        /// <param name="client">Function API client.</param>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="cachePath">Predictor cache path.</param>
        public Function (
            IFunctionClient client,
            string? clientId = null,
            string? cachePath = null
        ) {
            this.client = client;
            this.Storage = new StorageService(client);
            this.Users = new UserService(client);
            this.Predictors = new PredictorService(client);
            this.EnvironmentVariables = new EnvironmentVariableService(client);
            this.Predictions = new PredictionService(client, Storage, clientId, cachePath);
        }
        #endregion


        #region --Operations--
        public readonly IFunctionClient client;
        public const string Version = @"0.0.12";
        internal const string URL = @"https://api.fxn.ai";
        #endregion
    }
}