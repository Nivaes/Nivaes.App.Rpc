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

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(source.GetAsyncEnumerator(cancellationToken), cleanup, cancellationToken);
        }

        private sealed class Enumerator : IAsyncEnumerator<T>
        {
            private readonly IAsyncEnumerator<T> inner;
            private readonly Func<ValueTask> cleanup;
            private readonly CancellationToken cancellationToken;
            private bool cleaned;

            public Enumerator(IAsyncEnumerator<T> inner, Func<ValueTask> cleanup, CancellationToken cancellationToken)
            {
                this.inner = inner;
                this.cleanup = cleanup;
                this.cancellationToken = cancellationToken;
            }

            public T Current => inner.Current;

            public async ValueTask<bool> MoveNextAsync()
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                return await inner.MoveNextAsync();
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
