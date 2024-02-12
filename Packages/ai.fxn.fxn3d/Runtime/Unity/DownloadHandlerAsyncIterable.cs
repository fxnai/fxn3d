/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine.Networking;

    internal sealed class DownloadHandlerAsyncIterable : DownloadHandlerScript {

        private readonly AsyncEnumerableQueue<string> queue;

        public DownloadHandlerAsyncIterable () : base() {
            this.queue = new AsyncEnumerableQueue<string>();
        }

        public IAsyncEnumerable<string> Stream (
            CancellationToken cancellationToken = default
        ) => queue.Stream(cancellationToken);

        protected override bool ReceiveData (byte[] data, int dataLength) {
            if (data == null || data.Length == 0)
                return false;
            var payloadStr = Encoding.UTF8.GetString(data);
            queue.Enqueue(payloadStr);
            return true;
        }

        protected override void CompleteContent () => queue.Dispose();
    }
}