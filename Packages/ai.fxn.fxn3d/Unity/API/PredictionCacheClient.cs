/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.API {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Services;
    using Types;
    using UnityEngine;

    /// <summary>
    /// Function API client for Unity Engine.
    /// This uses Unity APIs for performing web requests.
    /// Furthermore, this handles partial prediction caching for edge predictors.
    /// </summary>
    internal sealed class PredictionCacheClient : UnityClient {

        #region --Types--
        /// <summary>
        /// Cached prediction.
        /// </summary>
        [Serializable, Preserve]
        public sealed class CachedPrediction {
            #pragma warning disable 8618
            public string clientId;
            public Prediction prediction;
            #pragma warning restore 8618
        }
        #endregion


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
            if (path != @"/predictions" || payload == null)
                return await base.Request<T>(method, path, payload, headers);
            if (TryLoadPredictionFromCache(payload, out var pred))
                return pred as T;
            return await CreatePredictionAndCache(payload) as T;
        }
        #endregion


        #region --Operations--
        private readonly List<CachedPrediction> cache;

        private bool TryLoadPredictionFromCache (
            Dictionary<string, object?> payload,
            out Prediction? prediction
        ) {
            prediction = null;
            var tag = payload.TryGetValue(@"tag", out var t) ? t as string : null;
            var clientId = payload.TryGetValue(@"clientId", out var id) ? id as string : null; 
            var entry = cache.FirstOrDefault(p => p.prediction.tag == tag && p.clientId == clientId);
            if (entry == null)
                return false;
            var @partial = entry.prediction;
            var configuration = PlayerPrefs.GetString(@partial.id, @"");
            if (string.IsNullOrEmpty(configuration))
                return false;
            var resources = @partial.resources.Select(res => new PredictionResource {
                type = res.type,
                url = $"file://{PredictionService.GetResourcePath(res, FunctionUnity.CachePath)}"
            }).ToArray();
            if (resources.Any(res => !File.Exists(new Uri(res.url).LocalPath)))
                return false;
            prediction = new Prediction {
                id = @partial.id,
                tag = @partial.tag,
                created = @partial.created,
                resources = resources,
                configuration = configuration
            };
            return true;
        }

        private async Task<Prediction> CreatePredictionAndCache (Dictionary<string, object?> payload) { // INCOMPLETE
            return null;


            /*
            var cachedPrediction = cache.FirstOrDefault(p => p.prediction.tag == tag && p.clientId == clientId);
            if (cachedPrediction == null)
                return await base.Request<T>(method, path, payload, headers);
            payload = new Dictionary<string, object?>(payload) {
                [@"predictionId"] = cachedPrediction.prediction.id
            };
            var prediction = await base.Request<Prediction>(method, path, payload, headers);
            return prediction as T;
            */
        }
        #endregion
    }
}