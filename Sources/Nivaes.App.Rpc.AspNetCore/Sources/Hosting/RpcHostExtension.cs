using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nivaes.App.Rpc.AspNetCore.Server;

namespace Nivaes.App.Rpc.Client.Hosting
{
    public static class RpcHostExtension
    {
        internal static IServiceProvider? Services;

        public static IEndpointRouteBuilder InitializeRpcServer(this IEndpointRouteBuilder builder)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            Services = builder.ServiceProvider;

            builder.MapGrpcService<EchoService>().EnableGrpcWeb();
            builder.MapGrpcService<SyncDataService>().EnableGrpcWeb();

            return builder;
        }
    }
}
