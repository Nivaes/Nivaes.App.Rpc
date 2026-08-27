using System;
using System.Collections.Generic;
using System.Text;

namespace Nivaes.App.Rpc.AspNetCore
{
    internal sealed class CleanupAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> source;
        private readonly Func<ValueTask> cleanup;

        public CleanupAsyncEnumerable(
            IAsyncEnumerable<T> source,
            Func<ValueTask> cleanup)
        {
            this.source = source;
            this.cleanup = cleanup;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            return new Enumerator(
                source.GetAsyncEnumerator(cancellationToken),
                cleanup);
        }

        private sealed class Enumerator : IAsyncEnumerator<T>
        {
            private readonly IAsyncEnumerator<T> inner;
            private readonly Func<ValueTask> cleanup;
            private bool cleaned;

            public Enumerator(
                IAsyncEnumerator<T> inner,
                Func<ValueTask> cleanup)
            {
                this.inner = inner;
                this.cleanup = cleanup;
            }

            public T Current => inner.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return inner.MoveNextAsync();
            }

            public async ValueTask DisposeAsync()
            {
                if (cleaned)
                    return;

                cleaned = true;

                try
                {
                    await inner.DisposeAsync();
                }
                finally
                {
                    await cleanup();
                }
            }
        }
    }
}
