using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nivaes.App.Rpc.Client.Hosting;
using Nivaes.App.Rpc.Sample.Client;
using Nivaes.App.Rpc.Sample.Worker;
using Nivaes.DataTestGenerator;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("Microsoft", LogLevel.Information);
builder.Logging.AddFilter("Nivaes", LogLevel.Trace);

var url = builder.Configuration["services:RpcSampleSerice:Grpc:0"];
var databasePath = "client.db";

builder.AddRpcClient<DatabaseContext>(new Uri(url!), idTenant: 100, idClient: 4, (sp, optionAction) =>
{
    optionAction.UseSqlite($"Data Source={databasePath}");
});

builder.Services.AddHostedService<Worker>();

Nivaes.App.Rpc.Sample.GeneratedRegisterRpcDataModelsExtensions.RegisterRpcDataModelsActions();

builder.AddServiceDefaults();

var host = builder.Build();

await host.Services.InitializeRpcClientAsync();

await DatabaseStart.InitializeDatabase(host);

await host.RunAsync();

