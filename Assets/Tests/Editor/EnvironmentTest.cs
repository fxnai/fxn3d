/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Newtonsoft.Json;
    using Types;

    internal sealed class EnvironmentTest {

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create(url: @"https://api.fxn.dev");

        [Test(Description = @"Should list user environment variables")]
        public async Task ListEnvironmentVariables () {
            var variables = await fxn.EnvironmentVariables.List();
            Assert.That(variables, Is.Not.Empty);
        }

        [Test(Description = @"Should create and destroy an environment variable")]
        public async Task CreateEnvironmentVariable () {
            // Create
            var name = "ABC_TOKEN";
            var variable = await fxn.EnvironmentVariables.Create(
                name: name,
                value: "Hello world"
            );
            Assert.That(variable.name, Is.EqualTo(name));
            Assert.That(variable.value, Is.Null);
            // Delete
            var deleted = await fxn.EnvironmentVariables.Delete(name: name);
            Assert.That(deleted, Is.True);
        }
    }
}