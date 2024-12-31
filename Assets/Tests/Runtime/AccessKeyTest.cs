/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Internal;

    internal sealed class AccessKeyTest : MonoBehaviour {

        private void Start () => Debug.Log($"Access key: {FunctionSettings.Instance.accessKey}");
    }
}