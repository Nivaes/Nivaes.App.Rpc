using System.Collections.Concurrent;
using System.ServiceModel.Channels;
using System.Threading.Channels;
using Grpc.Core;
using MemoryPack;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Connections;
using Nivaes.App.Cross;
using ProtoBuf.Grpc;

namespace Nivaes.App.Rpc.AspNetCore.Server;

internal sealed class SyncDataService(IMongoClient mongoClient, ILogger<SyncDataService> logger) 
    : ISyncDataService
{
    private const string CollectionName = "Items";
    //private static readonly ConcurrentDictionary<string, Channel<SyncData>> Connections = new();
    private static Channel<SyncData>? channel;

    async IAsyncEnumerable<SyncData> ISyncDataService.Connect(SyncConnection request, CallContext context)
    {
        channel = Channel.CreateUnbounded<SyncData>();

        //Connections[request.UserId] = channel;

        try
        {
            await foreach (var message in channel.Reader.ReadAllAsync(context.CancellationToken))
            {
                yield return message;
            }
        }
        finally
        {
            //Connections.TryRemove(request.UserId, out _);
            channel = null;
        }
    }

    async IAsyncEnumerable<SyncData> ISyncDataService.GetData(SyncRequest request, CallContext context)
    {
        logger.LogInformation("Rpc GetData");

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<MongoDocument>(CollectionName);

        var filter = Builders<MongoDocument>.Filter.And(
                Builders<MongoDocument>.Filter.Lte(x => x.TimeStampTicks, request.LastTimestampTicks)
            );
        var findOptions = new FindOptions<MongoDocument>
        {
            BatchSize = 100,
            Sort = Builders<MongoDocument>.Sort.Ascending(nameof(MongoDocument.TimeStampTicks))
        };

        using var syncDatas = await collection
            .FindAsync(filter, findOptions, context.CancellationToken);

        while (await syncDatas.MoveNextAsync(context.CancellationToken))
        {
            foreach (var syncData in syncDatas.Current)
            {
                var type = syncData.DataItem!.GetType();
                var data = MemoryPackSerializer.Serialize(type!, syncData.DataItem);

                yield return new SyncData
                {
                    Id = syncData.Id,
                    EntityType = type.FullName!,
                    Data = data,
                    TimeStampTicks = syncData.TimeStampTicks
                };
            }
        }
    }

    async ValueTask<SyncResult> ISyncDataService.SendData(IAsyncEnumerable<SyncData> datas, CallContext context)
    {       
        logger.LogInformation("Rpc SendData");

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<MongoDocument>(CollectionName);

        await foreach(var item in datas)
        {
            if(!Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType.TryGetValue(item.EntityType, out var syncDataType))
            {
                logger.LogError($"The type {item.EntityType} not register.");
                continue;
            }
            //var itemData = (IRpcDataModel?)MemoryPackSerializer.Deserialize(syncDataType, item.Data);

            await channel!.Writer.WriteAsync(item, context.CancellationToken);

            var test = new MongoDocument
            {
                Id = item.Id,
                //EntityType = item.EntityType,
                //DataItem= new BsonBinaryData(item.Data)
                DataItem = (IRpcDataModel?)MemoryPackSerializer.Deserialize(syncDataType, item.Data),
                TimeStampTicks = item.TimeStampTicks
            };
            await collection.InsertOrUpdateOneAsync(test, context.CancellationToken);
        }

        return new SyncResult
        {
            Success = true
        };
    }
}


