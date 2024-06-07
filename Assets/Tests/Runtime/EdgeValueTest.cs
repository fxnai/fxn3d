/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using UnityEngine;
    using Internal;
    using Services;
    using Types;

    internal sealed class EdgeValueTest : MonoBehaviour {

        void Start () {
            // Create value
            var tensor = new Tensor<ulong>(
                new ulong[30 * 2 * 5],
                new [] { 30, 2, 5 }
            );
            var value = PredictionService.ToValue(tensor);
            // Inspect
            value.GetValueType(out var type).Throw();
            value.GetValueDimensions(out var dimensions).Throw();
            var shape = new int[dimensions];
            value.GetValueShape(shape, shape.Length).Throw();
            // Log
            Debug.Log($"Value type: {type}");
            Debug.Log($"Value dimensions: {dimensions}");
            Debug.Log("Value shape: [" + string.Join(",", shape) + "]");
        }
    }
}