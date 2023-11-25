/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    [Function.Embed("@yusuf-delete/math", apiUrl: @"https://api.fxn.dev")]
    internal sealed class EdgeFunctionTest : MonoBehaviour {

        private async void Start () {
            // Create edge prediction
            var fxn = FunctionUnity.Create(url: @"https://api.fxn.dev");
            var prediction = await fxn.Predictions.Create(
                tag: "@yusuf-delete/math",
                inputs: new () { ["radius"] = 4 }
            );
            // Log
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
            // Delete
            var deleted = await fxn.Predictions.Delete("@yusuf-delete/math");
            Debug.Log($"Deleted predictor: {deleted}");
        }
    }
}