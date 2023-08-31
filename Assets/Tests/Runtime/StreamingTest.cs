/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;
    using API;

    internal sealed class StreamingTest : MonoBehaviour {

        private async void Start () {
            var fxn = FunctionUnity.Create("fxn-aVPMIDpsxTsr4of8dknUt", "https://api.fxn.dev");
            var stream = fxn.Predictions.Stream(
                tag: "@yusuf-delete/streaming",
                inputs: new () {
                    ["sentence"] = "Hello world"
                }
            );
            await foreach (var prediction in stream)
                Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
            Debug.Log("Done!");
        }
    }
}