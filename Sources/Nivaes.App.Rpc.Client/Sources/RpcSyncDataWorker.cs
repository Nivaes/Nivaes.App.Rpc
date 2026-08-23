using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nivaes.App.RPC.Client;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Nivaes.App.Rpc;

public class RpcSyncDataWorker(
        ISyncDataService syncDataService,
        ILogger<RpcSyncDataWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox error");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(2),
                stoppingToken);
        }
    }

    private async Task ProcessAsync(
      CancellationToken cancellationToken)
    {
        //await using var db =
        //    await factory.CreateDbContextAsync(
        //        cancellationToken);

        using var db = new RpcSyncDatabaseContext();

        var messages = await db.Set<SyncData>()
            .OrderBy(x => x.TimeStampTicks)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                //await syncDataService.SendData(
                //    [message],
                //    cancellationToken);

                db.Remove(message);

                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unable to send outbox message {Id}", message.Id);

                break;
            }
        }
    }
}
