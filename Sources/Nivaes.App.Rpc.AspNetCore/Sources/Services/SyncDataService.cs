using System.Collections.Concurrent;
using System.Reflection.PortableExecutable;
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
    private static readonly ConcurrentDictionary<int, Channel<SyncData>> Connections = new();

    async IAsyncEnumerable<SyncData> ISyncDataService.Connect(SyncDataRequest request, CallContext context)
    {
        var channel = Channel.CreateUnbounded<SyncData>();

        Connections[request.IdClient] = channel;

        try
        {
            await foreach (var message in channel.Reader.ReadAllAsync(context.CancellationToken))
            {
                yield return message;
            }
        }
        finally
        {
            Connections.TryRemove(request.IdClient, out _);
        }
    }

    async IAsyncEnumerable<SyncData> ISyncDataService.GetData(SyncDataRequest request, CallContext context)
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

    async ValueTask<SyncDataResult> ISyncDataService.SendData(IAsyncEnumerable<SyncData> datas, CallContext context)
    {       
        logger.LogInformation("Rpc SendData");
        var headers = context.ServerCallContext?.RequestHeaders;
        var idUser = int.Parse(headers!.FirstOrDefault(x => x.Key == "iduser")!.Value!);

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<MongoDocument>(CollectionName);

        await foreach(var item in datas)
        {
            if(!Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType.TryGetValue(item.EntityType, out var syncDataType))
            {
                logger.LogError($"The type {item.EntityType} not register.");
                continue;
            }

            foreach (var channel in Connections)
            {
                if(channel.Key != idUser)
                    await channel.Value.Writer.WriteAsync(item, context.CancellationToken);
            }

            var test = new MongoDocument
            {
                Id = item.Id,
                DataItem = (IRpcDataModel?)MemoryPackSerializer.Deserialize(syncDataType, item.Data),
                TimeStampTicks = item.TimeStampTicks
            };
            await collection.InsertOrUpdateOneAsync(test, context.CancellationToken);
        }

        return new SyncDataResult
        {
            Success = true
        };
    }
}


