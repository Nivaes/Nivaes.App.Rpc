using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.Threading.Channels;
using MemoryPack;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ProtoBuf.Grpc;

namespace Nivaes.App.Rpc.AspNetCore.Server;

internal sealed class SyncDataService(IMongoClient mongoClient, ILogger<SyncDataService> logger) 
    : ISyncDataService
{
    private const string CollectionName = "Items";
    private static readonly ConcurrentDictionary<int, Channel<SyncData>> Connections = new();

    #region Connect
    IAsyncEnumerable<SyncData> ISyncDataService.Connect(SyncConnectionRequest request, CallContext context)
    {
        logger.LogDebug("Rpc Connect");

        var channel = Channel.CreateUnbounded<SyncData>();

        Connections[request.IdClient] = channel;

        var stream = ReadMessages(channel.Reader, context.CancellationToken);

        //context.CancellationToken.Register(() =>
        //{
        //    channel.Writer.TryComplete();

        //    Connections.TryRemove(request.IdClient, out _);

        //    logger.LogInformation("Connection closed: {ClientId}", request.IdClient);
        //});

        return new CleanupAsyncEnumerable<SyncData>(
              stream,
              () =>
              {
                  Connections.TryRemove(request.IdClient, out _);

                  logger.LogInformation("Connection closed: {ClientId}", request.IdClient);

                  return ValueTask.CompletedTask;
              });
    }

    private async IAsyncEnumerable<SyncData> ReadMessages(ChannelReader<SyncData> reader, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var message in reader.ReadAllAsync(cancellationToken))
        {
            logger.LogTrace($"Send item (Connect): {message.Id}");
            yield return message;
        }
    }
    #endregion

   async IAsyncEnumerable<SyncData> ISyncDataService.GetData(SyncDataRequest request, CallContext context)
    {
        logger.LogDebug("Rpc GetData");

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
        logger.LogDebug("Rpc SendData");

        var headers = context.ServerCallContext?.RequestHeaders;
        var idUser = int.Parse(headers!.FirstOrDefault(x => x.Key == "idclient")!.Value!);

        var database = mongoClient.GetDatabase("Db1");
        var collection = database.GetCollection<MongoDocument>(CollectionName);

        await foreach(var item in datas)
        {
            logger.LogTrace($"Send item(SendData):{item.Id}");

            if (!Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType.TryGetValue(item.EntityType, out var syncDataType))
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


