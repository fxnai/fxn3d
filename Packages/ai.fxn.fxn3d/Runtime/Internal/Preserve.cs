/*
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System;

    /// <summary>
    /// Prevent code stripping.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    internal sealed class PreserveAttribute : Attribute { }
}