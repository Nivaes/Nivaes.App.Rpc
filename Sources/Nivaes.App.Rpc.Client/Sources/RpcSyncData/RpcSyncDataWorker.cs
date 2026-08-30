using System.Runtime.CompilerServices;
using Grpc.Core;
using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nivaes.App.Rpc.Client;
using Nivaes.App.Rpc.Client.RpcSyncData;
using Nivaes.App.RPC.Client;
using ProtoBuf.Grpc;

namespace Nivaes.App.Rpc;

internal class RpcSyncDataWorker<TContext>(
        SyncClientConfiguration clientConfiguration,
        IDbContextFactory<TContext> dbFactory,
        IDbContextFactory<RpcSyncDatabaseContext> syncDbFactory,
        ISyncDataService syncDataService,
        RpcSyncDataSignal signal,
        ILogger<RpcSyncDataWorker<TContext>> logger)
    : BackgroundService
    where TContext : DbContext
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Rpc ExecuteAsync");

        var waitTask = signal.WaitAsync(cancellationToken).AsTask();
        var delayTask = Task.Delay(TimeSpan.FromMinutes(30), cancellationToken);

        await ReadDatas(cancellationToken);

        var taskSync = Task.Run(() => ReceiverDatas(cancellationToken));

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var completed = await Task.WhenAny(waitTask, delayTask);

                await ProcessAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Sync RPC error");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch(TaskCanceledException)
            {
                return;
            }
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Rpc ProcessAsync");

        try
        {
            await UpdateLastTimestampSetting(cancellationToken);
        
            var syncDatas = SyncDatas(cancellationToken);

            await syncDataService.SendData(syncDatas, new CallContext(
                    new CallOptions(
                        headers: new Metadata {{ "IdClient", clientConfiguration.IdClient } },
                        cancellationToken: cancellationToken)));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to sync data");
        }
    }

    private async IAsyncEnumerable<SyncData> SyncDatas([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        logger.LogDebug("Rpc SyncDatas");

        await using var db = await syncDbFactory.CreateDbContextAsync(cancellationToken);

        var items = db.SyncDatas
            .OrderBy(x => x.TimeStampTicks)
            //.Take(50)
            .AsAsyncEnumerable();

        await foreach (var item in items)
        {
            logger.LogTrace($"Rpc Send item: {item.Id} . {item.EntityType}");

            yield return item;

            db.Remove(item);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ReadDatas(CancellationToken cancellationToken)
    {
        logger.LogDebug("Rpc ReadDatas.");

        await using var db = await syncDbFactory.CreateDbContextAsync(cancellationToken);

        var rquest = new SyncDataRequest
        {
            IdClient = clientConfiguration.IdClient,
            LastTimestampTicks = 0//await LastTimestampSetting(cancellationToken)
        };

        var items = syncDataService.GetData(rquest, cancellationToken);

        try
        {
            await SaveDatas(items, cancellationToken);

            await UpdateLastTimestampSetting(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RPC read datas error.");
        }
    }

    private async Task ReceiverDatas(CancellationToken cancellationToken)
    {
        logger.LogDebug("Rpc ReceiverDatas.");
        var requestSend = new SyncConnectionRequest
        {
            IdClient = clientConfiguration.IdClient
        };

        var connection = syncDataService.Connect(requestSend, cancellationToken);

        try
        {
            await SaveDatas(connection, cancellationToken);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            logger.LogDebug("Rpc connection cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RPC receiver datas error.");
        }
    }

    private async Task SaveDatas(IAsyncEnumerable<SyncData> items, CancellationToken cancellationToken)
    {
        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            logger.LogTrace($"RPC receiving item: {item.Id} . {item.EntityType}");

            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

            if (!Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType.TryGetValue(item.EntityType, out var syncDataType))
            {
                logger.LogError($"The type {item.EntityType} not register.");
                continue;
            }

            var dataItem = (IRpcDataModel?)MemoryPackSerializer.Deserialize(syncDataType, item.Data);

            await db.UpdateItemAsync(dataItem!);

            using (RpcSyncDataInterceptorScope.Suppress(db.ContextId))
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            logger.LogDebug("Rpc Reciving saving");
        }       
    }

    private const string keyLastTimestamp = "LastTimestamp";

    private async Task<long> LastTimestampSetting(CancellationToken cancellationToken)
    {
        await using var db = await syncDbFactory.CreateDbContextAsync(cancellationToken);

        var lastTimestampSetting = await db.SyncSetting.Where(x => x.Key == keyLastTimestamp).FirstOrDefaultAsync(cancellationToken);

        if (lastTimestampSetting != null)
        {
            long.TryParse(lastTimestampSetting.Value, out var timestamp);
            return timestamp;            
        }

        return 0;
    }

    private async Task UpdateLastTimestampSetting(CancellationToken cancellationToken)
    {
        await using var db = await syncDbFactory.CreateDbContextAsync(cancellationToken);

        var lastTimestampSetting = await db.SyncSetting.Where(x => x.Key == keyLastTimestamp).FirstOrDefaultAsync(cancellationToken);

        long lastTimestamp = DateTime.UtcNow.Ticks;

        if (lastTimestampSetting == null)
        {
            lastTimestampSetting = new SyncSetting { Key = keyLastTimestamp, Value = lastTimestamp.ToString() };
            db.SyncSetting.Add(lastTimestampSetting);
        }
        else
        {
            lastTimestampSetting.Value = lastTimestampSetting.Value;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
