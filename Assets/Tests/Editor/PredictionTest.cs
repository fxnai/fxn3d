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

    internal sealed class PredictionTest {

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create(url: @"https://api.fxn.dev/graph");

        [Test(Description = @"Should create a cloud prediction")]
        public async Task CreateCloudPrediction () {
            var tag = "@natml/identity";
            var inputs = new Dictionary<string, object> {
                ["name"] = "Yusuf",                
                ["age"] = 24,
                ["ratio"] = 3.14159f,
                ["option"] = false,
                ["choices"] = new [] { true, false, true, true, false },
                ["numbers"] = new [] { 12, 38, 4, 102, 99 },
                ["fractions"] = new [] { 4.2f, 9.3f, 1.3f },
                ["names"] = new [] { "Drake", "Josh", "Andy" },
                ["properties"] = new Dictionary<string, object> {
                    ["length"] = 12,
                    ["height"] = 44,
                    ["width"] = 3.9,
                }
            };
            var prediction = await fxn.Predictions.Create(
                tag: tag,
                inputs: inputs
            ) as CloudPrediction;
            var name = prediction?.results?[0];
            var age = prediction?.results?[1];
            var ratio = prediction?.results?[2];
            var option = prediction?.results?[3];
            Assert.AreEqual(tag, prediction?.tag);
            Assert.AreEqual(PredictorType.Cloud, prediction?.type);
            Assert.AreEqual(inputs["name"], name);
            Assert.AreEqual(inputs["age"], age);
            Assert.AreEqual(inputs["ratio"], ratio);
            Assert.AreEqual(inputs["option"], option);
        }
    }
}