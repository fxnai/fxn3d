/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System.IO;

    /// <summary>
    /// Helpful extension methods.
    /// </summary>
    internal static class FunctionUtils {

        public static byte[] ToArray (this Stream stream) {
            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();
            using (var dstStream = new MemoryStream()) {
                stream.CopyTo(dstStream);
                return dstStream.ToArray();
            }
        }
    }
}