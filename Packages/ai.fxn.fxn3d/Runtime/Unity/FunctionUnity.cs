/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function {

    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Graph;
    using Internal;
    using Types;

    /// <summary>
    /// Utilities for working with Unity.
    /// </summary>
    public static class FunctionUnity {

        #region --Client API--
        /// <summary>
        /// Create a Function client for Unity.
        /// </summary>
        /// <param name="accessKey">Function access key. This defaults to your access key in Project Settings.</param>
        /// <returns>Function client.</returns>
        public static Function Create (string? accessKey = null, string? url = null) {
            var key = !string.IsNullOrEmpty(accessKey) ? accessKey : FunctionSettings.Instance.accessKey;
            var graph = url ?? Function.URL;
            var client = Application.platform == RuntimePlatform.WebGLPlayer ?
                (IGraphClient)new UnityGraphClient(graph, key, ClientId) :
                new DotNetClient(graph, key, ClientId);
            var fxn = new Function(client);
            return fxn;
        }

        /// <summary>
        /// Convert a texture to a prediction value.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        /// <param name="minUploadSize">Textures larger than this size in bytes will be uploaded.</param>
        /// <returns>Prediction value.</returns>
        public static async Task<Value> ToValue (this Texture2D texture, int minUploadSize = 4096) {
            // Check
            if (!texture || !texture.isReadable)
                throw new InvalidOperationException(@"Texture cannot be converted to a prediction value because it is not readable");
            // Encode
            var png = texture.format == TextureFormat.RGBA32;
            var imageData = png ? texture.EncodeToPNG() : texture.EncodeToJPG();
            var ext = png ? ".png" : ".jpg";
            // Upload
            var client = Create();
            using var stream = new MemoryStream(imageData);
            var value = await client.Predictions.ToValue(stream, $"image{ext}", Dtype.Image, minUploadSize: minUploadSize);
            // Return
            return value;
        }

        /// <summary>
        /// Convert an audio clip to a prediction value.
        /// </summary>
        /// <param name="clip">Input audio clip.</param>
        /// <param name="minUploadSize">Audio clips larger than this size in bytes will be uploaded.</param>
        /// <returns>Prediction value.</returns>
        public static async Task<Value> ToValue (this AudioClip clip, int minUploadSize = 4096) {
            using var stream = new MemoryStream();
            var sampleCount = clip.samples * clip.channels;
            var dataLength = 44 + sampleCount * sizeof(short) - 8;
            var sampleBuffer = new float[sampleCount];
            clip.GetData(sampleBuffer, 0);
            stream.Write(Encoding.UTF8.GetBytes(@"RIFF"), 0, 4);
            stream.Write(BitConverter.GetBytes(dataLength), 0, 4);
            stream.Write(Encoding.UTF8.GetBytes(@"WAVE"), 0, 4);
            stream.Write(Encoding.UTF8.GetBytes(@"fmt "), 0, 4);
            stream.Write(BitConverter.GetBytes(16), 0, 4);
            stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
            stream.Write(BitConverter.GetBytes(clip.channels), 0, 2);
            stream.Write(BitConverter.GetBytes(clip.frequency), 0, 4);
            stream.Write(BitConverter.GetBytes(clip.frequency * clip.channels * sizeof(short)), 0, 4);
            stream.Write(BitConverter.GetBytes((ushort)(clip.channels * 2)), 0, 2);
            stream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
            stream.Write(Encoding.UTF8.GetBytes(@"data"), 0, 4);
            stream.Write(BitConverter.GetBytes(sampleCount * sizeof(ushort)), 0, 4);
            unsafe {
                fixed (float* srcData = sampleBuffer)
                    fixed (short* dstData = new short[sampleCount]) {
                        for (var i = 0; i < sampleCount; ++i)
                            dstData[i] = (short)(srcData[i] * short.MaxValue);
                        using var dataStream = new UnmanagedMemoryStream((byte*)dstData, sampleCount * sizeof(short));
                        dataStream.CopyTo(stream);
                    }
            }
            // Upload
            stream.Seek(0, SeekOrigin.Begin);
            var client = Create();
            var value = await client.Predictions.ToValue(stream, @"audio.wav", Dtype.Audio, minUploadSize: minUploadSize);
            // Return
            return value;
        }

        /// <summary>
        /// Convert an image value to a texture.
        /// </summary>
        /// <param name="value">Prediction value.</param>
        /// <param name="texture">Optional destination texture.</param>
        /// <returns>Texture.</returns>
        public static async Task<Texture2D> ToTexture (this Value value, Texture2D texture = null) {
            // Check
            if (value?.type != Dtype.Image)
                throw new InvalidOperationException($"Value cannot be converted to a texture because it has an invalid type: {value?.type}");
            // Download
            var client = Create();
            using var stream = await client.Storage.Download(value.data);
            // Create
            texture = texture ? texture : new Texture2D(16, 16);
            texture.LoadImage(stream.ToArray());
            // Return
            return texture;
        }

        /// <summary>
        /// Convert an audio value to an AudioClip.
        /// </summary>
        /// <param name="value">Prediction value.</param>
        /// <returns>Audio clip.</returns>
        public static async Task<AudioClip> ToAudioClip (this Value value) {
            // Create download URL
            using var urlCreator = new DownloadUrlCreator(value.data);
            var url = await urlCreator.URL();
            // Download
            using var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            www.SendWebRequest();
            while (!www.isDone)
                await Task.Yield();
            // Create clip
            var clip = DownloadHandlerAudioClip.GetContent(www);
            return clip;
        }

        /// <summary>
        /// Convert a `StreamingAssets` path to an absolute path accessible on the file system.
        /// This function will perform any necessary copying to ensure that the file is accessible.
        /// </summary>
        /// <param name="relativePath">Relative path to target file in `StreamingAssets` folder.</param>
        /// <returns>Absolute path to file or `null` if the file cannot be found.</returns>
        public static async Task<string?> StreamingAssetsToAbsolutePath (string relativePath) {
            // Check persistent
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            // Handle other platform
            if (Application.platform != RuntimePlatform.Android)
                return File.Exists(fullPath) ? fullPath : null;
            // Check persistent
            var persistentPath = Path.Combine(Application.persistentDataPath, relativePath);
            if (File.Exists(persistentPath))
                return persistentPath;
            // Create directories
            var directory = Path.GetDirectoryName(persistentPath);
            Directory.CreateDirectory(directory);
            // Download from APK/AAB
            using var request = UnityWebRequest.Get(fullPath);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                return null;
            // Copy
            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
            // Return
            return persistentPath;
        }
        #endregion


        #region --Operations--
        /// <summary>
        /// Function client identifier.
        /// </summary>
        internal static string ClientId => Application.platform switch {
            RuntimePlatform.Android         => @"android",
            RuntimePlatform.IPhonePlayer    => @"ios",
            RuntimePlatform.OSXEditor       => @"macos",
            RuntimePlatform.OSXPlayer       => @"macos",
            RuntimePlatform.WebGLPlayer     => @"browser",
            RuntimePlatform.WindowsEditor   => @"windows",
            RuntimePlatform.WindowsPlayer   => @"windows",
            _                               => null,
        };

        private sealed class DownloadUrlCreator : IDisposable {

            private readonly string url;
            private readonly string path;

            public DownloadUrlCreator (string url) {
                this.url = url;
                this.path = $"{Path.GetTempPath()}{Guid.NewGuid().ToString()}";
            }

            public async Task<string> URL () => url.StartsWith("data:") ? await CreateFileURL() : url;

            public void Dispose () => File.Delete(path);

            private async Task<string> CreateFileURL () {
                using var dataStream = await FunctionUnity.Create().Storage.Download(url);
                using var fileStream = new FileStream(path, FileMode.OpenOrCreate);
                dataStream.CopyTo(fileStream);
                return $"file://{path}";
            }
        }
        #endregion
    }
}