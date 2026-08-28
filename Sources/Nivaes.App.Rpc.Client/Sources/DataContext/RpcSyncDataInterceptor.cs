using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Nivaes.App.Rpc.Client.Hosting;
using Nivaes.App.Rpc.Client.RpcSyncData;
using Nivaes.App.RPC.Client;

namespace Nivaes.App.Rpc.Client
{
    internal class RpcSyncDataInterceptor : SaveChangesInterceptor
    {
        public override void SaveChangesCanceled(DbContextEventData eventData)
        {
            System.Diagnostics.Debugger.Break();
            base.SaveChangesCanceled(eventData);
        }

        public override Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debugger.Break();
            return base.SaveChangesCanceledAsync(eventData, cancellationToken);
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            System.Diagnostics.Debugger.Break();
            base.SaveChangesFailed(eventData);
        }

        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debugger.Break();
            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            System.Diagnostics.Debugger.Break();
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (RpcSyncDataInterceptorScope.IsSuppressed(eventData.Context!.ContextId))
                return result;

            var changes = eventData.Context!.ChangeTracker
                   .Entries()
                   .Where(x => x.State is
                       EntityState.Added or
                       EntityState.Modified or
                       EntityState.Deleted)
                   .ToList();

            var signal = RpcHostExtension.Services!.GetRequiredService<RpcSyncDataSignal>();
            var rpcSyncDbFactory = RpcHostExtension.Services!.GetRequiredService<IDbContextFactory<RpcSyncDatabaseContext>>();
            using var rpcSyncDb = await rpcSyncDbFactory.CreateDbContextAsync(cancellationToken);

            foreach (var entry in changes)
            {
                var item = entry.Entity as IRpcDataModel;
                if (item != null)
                {
                    item.TimeStampTicks = DateTime.UtcNow.Ticks;

                    var itemData = MemoryPackSerializer.Serialize(item.GetType(), item);

                    var syncDataItem = await rpcSyncDb.SyncDatas.FindAsync(item.Id, cancellationToken);

                    var syncData = new SyncData
                    {
                        Id = item.Id,
                        Data = itemData,
                        EntityType = item.GetType().FullName!,
                        TimeStampTicks = item.TimeStampTicks,
                    };

                    if (syncDataItem == null)
                    {
                        rpcSyncDb.SyncDatas.Add(syncData);
                    }
                    else
                    {
                        rpcSyncDb.Entry(syncDataItem).CurrentValues.SetValues(syncData);
                    }
                }
            }
            await rpcSyncDb.SaveChangesAsync();

            signal.Signal();

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult ThrowingConcurrencyException(ConcurrencyExceptionEventData eventData, InterceptionResult result)
        {
            System.Diagnostics.Debugger.Break();
            return base.ThrowingConcurrencyException(eventData, result);
        }

        public override ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(ConcurrencyExceptionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debugger.Break();
            return base.ThrowingConcurrencyExceptionAsync(eventData, result, cancellationToken);
        }
    }
}
