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
            var configId = new StringBuilder(2048);
            var version = Marshal.PtrToStringAuto(Function.GetVersion());
            Function.GetConfigurationUniqueID(configId, configId.Capacity);
            Debug.Log($"Function {version}");
            Debug.Log($"Configuration: {configId}");
        }
    }
}