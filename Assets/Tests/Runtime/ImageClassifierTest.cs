/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    [Function.Embed(Tag)]
    internal sealed class ImageClassifierTest : MonoBehaviour {

        [Header(@"Image")]
        [SerializeField] private Texture2D image;

        private const string Tag = "@yusuf/mobilenet-v2";

        private async void Start () {
            var fxn = FunctionUnity.Create();
            var prediction = await fxn.Predictions.Create(
                tag: Tag,
                inputs: new() { ["image"] = image.ToImage() }
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, formatting: Formatting.Indented));
        }
    }
}