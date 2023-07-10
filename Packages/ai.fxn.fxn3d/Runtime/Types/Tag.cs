/*
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace Function.Types {

    using System;
    using System.Diagnostics.CodeAnalysis;
    using Internal;

    /// <summary>
    /// Predictor tag.
    /// </summary>
    [Preserve, Serializable]
    public sealed class Tag {

        #region --Client API--
        /// <summary>
        /// Predictor owner username.
        /// </summary>
        public string username;

        /// <summary>
        /// Predictor name.
        /// </summary>
        public string name;

        /// <summary>
        /// Create a tag.
        /// </summary>
        /// <param name="username">Owner username.</param>
        /// <param name="name">Predictor name.</param>
        public Tag (string username, string name) {
            this.username = username;
            this.name = name;
        }

        /// <summary>
        /// Serialize the tag.
        /// </summary>
        public override string ToString () => $"@{username}/{name}";

        /// <summary>
        /// Try to parse a predictor tag from a string.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="tag">Output tag.</param>
        /// <returns>Whether the tag was successfully parsed.</returns>
        public static bool TryParse (string input, [NotNullWhen(true)] out Tag? tag) {
            tag = null;
            // Check username prefix
            input = input.ToLowerInvariant();
            if (!input.StartsWith("@"))
                return false;
            // Check stem
            var stem = input.Split('/');
            if (stem.Length != 2)
                return false;
            // Parse
            var username = stem[0].Substring(1);
            var name = stem[1];
            tag = new Tag(username, name);
            // Return
            return true;
        }

        public static implicit operator string (Tag tag) => tag.ToString();
        #endregion

    }
}