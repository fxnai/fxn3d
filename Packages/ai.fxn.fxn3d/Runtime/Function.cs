/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function {

    using System;
    using Graph;
    using Services;

    /// <summary>
    /// Function client.
    /// </summary>
    public sealed class Function {

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
        /// <param name="url">Function graph API URL.</param>
        public Function (string accessKey = null, string url = null) : this(new DotNetClient(url ?? URL, accessKey)) { }

        /// <summary>
        /// Create a Function client.
        /// </summary>
        /// <param name="client">Function graph API client.</param>
        public Function (IGraphClient client) {
            this.client = client;
            this.Storage = new StorageService(client);
            this.Users = new UserService(client);
            this.Predictors = new PredictorService(client);
            this.EnvironmentVariables = new EnvironmentVariableService(client);
            this.Predictions = new PredictionService(client, Storage);
        }
        #endregion


        #region --Operations--
        public readonly IGraphClient client;
        internal const string URL = @"https://api.fxn.ai/graph";
        #endregion
    }
}