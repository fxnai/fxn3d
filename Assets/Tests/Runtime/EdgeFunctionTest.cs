/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    [Function.Embed("@yusuf-delete/math", apiUrl: @"https://api.fxn.dev")]
    internal sealed class EdgeFunctionTest : MonoBehaviour {

        private Function fxn;

        private void Awake () => fxn = FunctionUnity.Create(url: @"https://api.fxn.dev");

        private async void Start () {
            // Create edge prediction
            var prediction = await fxn.Predictions.Create(
                tag: "@yusuf-delete/math",
                inputs: new () { ["radius"] = 4 }
            );
            // Log
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));            
        }

        private async void Update () {
            // Check
            if (!Input.GetKeyUp(KeyCode.Space))
                return;
            // Predict
            var prediction = await fxn.Predictions.Create(
                tag: "@yusuf-delete/math",
                inputs: new () { ["radius"] = 4 }
            );
            // Log
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
        }

        private async void OnDestroy () {
            var deleted = await fxn.Predictions.Delete("@yusuf-delete/math");
            Debug.Log($"Deleted predictor: {deleted}");
        }
    }
}