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
    using UnityEngine;
    using Services;
    using Types;

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
        public sealed class CachedPrediction : Prediction {
            public string? clientId;
            public string? configurationId;

            public static CachedPrediction FromPrediction (Prediction prediction) => new () {
                id = prediction.id,
                tag = prediction.tag,
                created = prediction.created,
                results = prediction.results,
                latency = prediction.latency,
                error = prediction.error,
                logs = prediction.logs,
                resources = prediction.resources,
                configuration = prediction.configuration,
            };
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
        public override async Task<T?> Request<T> (
            string method,
            string path,
            Dictionary<string, object?>? payload = default,
            Dictionary<string, string>? headers = default
        ) where T : class {
            if (method != @"POST" || path != @"/predictions" || payload == null)
                return await base.Request<T>(method, path, payload, headers);
            if (TryLoadPredictionFromCache(payload, out var pred))
                return pred as T;
            return await CreateAndCachePrediction(payload, headers) as T;
        }
        #endregion


        #region --Operations--
        private readonly List<CachedPrediction> cache;
        private static string CacheRoot => Application.isEditor ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".fxn") :
            Path.Combine(Application.persistentDataPath, @"fxn");
        private static string CachePath => Path.Combine(CacheRoot, @"cache");

        private bool TryLoadPredictionFromCache (
            Dictionary<string, object?> payload,
            out Prediction? prediction
        ) {
            prediction = null;
            var @partial = GetCachedPrediction(payload);
            if (@partial == null)
                return false;
            var configuration = PlayerPrefs.GetString(@partial.id, @"");
            if (string.IsNullOrEmpty(configuration))
                return false;
            var resources = @partial.resources.Select(res => new PredictionResource {
                type = res.type,
                url = $"file://{PredictionService.GetResourcePath(res, CachePath)}"
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

        private async Task<Prediction?> CreateAndCachePrediction ( // INCOMPLETE // Use the FS instead
            Dictionary<string, object?> payload,
            Dictionary<string, string>? headers
        ) {
            payload = new Dictionary<string, object?>(payload);
            var @partial = GetCachedPrediction(payload);
            if (@partial != null)
                payload.Add(@"predictionId", @partial.id);
            var prediction = await base.Request<Prediction>(@"POST", @"/predictions", payload, headers);
            if (prediction != null) {
                prediction.resources = await Task.WhenAll(prediction.resources.Select(GetCachedResource));
                if (@partial != null) {
                    PlayerPrefs.SetString(@partial.id, prediction.configuration);
                    PlayerPrefs.Save();
                }
            }
            return prediction;
        }

        private CachedPrediction? GetCachedPrediction (Dictionary<string, object?> payload) {
            var tag = payload.TryGetValue(@"tag", out var t) ? t as string : null;
            var clientId = payload.TryGetValue(@"clientId", out var id) ? id as string : null; 
            var prediction = cache.FirstOrDefault(p => p.tag == tag && p.clientId == clientId);
            return prediction;
        }

        private async Task<PredictionResource> GetCachedResource (PredictionResource resource) {
            var path = PredictionService.GetResourcePath(resource, CachePath);
            if (!File.Exists(path)) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var dataStream = await Download(resource.url);
                using var fileStream = File.Create(path);
                dataStream.CopyTo(fileStream);
            }
            return new PredictionResource { type = resource.type, url = $"file://{path}" };
        }
        #endregion
    }
}