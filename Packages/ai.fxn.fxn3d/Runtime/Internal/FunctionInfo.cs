/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using static Function.Internal.Function;

// Metadata
[assembly: AssemblyCompany(@"NatML Inc")]
[assembly: AssemblyTitle(@"Function.Runtime")]
[assembly: AssemblyVersionAttribute(Version)]
[assembly: AssemblyCopyright(@"Copyright © 2023 NatML Inc. All Rights Reserved.")]

// Friends
[assembly: InternalsVisibleTo(@"Function.Editor")]
[assembly: InternalsVisibleTo(@"Function.Tests.Editor")]
[assembly: InternalsVisibleTo(@"Function.Tests.Runtime")]