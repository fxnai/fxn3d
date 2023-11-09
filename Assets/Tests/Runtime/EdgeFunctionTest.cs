/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Text;
    using UnityEngine;
    using Internal;

    internal sealed class EdgeFunctionTest : MonoBehaviour {

        void Start () {
            var uniqueId = new StringBuilder(2048);
            Function.GetConfigurationUniqueID(uniqueId, uniqueId.Capacity);
            Debug.Log($"Configuration ID: {uniqueId.ToString()}");
        }
    }
}