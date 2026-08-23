using Grpc.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ProtoBuf.Grpc;

namespace Nivaes.App.Rpc.AspNetCore.Server;

internal class SyncDataService(IMongoClient mongoClient, ILogger<SyncDataService> logger) 
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

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<Test>("Test");

        await foreach(var syncData in datas)
        {
            var test = new Test
            {
                Id = syncData.Id,
                Name = syncData.EntityType,
                TimeStampTicks = syncData.TimeStampTicks
            };
            await collection.InsertOneAsync(test);
            Console.WriteLine($"Sync: {syncData.Id}: {syncData.EntityType}");
        }

        return new SyncResult
        {
            Success = true
        };
    }

}

public class Test
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public long TimeStampTicks { get; set; }
}
