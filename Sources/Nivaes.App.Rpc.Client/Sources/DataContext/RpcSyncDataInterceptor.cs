using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Nivaes.App.Rpc.Client.RpcSyncData;
using Nivaes.App.RPC.Client;

namespace Nivaes.App.Rpc.Client
{
    internal class RpcSyncDataInterceptor : SaveChangesInterceptor
    {
        private readonly IDbContextFactory<RpcSyncDatabaseContext> _rpcSyncDbFactory;
        private readonly RpcSyncDataSignal _rpcSyncSignal;

        public RpcSyncDataInterceptor(IDbContextFactory<RpcSyncDatabaseContext> rpcSyncDbFactory, RpcSyncDataSignal signal)
        {
            _rpcSyncDbFactory = rpcSyncDbFactory;
            _rpcSyncSignal = signal;
        }

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

        #region SavingChanges
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (RpcSyncDataInterceptorScope.IsSuppressed(eventData.Context!.ContextId))
                return result;

            var context = eventData.Context!;

            var changes = GetChanges(context);

            using var rpcSyncDb = _rpcSyncDbFactory.CreateDbContext();

            foreach (var entry in changes)
            {
                var syncData = CreateSyncData(entry);

                if (syncData is null)
                    continue;

                var syncDataItem = rpcSyncDb.SyncDatas.Find(syncData.Id);

                if (syncDataItem is null)
                {
                    rpcSyncDb.SyncDatas.Add(syncData);
                }
                else
                {
                    rpcSyncDb.Entry(syncDataItem)
                        .CurrentValues
                        .SetValues(syncData);
                }
            }

            rpcSyncDb.SaveChanges();

            _rpcSyncSignal.Signal();

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (RpcSyncDataInterceptorScope.IsSuppressed(eventData.Context!.ContextId))
                return result;

            var context = eventData.Context!;

            var changes = GetChanges(context);

            await using var rpcSyncDb = await _rpcSyncDbFactory.CreateDbContextAsync(cancellationToken);

            foreach (var entry in changes)
            {
                var syncData = CreateSyncData(entry);

                if (syncData is null)
                    continue;

                var syncDataItem = await rpcSyncDb.SyncDatas.FindAsync(syncData.Id, cancellationToken);

                if (syncDataItem is null)
                {
                    await rpcSyncDb.SyncDatas.AddAsync(syncData);
                }
                else
                {
                    rpcSyncDb.Entry(syncDataItem)
                        .CurrentValues
                        .SetValues(syncData);
                }
            }

            await rpcSyncDb.SaveChangesAsync(cancellationToken);

            _rpcSyncSignal.Signal();

            return await base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        private static List<EntityEntry> GetChanges(DbContext context)
        {
            return context.ChangeTracker
                .Entries()
                .Where(x => x.State is
                    EntityState.Added or
                    EntityState.Modified or
                    EntityState.Deleted)
                .ToList();
        }

        private static SyncData? CreateSyncData(EntityEntry entry)
        {
            var item = entry.Entity as IRpcDataModel;

            if (item is null)
                return null;

            item.TimeStampTicks = DateTime.UtcNow.Ticks;

            var itemData =
                MemoryPackSerializer.Serialize(
                    item.GetType(),
                    item);

            return new SyncData
            {
                Id = item.Id,
                Data = itemData,
                EntityType = item.GetType().FullName!,
                TimeStampTicks = item.TimeStampTicks,
            };
        }
        #endregion

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
