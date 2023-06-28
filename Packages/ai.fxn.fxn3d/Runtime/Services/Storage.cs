/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Services {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Graph;
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
            return await client.Download(url);
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
                var data = Convert.ToBase64String(ReadStream(stream));
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
        public Task<string> CreateUploadUrl (
            string name,
            UploadType type,
            string? key = null
        ) => client.Query<string>(
            @$"mutation ($input: CreateUploadUrlInput!) {{
                createUploadUrl (input: $input)
            }}",
            @"createUploadUrl",
            new () {
                ["input"] = new CreateUploadUrlInput {
                    name = name,
                    type = type,
                    key = key
                }
            }
        );
        #endregion


        #region --Operations--
        private readonly IGraphClient client;

        internal StorageService (IGraphClient client) => this.client = client;

        internal static byte[] ReadStream (Stream stream) {
            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();
            using (var dstStream = new MemoryStream()) {
                stream.CopyTo(dstStream);
                return dstStream.ToArray();
            }
        }
        #endregion


        #region --Types--

        private sealed class CreateUploadUrlInput {
            public string name;
            public UploadType type;
            public string? key;
        }
        #endregion
    }
}