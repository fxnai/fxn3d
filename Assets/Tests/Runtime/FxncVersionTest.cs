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

        private void Start () {
            // Version
            var version = Marshal.PtrToStringAuto(Function.GetVersion());
            Debug.Log($"Function {version}");
            // Configuration
            var configId = new StringBuilder(2048);
            Function.GetConfigurationUniqueID(configId, configId.Capacity);
            Debug.Log($"Configuration: {configId}");
        }
    }
}