/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using API;
    using Internal;
    using Types;
    using Unity.Collections.LowLevel.Unsafe;

    /// <summary>
    /// Utilities for working with Unity.
    /// </summary>
    public static class FunctionUnity {

        #region --Client API--
        /// <summary>
        /// Create a Function client for Unity.
        /// </summary>
        /// <param name="accessKey">Function access key. This defaults to your access key in Project Settings.</param>
        /// <param name="url">Function API URL.</param>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="cachePath">Predictor cache path.</param>
        /// <returns>Function client.</returns>
        public static Function Create (
            string? accessKey = null,
            string? url = null,
            string? clientId = null,
            string? cachePath = null
        ) {
            var settings = FunctionSettings.Instance!;
            var client = new PredictionCacheClient(
                url ?? Function.URL,
                accessKey: accessKey ?? settings?.accessKey,
                cache: settings?.cache
            );
            var fxn = new Function(
                client,
                clientId: clientId ?? ClientId,
                cachePath: cachePath ?? CachePath
            );
            return fxn;
        }

        /// <summary>
        /// Convert a texture to a Function image.
        /// This is useful for making edge predictions on images.
        /// NOTE: The texture format must be `R8`, `Alpha8`, `RGB24`, or `RGBA32`.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        /// <returns>Image.</returns>
        public static unsafe Image ToImage (this Texture2D texture) {
            // Check texture
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            // Check
            if (!texture.isReadable)
                throw new InvalidOperationException(@"Texture cannot be converted to a Function image because it is not readable");
            // Check format
            var FormatChannelMap = new Dictionary<TextureFormat, int> {
                [TextureFormat.R8] = 1,
                [TextureFormat.Alpha8] = 1,
                [TextureFormat.RGB24] = 3,
                [TextureFormat.RGBA32] = 4,
            };
            if (!FormatChannelMap.TryGetValue(texture.format, out var channels))
                throw new InvalidOperationException($"Texture cannot be converted ton a Function image because it has unsupported format: {texture.format}");
            // Flip vertical
            var rowStride = texture.width * channels;
            var pixelBuffer = new byte[rowStride * texture.height];
            fixed (void* dst = pixelBuffer)
                UnsafeUtility.MemCpyStride(
                    dst,
                    rowStride,
                    (byte*)texture.GetRawTextureData<byte>().GetUnsafePtr() + (rowStride * (texture.height - 1)),
                    -rowStride,
                    rowStride,
                    texture.height
                );
            // Create image
            var image = new Image(
                pixelBuffer,
                texture.width,
                texture.height,
                channels
            );
            // Return
            return image;
        }

        /// <summary>
        /// Convert a texture to a prediction value.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        /// <param name="minUploadSize">Textures larger than this size in bytes will be uploaded.</param>
        /// <returns>Prediction value.</returns>
        public static async Task<Value> ToValue (this Texture2D texture, int minUploadSize = 4096) {
            // Check texture
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            // Check readable
            if (!texture.isReadable)
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
        public static async Task<Texture2D> ToTexture (this Value value, Texture2D? texture = null) {
            // Check
            if (value?.type != Dtype.Image)
                throw new InvalidOperationException($"Value cannot be converted to a texture because it has an invalid type: {value?.type}");
            // Download
            var client = Create();
            using var stream = await client.Storage.Download(value.data!);
            // Create
            texture = texture != null ? texture : new Texture2D(16, 16);
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
            using var urlCreator = new DownloadUrlCreator(value.data!);
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
        /// Predictor cache path.
        /// </summary>
        internal static string CachePath => Path.Combine(Application.persistentDataPath, "fxn", "cache");

        /// <summary>
        /// Function client identifier.
        /// </summary>
        internal static string? ClientId {
            get {
                if (Application.platform == RuntimePlatform.Android)
                    switch (RuntimeInformation.ProcessArchitecture) {
                        case Architecture.Arm:          return "android:armeabi-v7a";
                        case Architecture.Arm64:        return "android:arm64-v8a";
                        case Architecture.X86:          return "android:x86";
                        case Architecture.X64:          return "android:x86_64";
                        default:                        return null;
                    }
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                    return "ios:arm64";
                if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
                    switch (RuntimeInformation.ProcessArchitecture) {
                        case Architecture.Arm64:    return "macos:arm64";
                        case Architecture.X64:      return "macos:x86_64";
                        default:                    return null;
                    }
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                    return @"browser";
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                    return @"windows:x86_64"; // assume no ARM support for now
                return null;
            }
        }

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