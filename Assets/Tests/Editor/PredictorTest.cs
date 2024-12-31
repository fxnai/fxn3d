/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine.TestTools;
    using Types;

    internal sealed class PredictorTest {

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create();

        [Test(Description = @"Should retrieve a valid predictor")]
        public async Task RetrievePredictor () {
            var tag = "@fxn/greeting";
            var predictor = await fxn.Predictors.Retrieve(tag);
            Assert.AreEqual(tag, predictor?.tag);
            Assert.AreEqual(PredictorStatus.Active, predictor?.status);
        }
    }
}