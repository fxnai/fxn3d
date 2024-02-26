/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Newtonsoft.Json;
    using Internal;

    internal sealed class InspectBuildCacheTest : MonoBehaviour {

        private void Start () {
            var settings = FunctionSettings.Instance;
            Debug.Log(JsonConvert.SerializeObject(settings.cache, Formatting.Indented));
            Debug.Log(settings.accessKey);
        }
    }
}