/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Newtonsoft.Json;
    using Types;

    internal sealed class EnvironmentTest { // INCOMPLETE

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create(url: @"https://api.fxn.dev/graph");

        [Test(Description = @"Should list user environment variables")]
        public async Task ListEnvironmentVariables () {

        }

        [Test(Description = @"Should create an environment variable")]
        public async Task CreateEnvironmentVariable () {

        }

        [Test(Description = @"Should delete an environment variable")]
        public async Task DeleteEnvironmentVariable () {

        }
    }
}