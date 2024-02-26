/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Runtime.InteropServices;
    using UnityEngine;
    using Internal;

    internal sealed class FxncVersionTest : MonoBehaviour {

        private void Start () {
            var version = Marshal.PtrToStringAuto(Function.GetVersion());
            Debug.Log($"Function {version}");
        }
    }
}