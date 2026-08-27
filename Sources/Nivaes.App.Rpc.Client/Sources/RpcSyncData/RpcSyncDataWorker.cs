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
using static Grpc.Core.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Nivaes.App.Rpc;

internal class RpcSyncDataWorker<TContext>(
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

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            await UpdateLastTimestampSetting(cancellationToken);
        
            var syncDatas = SyncDatas(cancellationToken);

            await syncDataService.SendData(syncDatas, new CallContext(
                    new CallOptions(
                        headers: new Metadata {{ "IdClient", "1" }},
                        cancellationToken: cancellationToken)));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to sync data");
        }
    }

    private async IAsyncEnumerable<SyncData> SyncDatas([EnumeratorCancellation] CancellationToken cancellationToken)
    {
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
        await using var db = await syncDbFactory.CreateDbContextAsync(cancellationToken);

        var rquest = new SyncDataRequest
        {
            IdClient = 3,
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

    private async Task SaveDatas(IAsyncEnumerable<SyncData> items, CancellationToken cancellationToken)
    {
        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            logger.LogDebug($"RPC receiving item: {item.Id} . {item.EntityType}");

            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

            if (!Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType.TryGetValue(item.EntityType, out var syncDataType))
            {
                logger.LogError($"The type {item.EntityType} not register.");
                continue;
            }

            var dataItem = (IRpcDataModel?)MemoryPackSerializer.Deserialize(syncDataType, item.Data);

            await db.UpdateItemAsync(dataItem!);

            await db.SaveChangesAsync(cancellationToken);

            logger.LogDebug("Rpc Reciving saving");
        }       
    }

    private async Task ReceiverDatas(CancellationToken cancellationToken)
    {
        var requestSend = new SyncConnectionRequest
        {
            IdClient = 2
        };

        var connection = syncDataService.Connect(requestSend, cancellationToken);

        try
        { 
            await SaveDatas(connection, cancellationToken);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            logger.LogTrace("Rpc connection cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RPC read datas error.");
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
