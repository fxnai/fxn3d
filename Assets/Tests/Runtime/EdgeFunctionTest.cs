/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Newtonsoft.Json;
    using Stopwatch = System.Diagnostics.Stopwatch;

    [Function.Embed(Tag)]
    internal sealed class EdgeFunctionTest : MonoBehaviour {

        private Function fxn;
        private const string Tag = "@yusuf/circle-area";

        private void Awake () => fxn = FunctionUnity.Create();

        private void Start () => Predict();

        private void Update () {
            if (Input.GetKeyUp(KeyCode.Space))
                Predict();
        }

        private async void Predict () {
            // Predict
            var watch = Stopwatch.StartNew();
            var prediction = await fxn.Predictions.Create(
                tag: Tag,
                inputs: new () { ["radius"] = 4 }
            );
            watch.Stop();
            // Log
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
            Debug.Log($"Prediction latency: {watch.Elapsed.TotalMilliseconds}ms"); 
        }

        private async void OnDisable () {
            var deleted = await fxn.Predictions.Delete(Tag);
            Debug.Log($"Deleted predictor: {deleted}");
        }
    }
}