/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Types;

    internal sealed class ImageTest : MonoBehaviour {

        [Header(@"Image")]
        [SerializeField] private Texture2D image;

        [Header(@"UI")]
        [SerializeField] private UnityEngine.UI.RawImage rawImage;

        private async void Start () {
            // Predict
            var fxn = FunctionUnity.Create(url: @"https://api.fxn.dev");
            var prediction = await fxn.Predictions.Create(
                "@yusuf/image-identity",
                new () {
                    ["image"] = image.ToImage(),
                }
            );
            // Display
            var result = (Image)prediction.results[0];
            rawImage.texture = result.ToTexture();
        }
    }
}