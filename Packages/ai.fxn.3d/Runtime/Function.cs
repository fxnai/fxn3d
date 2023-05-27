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
        /// Function graph API URL.
        /// </summary>
        public const string URL = @"https://api.fxn.ai/graph";

        /// <summary>
        /// Manage users.
        /// </summary>
        public readonly UserService Users;

        /// <summary>
        /// Upload and download files.
        /// </summary>
        public readonly StorageService Storage;

        /// <summary>
        /// Create a Function client.
        /// </summary>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="url">Function graph API URL.</param>
        public Function (
            string accessKey = null,
            string url = null
        ) : this(new DotNetClient(url ?? URL, accessKey)) { }

        /// <summary>
        /// Create a Function client.
        /// </summary>
        /// <param name="client">Function graph API client.</param>
        public Function (IGraphClient client) {
            this.client = client;
            this.Users = new UserService(client);
            //this.Predictors = new PredictorService(client);
            //this.Graphs = new GraphService(client);
            //this.Endpoints = new EndpointService(client);
            //this.PredictorSessions = new PredictorSessionService(client);
            //this.EndpointPredictions = new EndpointPredictionService(client);
            this.Storage = new StorageService(client);
        }
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        #endregion
    }
}