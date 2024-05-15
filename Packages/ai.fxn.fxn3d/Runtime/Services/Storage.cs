/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Function.Services {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using API;
    using Internal;
    using Types;

    /// <summary>
    /// Upload and download files.
    /// </summary>
    public sealed class StorageService {

        #region --Client API--
        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">Data URL.</param>
        public async Task<MemoryStream> Download (string url) {
            // Handle data URL
            if (url.StartsWith(@"data:")) {
                var dataIdx = url.LastIndexOf(",") + 1;
                var b64Data = url.Substring(dataIdx);
                var data = Convert.FromBase64String(b64Data);
                return new MemoryStream(data, 0, data.Length, false, false);
            }
            // Remote URL
            var dataStream = await client.Download(url);
            var memoryStream = new MemoryStream();
            await dataStream.CopyToAsync(memoryStream);
            // Return
            return memoryStream;
        }

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="stream">Data stream.</param>
        /// <param name="type">Upload type.</param>
        /// <param name="dataUrlLimit">Return a data URL if the provided stream is smaller than this limit (in bytes).</param>
        public async Task<string> Upload (
            string name,
            Stream stream,
            UploadType type,
            string? mime = null,
            int dataUrlLimit = 0,
            string? key = null
        ) {
            mime ??= @"application/octet-stream";
            // Data URL
            if (stream.Length < dataUrlLimit) {
                var data = Convert.ToBase64String(stream.ToArray());
                var result = $"data:{mime};base64,{data}";
                return result;
            }
            // Upload
            var url = await CreateUploadUrl(name, type, key: key);
            await client.Upload(stream, url, mime);
            // Return
            return url;
        }

        /// <summary>
        /// Create an upload URL.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="type">Upload type.</param>
        /// <param name="key">File key. This is useful for grouping related files.</param>
        public async Task<string> CreateUploadUrl (
            string name,
            UploadType type,
            string? key = null
        ) {
            var response = await client.Query<CreateUploadUrlResponse>(
                @$"mutation ($input: CreateUploadUrlInput!) {{
                    createUploadUrl (input: $input)
                }}",
                new () {
                    ["input"] = new CreateUploadUrlInput {
                        name = name,
                        type = type,
                        key = key
                    }
                }
            );
            return response!.createUploadUrl!;
        }
        #endregion


        #region --Operations--
        private readonly FunctionClient client;

        internal StorageService (FunctionClient client) => this.client = client;
        #endregion


        #region --Types--

        private sealed class CreateUploadUrlInput {
            public string name;
            public UploadType type;
            public string? key;
        }

        private sealed class CreateUploadUrlResponse {
            public string createUploadUrl;
            [Preserve] public CreateUploadUrlResponse () { }
        }
        #endregion
    }
}