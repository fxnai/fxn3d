/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    [Function.Embed(Tag)]
    internal sealed class GreetingTest : MonoBehaviour {

        private const string Tag = "@fxn/greeting";

        private async void Start () {
            // Predict
            var fxn = FunctionUnity.Create();
            var prediction = await fxn.Predictions.Create(
                tag: Tag,
                inputs: new () { [@"name"] = "Yusuf" }
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
            // Unload from memory
            var deleted = await fxn.Predictions.Delete(Tag);
            Debug.Log($"Deleted predictor: {deleted}");
        }
    }
}