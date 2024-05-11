/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Types;

    internal sealed class EnumValueTest {

        private const string RedAlias = @"hello";

        private enum SomeEnum {
            [EnumMember(Value = RedAlias)]
            Red = 0,
            [EnumMember]
            Green = 1,
            Blue = 2,
        }

        private Function fxn;

        [SetUp]
        public void Before () => fxn = FunctionUnity.Create(url: @"https://api.fxn.dev");

        [Test(Description = @"Should roundtrip an enum value")]
        public async Task RountripEnumValue () {
            // Rountrip enum with serialization alias
            var redValue = await fxn.Predictions.ToValue(SomeEnum.Red, "enum");
            var redObject = await fxn.Predictions.ToObject(redValue);
            Assert.That(redValue.type, Is.EqualTo(Dtype.String));
            Assert.That(redObject, Is.EqualTo(RedAlias));
            // Roundtrip enum with attribute but no alias
            var greenValue = await fxn.Predictions.ToValue(SomeEnum.Green, "enum");
            var greenObject = await fxn.Predictions.ToObject(greenValue);
            Assert.That(greenValue.type, Is.EqualTo(Dtype.Int32));
            Assert.That(greenObject, Is.EqualTo((int)SomeEnum.Green));
            // Rountrip plain enum value
            var blueValue = await fxn.Predictions.ToValue(SomeEnum.Blue, "enum");
            var blueObject = await fxn.Predictions.ToObject(blueValue);
            Assert.That(blueValue.type, Is.EqualTo(Dtype.Int32));
            Assert.That(blueObject, Is.EqualTo((int)SomeEnum.Blue));
        }
    }
}