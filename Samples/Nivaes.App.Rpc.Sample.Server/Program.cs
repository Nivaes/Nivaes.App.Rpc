using Nivaes.App.Rpc.Client.Hosting;
using ProtoBuf.Grpc.Server;

namespace Nivaes.App.Rpc.Sample.Server;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        //builder.AddNpgsqlDbContext<ServerDatabaseContext>("DbAppSample",
        //           settings => { },
        //           options =>
        //           {
        //               options.UseNpgsql(o =>
        //               {
        //                   o.EnableRetryOnFailure();
        //               })
        //               .LogTo(Console.WriteLine);
        //           });

        builder.AddMongoDBClient(connectionName: "DbAppMongo");

        // Add services to the container.
        //builder.Services.AddGrpc();
        builder.Services.AddCodeFirstGrpc();
        //builder.Services.AddHealthChecks();

        Nivaes.App.Rpc.Sample.GeneratedRegisterRpcDataModelsExtensions.RegisterRpcDataModelsActions();
        //RpcDataModelTypeContainerHelper.RegisterRpcDataModels([
        //    RpcDataModelTypeContainerHelper.New<UserDataModel>()
        //]);

        var app = builder.Build();

        //await app.InitializeLoadDatatest();

        // Configure the HTTP request pipeline.
        //app.MapGrpcService<EchoService>().EnableGrpcWeb();
        //app.MapGrpcService<SyncDataService>().EnableGrpcWeb();
        app.InitializeRpcServer();

        //app.UseRouting();
        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapGrpcService<SendSyncDataService>();
        //});

        //app.MapGet("/test", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        //app.MapGet("/test", () => "OK");

        app.MapDefaultEndpoints();

        app.Run();
    }
}