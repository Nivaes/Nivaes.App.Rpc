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
    public async IAsyncEnumerable<SyncData> GetData(long lastTimestampTicks, CallContext context = default)
    {
        logger.LogInformation("The message is received");

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<Test>("Test");

        var filter = Builders<Test>.Filter.And(
                Builders<Test>.Filter.Gt("TimeStampTicks", lastTimestampTicks)
            );

        using var syncDatas = await collection
            .Find(filter)
            .Sort(Builders<Test>.Sort.Ascending(nameof(Test.TimeStampTicks)))
             .ToCursorAsync();

        while (await syncDatas.MoveNextAsync())
        {
            foreach (var syncData in syncDatas.Current)
            {
                yield return new SyncData
                {
                    Id = syncData.Id,
                    EntityType = syncData.EntityType,
                    TimeStampTicks = syncData.TimeStampTicks
                };
            }
        }
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
                EntityType = syncData.EntityType,
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
    required public Guid Id { get; set; }

    required public string EntityType { get; set; }

    required public long TimeStampTicks { get; set; }
}
