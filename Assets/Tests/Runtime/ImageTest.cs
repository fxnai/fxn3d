/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using UnityEngine.UI;
    using Types;

    internal sealed class ImageTest : MonoBehaviour {

        [Header(@"Image")]
        [SerializeField] private Texture2D image;
        [SerializeField, Range(0f, 2f)] private float contrast = 1f;

        [Header(@"UI")]
        [SerializeField] private RawImage rawImage;

        private async void Start () {
            // Predict
            var fxn = FunctionUnity.Create(url: @"https://api.fxn.dev/graph");
            var prediction = await fxn.Predictions.Create(
                "@natml/auto-test-v1",
                new () {
                    ["image"] = await image.ToFeature(),
                    ["contrast"] = contrast
                }
            ) as CloudPrediction;
            // Display
            var result = prediction.results[0] as Feature;
            rawImage.texture = await result.ToTexture();
        }
    }
}