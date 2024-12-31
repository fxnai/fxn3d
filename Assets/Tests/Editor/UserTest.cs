/* 
*   Function
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine.TestTools;

    internal sealed class UserTest {

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create();

        [Test(Description = @"Should retrieve the current user")]
        public async Task RetrieveUser () {
            var user = await fxn.Users.Retrieve();
            Assert.AreEqual(@"yusuf", user?.username);
        }
    }
}