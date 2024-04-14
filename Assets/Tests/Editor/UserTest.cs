/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine.TestTools;
    using Types;

    internal sealed class UserTest {

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create(url: @"https://api.fxn.dev");

        [Test(Description = @"Should retrieve the current user")]
        public async Task RetrieveUser () {
            var username = "natml";
            var user = await fxn.Users.Retrieve();
            Assert.AreEqual(username, user?.username);
            Assert.IsNotNull(user?.email);
        }

        [Test(Description = @"Should retrieve a user profile")]
        public async Task ViewProfile () {
            var username = "natsuite";
            var profile = await fxn.Users.Retrieve(username);
            Assert.AreEqual(username, profile?.username);
        }
    }
}