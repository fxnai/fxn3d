/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Types;
    using CachedPrediction = Internal.FunctionSettings.CachedPrediction;

    /// <summary>
    /// Function API client for Unity Engine.
    /// This uses Unity APIs for performing web requests.
    /// Furthermore, this handles partial prediction caching for edge predictors.
    /// </summary>
    internal sealed class PredictionCacheClient : UnityClient {

        #region --Client API--
        /// <summary>
        /// Create the client.
        /// </summary>
        /// <param name="url">Function API URL.</param>
        /// <param name="accessKey">Function access key.</param>
        /// <param name="cache">Prediction cache.</param>
        public PredictionCacheClient (
            string url,
            string? accessKey,
            List<CachedPrediction>? cache = default
        ) : base(url, accessKey) => this.cache = cache ?? new();

        /// <summary>
        /// Perform a request to a Function REST endpoint.
        /// </summary>
        /// <typeparam name="T">Deserialized response type.</typeparam>
        /// <param name="method">HTTP request method.</param>
        /// <param name="path">Endpoint path.</param>
        /// <param name="payload">Request body.</param>
        /// <param name="headers">Request body.</param>
        /// <returns>Deserialized response.</returns>
        public override async Task<T?> Request<T> ( // DEPLOY
            string method,
            string path,
            Dictionary<string, object?>? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class {
            // Check prediction
            if (path != @"/predictions" || payload == null)
                return await base.Request<T>(method, path, payload, headers);
            // Check for cached prediction
            var tag = payload.TryGetValue(@"tag", out var t)  ? t as string : null;
            var clientId = payload.TryGetValue(@"clientId", out var id) ? id as string : null; 
            var cachedPrediction = cache.FirstOrDefault(p => p.prediction.tag == tag && p.platform == clientId);
            if (cachedPrediction == null)
                return await base.Request<T>(method, path, payload, headers);
            // Predict
            payload = new Dictionary<string, object?>(payload) {
                [@"predictionId"] = cachedPrediction.prediction.id
            };
            var prediction = await base.Request<Prediction>(method, path, payload, headers);
            // Return
            return prediction as T;
        }
        #endregion


        #region --Operations--
        private readonly List<CachedPrediction> cache;
        #endregion
    }
}