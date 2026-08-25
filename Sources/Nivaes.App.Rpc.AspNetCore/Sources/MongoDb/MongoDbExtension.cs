using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Nivaes.App.Rpc.AspNetCore.Server
{
    public static class MongoDbExtension
    {
        extension(IMongoCollection<MongoDocument> collection)
        {
            public async Task InsertOrUpdateOneAsync(MongoDocument document, CancellationToken cancellationToken = default)
            {
                //var models = collection.Select(document =>
                //    new ReplaceOneModel<MongoDocument>(
                //        Builders<MongoDocument>.Filter.Eq(x => x.Id, document.Id),
                //        document)
                //    {
                //        IsUpsert = true
                //    });

                //await collection.BulkWriteAsync(models);
                var filter = Builders<MongoDocument>.Filter.Eq(x => x.Id, document.Id);

                await collection.ReplaceOneAsync(
                    filter,
                    document,
                    new ReplaceOptions
                    {
                        IsUpsert = true
                    });
            }
        }
    }
}
