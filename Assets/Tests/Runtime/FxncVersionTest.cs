/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Runtime.InteropServices;
    using UnityEngine;
    using C;

    internal sealed class FxncVersionTest : MonoBehaviour {

        private async void Start () {
            await Configuration.InitializationTask;
            Debug.Log("Initialized Function!");
            // Version
            var version = Marshal.PtrToStringAuto(Function.GetVersion());
            Debug.Log($"Function {version}");
            Debug.Log($"Client: {Configuration.ClientId}");
            Debug.Log($"Configuration: {Configuration.ConfigurationId}");
        }
    }
}