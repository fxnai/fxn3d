/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using UnityEngine;
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
        public override async Task<T?> Request<T> (
            string method,
            string path,
            object? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class {
            path = path.TrimStart('/');
            // Check prediction
            if (!path.StartsWith(@"predict"))
                return await base.Request<T>(method, path, payload: payload, headers: headers);
            // Check tag
            var uri = new Uri($"{url}/{path}");
            var match = Regex.Match(uri.AbsolutePath, @".*(@[a-z1-9-]+\/[a-z1-9-]+).*");
            if (!match.Success)
                return await base.Request<T>(method, path, payload: payload, headers: headers);
            // Get cached prediction
            var tag = match.Groups[1].Value;
            var platform = headers != null && headers.TryGetValue(@"fxn-client", out var p) ? p : null;
            var cachedPrediction = cache.FirstOrDefault(p => p.prediction.tag == tag && p.platform == platform);
            if (cachedPrediction == null)
                return await base.Request<T>(method, path, payload: payload, headers: headers);
            // Predict
            var cachedToken = GetPredictionToken(cachedPrediction.prediction);
            if (cachedToken != null)
                headers?.Remove(@"fxn-configuration-token");
            var prediction = await base.Request<Prediction>(
                method,
                $"{path}{(path.Contains('?') ? '&' : '?')}partialPrediction={cachedPrediction.prediction.id}",
                payload: payload,
                headers: headers
            );
            // Save to cache
            if (prediction!.configuration != null)
                SavePredictionToken(cachedPrediction.prediction, prediction.configuration);
            else
                prediction.configuration = cachedToken;
            // Return
            return prediction as T;
        }
        #endregion


        #region --Operations--
        private readonly List<CachedPrediction> cache;

        private string? GetPredictionToken (Prediction prediction) {
            var key = GetCacheKey(prediction);
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
        }

        private void SavePredictionToken (Prediction prediction, string token) => PlayerPrefs.SetString(
            GetCacheKey(prediction),
            token
        );

        private string GetCacheKey (Prediction prediction) => $"ai.fxn.fxn3d.predictions.{prediction.id}";
        #endregion
    }
}