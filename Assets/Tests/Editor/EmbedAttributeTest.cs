/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Tests {

    using System;
    using System.Linq;
    using NUnit.Framework;
    using Editor.Build;

    [Function.Embed]
    internal sealed class EmbedAttributeTest {

        private const string CustomPredictor = "@samples/stable-diffusion";
        private const string CustomURL = "https://www.google.com";
        private const string CustomAccessKey = @"hello world"; 

        [Function.Embed(CustomPredictor)]
        private static Function fxn => new Function(url: CustomURL, accessKey: CustomAccessKey);

        [Test(Description = @"Should extract default embed attribute")]
        public void ExtractDefaultEmbed () {
            var embeds = BuildHandler.GetEmbeds();
            var embed = embeds.FirstOrDefault(e => e.tags.Length == 0);
            Assert.That(embed, Is.Not.Null);
            var fxn = embed.getFunction();
            Assert.That(fxn, Is.Not.Null);
            Assert.That(fxn.client.url == Function.URL);
            Assert.That(fxn.client.accessKey == Internal.FunctionSettings.Instance.accessKey);
        }

        [Test(Description = @"Should extract custom embed attribute")]
        public void ExtractCustomEmbed () {
            var embeds = BuildHandler.GetEmbeds();
            var embed = embeds.FirstOrDefault(e => e.tags.Contains("@natml/greeting"));
            Assert.That(embed, Is.Not.Null);
            var fxn = embed.getFunction();
            Assert.That(fxn, Is.Not.Null);
            Assert.That(fxn.client.url == CustomURL);
            Assert.That(fxn.client.accessKey == CustomAccessKey);
        }
    }
}