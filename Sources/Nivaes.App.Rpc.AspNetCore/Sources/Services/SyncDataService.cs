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
using static System.Net.WebRequestMethods;

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
        var collection = database.GetCollection<MongoDocument>("Test");

        var filter = Builders<MongoDocument>.Filter.And(
                Builders<MongoDocument>.Filter.Gt("TimeStampTicks", request.LastTimestampTicks)
            );

        using var syncDatas = await collection
            .Find(filter)
            .Sort(Builders<MongoDocument>.Sort.Ascending(nameof(MongoDocument.TimeStampTicks)))
             .ToCursorAsync(context.CancellationToken);

        while (await syncDatas.MoveNextAsync(context.CancellationToken))
        {
            foreach (var syncData in syncDatas.Current)
            {
                yield return new SyncData
                {
                    Id = syncData.Id,
                    //EntityType = syncData.EntityType
                    TimeStampTicks = syncData.TimeStampTicks
                };
            }
        }
    }

    async ValueTask<SyncResult> ISyncDataService.SendData(IAsyncEnumerable<SyncData> datas, CallContext context)
    {       
        logger.LogInformation("The message is received");

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<MongoDocument>("Items");

        await foreach(var item in datas)
        {
            if(!Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType.TryGetValue(item.EntityType, out var syncDataType))
            {
                logger.LogError($"The type {item.EntityType} not register.");
                continue;
            }
            //var itemData = (IRpcDataModel?)MemoryPackSerializer.Deserialize(syncDataType, item.Data);

            var test = new MongoDocument
            {
                Id = item.Id,
                //EntityType = item.EntityType,
                //DataItem= new BsonBinaryData(item.Data)
                DataItem = (IRpcDataModel?)MemoryPackSerializer.Deserialize(syncDataType, item.Data),
                TimeStampTicks = item.TimeStampTicks
            };
            await collection.InsertOrUpdateOneAsync(test, context.CancellationToken);
            Console.WriteLine($"Sync: {item.Id}: {item.EntityType}");

            using var syncDatas = await collection
             .Find(Builders<MongoDocument>.Filter.Empty)
             .ToCursorAsync();

            while (await syncDatas.MoveNextAsync(context.CancellationToken))
            {
                foreach (var syncData in syncDatas.Current)
                {
                    //yield return new SyncData
                    //{
                    //    Id = syncData.Id,
                    //    EntityType = syncData.EntityType,
                    //    TimeStampTicks = syncData.TimeStampTicks
                    //};
                }
            }
        }

        return new SyncResult
        {
            Success = true
        };
    }
}


