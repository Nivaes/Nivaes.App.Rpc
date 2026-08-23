using Microsoft.EntityFrameworkCore;
using Nivaes.App.Rpc.AspNetCore.Server;

namespace Nivaes.App.RPC.Sample.Server;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.AddNpgsqlDbContext<ServerDatabaseContext>("dbAppPostgres",
                   settings => { },
                   options =>
                   {
                       options.UseNpgsql(o =>
                       {
                           o.EnableRetryOnFailure();
                       })
                       .LogTo(Console.WriteLine);
                   });

        builder.AddMongoDBClient(connectionName: "dbAppMongo");

        builder.Services.AddMagicOnion();

        var app = builder.Build();

        await app.InitializeLoadDatatest();

        app.MapMagicOnionService(typeof(SendSyncDataService).Assembly);

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