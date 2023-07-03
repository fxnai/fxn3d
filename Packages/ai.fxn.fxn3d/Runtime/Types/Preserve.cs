/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Types {

    using System;

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class PreserveAttribute : Attribute { }
}