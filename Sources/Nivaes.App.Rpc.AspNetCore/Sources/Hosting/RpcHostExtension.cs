using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ProtoBuf.Grpc.Server;

namespace Nivaes.App.Rpc.AspNetCore.Server.Hosting
{
    public static class RpcHostExtension
    {
        internal static IServiceProvider? ServiceProvider;

        public static IHostApplicationBuilder AddRpcService(this IHostApplicationBuilder builder, string mongoConnectionName)  
        {
            builder.AddMongoDBClient(connectionName: mongoConnectionName);
            builder.Services.AddCodeFirstGrpc();

            return builder;
        }

        public static IEndpointRouteBuilder InitializeRpcServer(this IEndpointRouteBuilder builder)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            ServiceProvider = builder.ServiceProvider;

            builder.MapGrpcService<EchoService>().EnableGrpcWeb();
            builder.MapGrpcService<SyncDataService>().EnableGrpcWeb();

            return builder;
        }
    }
}
