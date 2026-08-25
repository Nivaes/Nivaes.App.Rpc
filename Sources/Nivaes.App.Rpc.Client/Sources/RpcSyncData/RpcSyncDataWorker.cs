using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nivaes.App.Rpc.Client.RpcSyncData;
using Nivaes.App.RPC.Client;

namespace Nivaes.App.Rpc;

internal class RpcSyncDataWorker(
        IDbContextFactory<RpcSyncDatabaseContext> factory,
        ISyncDataService syncDataService,
        RpcSyncDataSignal signal,
        ILogger<RpcSyncDataWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var waitTask = signal.WaitAsync(cancellationToken).AsTask();
        var delayTask = Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);

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

            await Task.Delay(
                TimeSpan.FromSeconds(2),
                cancellationToken);
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await using var db =
            await factory.CreateDbContextAsync(
                cancellationToken);

        try
        {
            var syncDatas = SyncDatas(cancellationToken);

            await syncDataService.SendData(syncDatas, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to sync data");
        }
    }

    private async IAsyncEnumerable<SyncData> SyncDatas([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        var messages = db.SyncDatas
            .OrderBy(x => x.TimeStampTicks)
            .Take(50)
            .AsAsyncEnumerable();

        await foreach (var message in messages)
        {
            yield return message;

            db.Remove(message);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
