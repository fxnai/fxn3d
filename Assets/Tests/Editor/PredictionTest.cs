/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;

    internal sealed class PredictionTest {

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create();

        [Test(Description = @"Should create a prediction")]
        public async Task CreatePrediction () {
            var prediction = await fxn.Predictions.Create(
                tag: "@yusuf/area",
                inputs: new () {
                    ["radius"] = 3f,
                }
            );
            var result = prediction?.results?[0];
            Assert.NotNull(result);
        }
    }
}