/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

#nullable enable

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyCompany(@"NatML Inc")]
[assembly: AssemblyTitle(@"Function.Unity")]
[assembly: AssemblyVersion(Function.Function.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2025 NatML Inc. All Rights Reserved.")]
[assembly: InternalsVisibleTo(@"Function.Editor")]
[assembly: InternalsVisibleTo(@"Function.Tests.Editor")]
[assembly: InternalsVisibleTo(@"Function.Tests.Runtime")]

namespace Function {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Unity.Collections.LowLevel.Unsafe;
    using API;
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
        /// <param name="url">Function API URL.</param>
        /// <param name="cachePath">Predictor cache path.</param>
        /// <returns>Function client.</returns>
        public static Function Create (
            string? accessKey = null,
            string? url = null
        ) {
            var settings = FunctionSettings.Instance!;
            var client = new PredictionCacheClient(
                url ?? Function.URL,
                accessKey: accessKey ?? settings?.accessKey,
                cache: settings?.cache
            );
            var fxn = new Function(client);
            return fxn;
        }

        /// <summary>
        /// Convert a texture to an image.
        /// NOTE: The texture format must be `R8`, `Alpha8`, `RGB24`, or `RGBA32`.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        /// <param name="pixelBuffer">Pixel buffer to store image data. Use this to prevent allocations.</param>
        /// <returns>Image.</returns>
        public static unsafe Image ToImage (
            this Texture2D texture,
            byte[]? pixelBuffer = null
        ) {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            if (!texture.isReadable)
                throw new InvalidOperationException(@"Texture cannot be converted to a Function image because it is not readable");
            var FormatChannelMap = new Dictionary<TextureFormat, int> {
                [TextureFormat.R8] = 1,
                [TextureFormat.Alpha8] = 1,
                [TextureFormat.RGB24] = 3,
                [TextureFormat.RGBA32] = 4,
            };
            if (!FormatChannelMap.TryGetValue(texture.format, out var channels))
                throw new InvalidOperationException($"Texture cannot be converted to a Function image because it has unsupported format: {texture.format}");
            var rowStride = texture.width * channels;
            var bufferSize = rowStride * texture.height;
            pixelBuffer ??= new byte[bufferSize];
            if (pixelBuffer.Length != bufferSize)
                throw new InvalidOperationException($"Texture cannot be converted to a Function image because pixel buffer length was expected to be {bufferSize} but got {pixelBuffer.Length}");
            fixed (void* dst = pixelBuffer)
                UnsafeUtility.MemCpyStride(
                    dst,
                    rowStride,
                    (byte*)texture.GetRawTextureData<byte>().GetUnsafePtr() + (rowStride * (texture.height - 1)),
                    -rowStride,
                    rowStride,
                    texture.height
                );
            var image = new Image(pixelBuffer, texture.width, texture.height, channels);
            return image;
        }

        /// <summary>
        /// Convert an image to a texture.
        /// </summary>
        /// <param name="value">Image.</param>
        /// <param name="texture">Optional destination texture.</param>
        /// <returns>Texture.</returns>
        public static unsafe Texture2D ToTexture (
            this Image image,
            Texture2D? texture = null
        ) {
            var ChannelFormatMap = new Dictionary<int, TextureFormat> {
                [1] = TextureFormat.Alpha8,
                [3] = TextureFormat.RGB24,
                [4] = TextureFormat.RGBA32
            };
            if (!ChannelFormatMap.TryGetValue(image.channels, out var format))
                throw new InvalidOperationException($"Image cannot be converted to a Texture2D because it has unsupported channel count: {image.channels}");
            texture = texture != null ? texture : new Texture2D(image.width, image.height, format, false);
            if (texture.width != image.width || texture.height != image.height || texture.format != format)
                texture.Reinitialize(image.width, image.height, format, false);
            var rowStride = image.width * image.channels;
            fixed (byte* srcData = image)
                UnsafeUtility.MemCpyStride(
                    texture.GetRawTextureData<byte>().GetUnsafePtr(),
                    rowStride,
                    srcData + (rowStride * (image.height - 1)),
                    -rowStride,
                    rowStride,
                    image.height
                );
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Convert a `StreamingAssets` path to an absolute path accessible on the file system.
        /// This function will perform any necessary copying to ensure that the file is accessible.
        /// </summary>
        /// <param name="relativePath">Relative path to target file in `StreamingAssets` folder.</param>
        /// <returns>Absolute path to file or `null` if the file cannot be found.</returns>
        public static async Task<string?> StreamingAssetsToAbsolutePath (string relativePath) {
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            if (Application.platform != RuntimePlatform.Android)
                return File.Exists(fullPath) ? fullPath : null;
            var persistentPath = Path.Combine(Application.persistentDataPath, relativePath);
            if (File.Exists(persistentPath))
                return persistentPath;
            var directory = Path.GetDirectoryName(persistentPath);
            Directory.CreateDirectory(directory);
            using var request = UnityWebRequest.Get(fullPath);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                return null;
            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
            return persistentPath;
        }
        #endregion
    }
}