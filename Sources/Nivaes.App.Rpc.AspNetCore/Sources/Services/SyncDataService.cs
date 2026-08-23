using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Nivaes.App.Rpc.AspNetCore.Server;

public class SyncDataService(ILogger<SyncDataService> logger) 
    : ISyncDataContract
{
    public ValueTask<string> Echo(string message/*, ServerCallContext? context = null*/)
    {
        logger.LogInformation($"Echo{message}");
        return ValueTask.FromResult(message);
    }

    ValueTask<SyncData> ISyncDataContract.GetData(IAsyncStreamReader<SyncData> requestStream/*, ServerCallContext? context*/)
    {
        //logger.LogInformation("The message is received from {id}", requestStream.Current.Id);
        logger.LogInformation("The message is received");

        return ValueTask.FromResult(new SyncData
        {

        });
    }
}
