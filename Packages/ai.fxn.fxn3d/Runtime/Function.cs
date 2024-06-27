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
        /// Embed predictors at build time to avoid errors due to sandboxing restrictions.
        /// NOTE: This is required to use edge predictors on Android and iOS.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
        public sealed class EmbedAttribute : Attribute {
            
            #region --Client API--
            /// <summary>
            /// Embed predictors at build time.
            /// </summary>
            public EmbedAttribute (params string[] tags) => this.tags = tags;
            #endregion


            #region --Operations--
            internal readonly string[] tags;
            internal Func<Function>? getFunction;
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
        /// <param name="cachePath">Predictor cache path.</param>
        public Function (
            string? accessKey = null,
            string? url = null,
            string? cachePath = null
        ) : this(new DotNetClient(url ?? URL, accessKey: accessKey), cachePath) { }

        /// <summary>
        /// Create a Function client.
        /// </summary>
        /// <param name="client">Function API client.</param>
        /// <param name="cachePath">Predictor cache path.</param>
        public Function (
            FunctionClient client,
            string? cachePath = null
        ) {
            this.client = client;
            this.Storage = new StorageService(client);
            this.Users = new UserService(client);
            this.Predictors = new PredictorService(client);
            this.EnvironmentVariables = new EnvironmentVariableService(client);
            this.Predictions = new PredictionService(client, Storage, cachePath);
        }
        #endregion


        #region --Operations--
        public readonly FunctionClient client;
        public const string Version = @"0.0.23";
        internal const string URL = @"https://api.fxn.ai";
        #endregion
    }
}