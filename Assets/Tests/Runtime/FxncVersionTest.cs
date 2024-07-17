/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using Internal;

    internal sealed class FxncVersionTest : MonoBehaviour {

        private async void Start () {
            await FunctionUtils.Initialization;
            Debug.Log("Initialized Function!");
            // Version
            var version = Marshal.PtrToStringAuto(Function.GetVersion());
            Debug.Log($"Function {version}");
            // Client
            var clientId = new StringBuilder(64);
            Function.GetConfigurationClientID(clientId, clientId.Capacity);
            Debug.Log($"Client: {clientId}");
            // Configuration
            var configId = new StringBuilder(2048);
            Function.GetConfigurationUniqueID(configId, configId.Capacity);
            Debug.Log($"Configuration: {configId}");
        }
    }
}