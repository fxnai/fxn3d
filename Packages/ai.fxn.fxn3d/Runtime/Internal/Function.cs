/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using Dtype = Types.Dtype;

    /// <summary>
    /// Function C API.
    /// </summary>
    public static class Function {

        public const string Version = @"0.0.4";
    }

    /// <summary>
    /// Prevent code stripping.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    internal sealed class PreserveAttribute : Attribute { }
}