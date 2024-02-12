/*
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Function.Types {

    using Internal;

    /// <summary>
    /// Tensor.
    /// </summary>
    [Preserve]
    public readonly struct Tensor<T> where T : unmanaged {

        /// <summary>
        /// Tensor data.
        /// </summary>
        public readonly T[] data;

        /// <summary>
        /// Tensor shape.
        /// </summary>
        public readonly int[] shape;

        /// <summary>
        /// Create a tensor.
        /// </summary>
        /// <param name="data">Tensor data.</param>
        /// <param name="shape">Tensor shape.</param>
        public Tensor (T[] data, int[] shape) {
            this.data = data;
            this.shape = shape;
        }
    }
}