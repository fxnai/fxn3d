/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine.TestTools;
    using Types;

    internal sealed class PredictorTest {

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create(url: @"https://api.fxn.dev/graph");

        [Test(Description = @"Should retrieve a valid predictor")]
        public async Task RetrievePredictor () {
            var tag = "@natml/identity";
            var predictor = await fxn.Predictors.Retrieve(tag);
            Assert.AreEqual(tag, predictor?.tag);
            Assert.AreEqual(PredictorStatus.Active, predictor?.status);
            Assert.AreEqual(PredictorType.Cloud, predictor?.type);
            Assert.AreEqual(Acceleration.CPU, predictor?.acceleration);
        }

        [Test(Description = @"Should list predictors owned by user")]
        public async Task ListPredictors () {
            var predictors = await fxn.Predictors.List();
            Assert.IsNotEmpty(predictors);
        }

        [Test(Description = @"Should search public predictors")]
        public async Task SearchPredictors () {
            var predictors = await fxn.Predictors.Search();
            Assert.IsNotEmpty(predictors);
        }

        [Test(Description = @"Should create and delete predictor")]
        public async Task CreatePredictor () {
            var tag = "@natml/unity-test";
            var type = PredictorType.Cloud;
            var notebook = "https://fxnai.s3.amazonaws.com/notebooks/05d441948f1da5f2b49a1c/identity.ipynb";
            // Create
            var predictor = await fxn.Predictors.Create(tag, notebook, type: type);
            Assert.AreEqual(tag, predictor?.tag);
            // Delete
            var result = await fxn.Predictors.Delete(tag);
            Assert.IsTrue(result);
        }
    }
}