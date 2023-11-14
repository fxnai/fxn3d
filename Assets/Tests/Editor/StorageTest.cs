/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Unity.Collections.LowLevel.Unsafe;
    using Services;
    using Types;

    internal sealed class StorageTest {

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create(url: @"https://api.fxn.dev/graph");

        [Test(Description = @"Should create an upload URL")]
        public async Task CreateUploadURL () {
            var name = "stablediffusion.ipynb";
            var type = UploadType.Notebook;
            var url = await fxn.Storage.CreateUploadUrl(name, type);
            Assert.That(url, Does.StartWith("https://"));
        }

        [Test(Description = @"Should upload a file to a data URL")]
        public async Task UploadToDataURL () {
            // Create url
            using var stream = new FileStream("Assets/Media/cat.jpg", FileMode.Open);
            var url = await fxn.Storage.Upload("cat.jpg", stream, UploadType.Media, dataUrlLimit: 4 * 1024 * 1024);
            Assert.That(url, Does.StartWith("data:"));
            // Read file data
            stream.Seek(0, SeekOrigin.Begin);
            // Check
            var dataIdx = url.LastIndexOf(",") + 1;
            var b64Data = url.Substring(dataIdx);
            var urlData = Convert.FromBase64String(b64Data);
            unsafe {
                fixed (byte* fbuf = Internal.FunctionUtils.ToArray(stream), urlbuf = urlData)
                    Assert.That(UnsafeUtility.MemCmp(fbuf, urlbuf, stream.Length), Is.Zero);
            }
        }

        [Test(Description = @"Should upload a file to a web URL")]
        public async Task UploadToWebURL () {
            using var stream = new FileStream("Assets/Media/cat.jpg", FileMode.Open);
            var url = await fxn.Storage.Upload("cat.jpg", stream, UploadType.Media);
            Assert.That(url, Does.StartWith("https://"));
        }

        [Test(Description = @"Should download a file from a data URL")]
        public async Task DownloadFromDataURL () {
            // Create url
            using var stream = new FileStream("Assets/Media/cat.jpg", FileMode.Open);
            var url = await fxn.Storage.Upload("cat.jpg", stream, UploadType.Media, dataUrlLimit: 4 * 1024 * 1024);
            Assert.That(url, Does.StartWith("data:"));
            // Read file data
            stream.Seek(0, SeekOrigin.Begin);
            // Download
            var urlStream = await fxn.Storage.Download(url);
            var urlData = urlStream.ToArray();
            unsafe {
                fixed (byte* fbuf = Internal.FunctionUtils.ToArray(stream), urlbuf = urlData)
                    Assert.That(UnsafeUtility.MemCmp(fbuf, urlbuf, stream.Length), Is.Zero);
            }
        }

        [Test(Description = @"Should download a file from a web URL")]
        public async Task DownloadFromWebURL () {
            // Read file data
            using var fileStream = new FileStream("Assets/Media/cat.jpg", FileMode.Open);
            // Download
            var url = @"https://cdn.natml.ai/media/3e9e6c7a2fa114156c8b5f/cat.jpg";
            using var urlStream = await fxn.Storage.Download(url);
            var urlData = urlStream.ToArray();
            unsafe {
                fixed (byte* fbuf = Internal.FunctionUtils.ToArray(fileStream), urlbuf = urlData)
                    Assert.That(UnsafeUtility.MemCmp(fbuf, urlbuf, fileStream.Length), Is.Zero);
            }
        }
    }
}