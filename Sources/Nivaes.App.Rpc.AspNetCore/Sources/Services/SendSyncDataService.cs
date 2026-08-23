using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Nivaes.App.Rpc.AspNetCore.Server;

public class SendSyncDataService(ILogger<SendSyncDataService> logger, IMongoClient mongoClient) 
    : ServiceBase<ISyncDataService>,
        ISyncDataService
{
    //public ValueTask<string> Echo(string message/*, ServerCallContext? context = null*/)
    //{
    //    logger.LogInformation($"Echo{message}");
    //    return ValueTask.FromResult(message);
    //}

    public async UnaryResult<string> Echo(string message)
    {
        logger.LogInformation($"Echo{message}");
        return message;
    }

    //ValueTask<SyncData> ISendSyncDataService.GetData(IAsyncStreamReader<SyncData> requestStream/*, ServerCallContext? context*/)
    //{
    //    //logger.LogInformation("The message is received from {id}", requestStream.Current.Id);
    //    logger.LogInformation("The message is received");

    //    return ValueTask.FromResult(new SyncData
    //    {

    //    });
    //}
}
