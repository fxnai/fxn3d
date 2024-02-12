/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class AsyncEnumerableQueue<T> : IDisposable {

        #region --Client API--

        public AsyncEnumerableQueue () {
            this.queue = new ConcurrentQueue<T>();
            this.semaphore = new SemaphoreSlim(0);
            this.completed = false;
        }

        public void Enqueue (T item) {
            queue.Enqueue(item);
            semaphore.Release();
        }

        public void Dispose () {
            completed = true;
            semaphore.Release();
        }

        public async IAsyncEnumerable<T> Stream (
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        ) {
            while (true) {
                await semaphore.WaitAsync(cancellationToken);
                if (queue.TryDequeue(out var item))
                    yield return item;
                if (completed && queue.IsEmpty)
                    yield break;
            }
        }
        #endregion


        #region --Operations--
        private readonly ConcurrentQueue<T> queue;
        private readonly SemaphoreSlim semaphore;
        private bool completed;
        #endregion
    }
}