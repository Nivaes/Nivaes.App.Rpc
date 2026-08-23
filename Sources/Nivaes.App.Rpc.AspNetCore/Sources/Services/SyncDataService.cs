using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;

namespace Nivaes.App.Rpc.AspNetCore.Server;

public class SyncDataService(ILogger<SyncDataService> logger) 
    : ISyncDataService
{
    public async IAsyncEnumerable<SyncData> GetData(CallContext context = default)
    {
        logger.LogInformation("The message is received");

        yield return new SyncData
        {
            // datos
        };

        yield return new SyncData
        {
            // datos
        };
    }

    public async ValueTask<SyncResult> SendData(IAsyncEnumerable<SyncData> datas, CallContext context = default)
    {
        logger.LogInformation("The message is received");

        await foreach(var syncData in datas)
        {
            Console.WriteLine($"Sync: {syncData.Id}: {syncData.EntityType}");
        }

        return new SyncResult
        {
            Success = true
        };
    }

}
