/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Newtonsoft.Json;
    using Beta;

    internal sealed class RemoteGreetingTest : MonoBehaviour {

        public RemoteAcceleration acceleration;
        private const string Tag = "@fxn/greeting";

        private async void Start () {
            var fxn = FunctionUnity.Create();
            var prediction = await fxn.Beta.Predictions.Remote.Create(
                tag: Tag,
                inputs: new () { [@"name"] = "Yusuf" },
                acceleration: acceleration
            );
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
        }
    }
}