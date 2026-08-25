using System.Collections.Concurrent;
using System.Threading.Channels;
using Grpc.Core;
using MemoryPack;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Nivaes.App.Cross;
using ProtoBuf.Grpc;

namespace Nivaes.App.Rpc.AspNetCore.Server;

internal sealed class SyncDataService(IMongoClient mongoClient, ILogger<SyncDataService> logger) 
    : ISyncDataService
{
    private static readonly ConcurrentDictionary<string, Channel<SyncData>> Connections = new();
    async IAsyncEnumerable<SyncData> ISyncDataService.Connect(SyncConnection request, CallContext context)
    {
        var channel = Channel.CreateUnbounded<SyncData>();

        Connections[request.UserId] = channel;

        try
        {
            await foreach (var message in channel.Reader.ReadAllAsync(context.CancellationToken))
            {
                yield return message;
            }
        }
        finally
        {
            Connections.TryRemove(request.UserId, out _);
        }
    }

    async IAsyncEnumerable<SyncData> ISyncDataService.GetData(SyncRequest request, CallContext context)
    {
        logger.LogInformation("The message is received");

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<Test>("Test");

        var filter = Builders<Test>.Filter.And(
                Builders<Test>.Filter.Gt("TimeStampTicks", request.LastTimestampTicks)
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

    async ValueTask<SyncResult> ISyncDataService.SendData(IAsyncEnumerable<SyncData> datas, CallContext context)
    {       
        logger.LogInformation("The message is received");

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<Test>("Test");

        await foreach(var item in datas)
        {
            if(!Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType.TryGetValue(item.EntityType, out var syncDataType))
            {
                logger.LogError($"The type {item.EntityType} not register.");
                continue;
            }
            var itemData = (IRpcDataModel?)MemoryPackSerializer.Deserialize(syncDataType, item.Data);

            var test = new Test
            {
                Id = item.Id,
                EntityType = item.EntityType,
                ItemData= itemData,
                TimeStampTicks = item.TimeStampTicks
            };
            await collection.InsertOneAsync(test);
            Console.WriteLine($"Sync: {item.Id}: {item.EntityType}");
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

    required public IRpcDataModel? ItemData { get; set; }

    required public long TimeStampTicks { get; set; }
}
