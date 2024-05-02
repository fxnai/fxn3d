/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;
    using Types;

    internal sealed class ImageDeserializeTest : MonoBehaviour {

        [SerializeField] private Texture2D image;
        [SerializeField] private RawImage rawImage;

        private unsafe void Start () {
            IntPtr pngValue = default, imageValue = default;
            try {
                // Encode and decode
                var pngData = image.EncodeToPNG();
                Function.CreateBinaryValue(pngData, pngData.Length, Function.ValueFlags.CopyData, out pngValue).Throw();
                Function.CreateDeserializedValue(pngValue, Dtype.Image, Function.ValueFlags.None, out imageValue).Throw();
                // Get shape
                var shape = new int[3];
                Function.GetValueShape(imageValue, shape, shape.Length).Throw();
                Function.GetValueData(imageValue, out var imageData).Throw();
                Debug.Log($"Image shape: (" + string.Join(",", shape) + ")");
                // Create tex
                var result = new Texture2D(shape[1], shape[0], TextureFormat.RGBA32, false);
                var resultData = result.GetRawTextureData<byte>();
                UnsafeUtility.MemCpy(resultData.GetUnsafePtr(), (void*)imageData, result.width * result.height * 4);
                result.Apply();
                // Display
                rawImage.texture = result;
            } finally {
                pngValue.ReleaseValue();
                imageValue.ReleaseValue();
            }
        }
    }
}