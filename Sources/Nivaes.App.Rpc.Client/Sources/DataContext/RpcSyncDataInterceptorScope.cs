using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Nivaes.App.Rpc.Client;

internal static class RpcSyncDataInterceptorScope
{
    private static readonly ConcurrentDictionary<DbContextId, byte> _suppress = new();
    public static bool IsSuppressed(DbContextId id) => _suppress.ContainsKey(id);

    public static IDisposable Suppress(DbContextId id)
    {
        _suppress.TryAdd(id, 0);
        return new DisposableAction(() => _suppress.TryRemove(id, out _));
    }

    private sealed class DisposableAction : IDisposable
    {
        private readonly Action _onDispose;
        private bool _disposed;

        public DisposableAction(Action onDispose) => _onDispose = onDispose;

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _onDispose();
        }
    }
}