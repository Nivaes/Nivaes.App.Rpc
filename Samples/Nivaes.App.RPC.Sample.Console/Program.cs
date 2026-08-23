using System.Diagnostics;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nivaes.App.Rpc;
using Nivaes.App.Rpc.Client.Hosting;
using Nivaes.App.RPC.Sample.Client;
using Nivaes.DataTestGenerator;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nivaes.App.RPC.Sample;

internal static class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Servicios
        //builder.Services.AddSingleton<MyService>();

        // Logging
        builder.Logging.AddConsole();

        var url = builder.Configuration["services:SampleServer:Grpc:0"];

        builder.AddRpcClient(new Uri(url!));

        using var host = builder.Build();

        await host.InitializeRpcClientAsync();

        await host.StartAsync();

        try
        {
            await DatabaseStart.InitializeDatabase();

            await SaveUsers();
        }
        finally
        {
            await host.StartAsync();
        }
        //var service = host.Services.GetRequiredService<MyService>();
        //await service.RunAsync();

        //await DatabaseStart.InitializeDatabase("Data Source=client.db");
        //await DatabaseStart.InitializeDatabase("client.db");
        //await LoadUsers();

        //GrpcClientFactory.AllowUnencryptedHttp2 = true;
        //var innerHandler = new SocketsHttpHandler();

        //var grpcWebHandler = new GrpcWebHandler(
        //    GrpcWebMode.GrpcWeb,
        //    innerHandler);
        //var httpClient = new HttpClient(grpcWebHandler);

        
        //using var channel = GrpcChannel.ForAddress(url!,
        //    new GrpcChannelOptions
        //    {
        //        HttpClient = httpClient
        //    });
        //using var channel = GrpcChannel.ForAddress(url!);

        ////GrpcChannel channel = await GetGrpcChannel().ConfigureAwait(false);
        
        //var echoService = channel.CreateGrpcService<IEchoService>();
        //var message = await echoService.Echo("Message");

        //using var cancel = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        //var options = new CallOptions(cancellationToken: cancel.Token);
        //var message2 = await echoService.Echo("Message", new CallContext(options));

        //var syncService = channel.CreateGrpcService<ISyncDataService>();
        //await syncService.SendData(GetData()/*, new CallContext(options)*/);

        //Console.Write(message);
    }

    private static async IAsyncEnumerable<SyncData> GetData()
    {
        yield return new SyncData
        {
            Id = Guid.NewGuid(),
            Data = []
        };

        await Task.Delay(1000);

        yield return new SyncData
        {
            Id = Guid.NewGuid(),
            Data = []
        };

        await Task.Delay(1000);

        yield return new SyncData
        {
            Id = Guid.NewGuid(),
            Data = []
        };

        await Task.Delay(1000);

        await Task.CompletedTask;
    }

    private static async Task SaveUsers()
    {
        await using DatabaseContext db = new DatabaseContext();

        var users = new List<UserDataModel>();

        for (int i = 1; i <= 1000; i++)
        {
            var contact = ContactGenerator.GenerateContact();

            var user = new UserDataModel
            {
                IdUser = Guid.NewGuid(),
                Identification = $"ID{i:00000}",
                Name = contact.SortName,
                GivenName = contact.GivenName,
                FamilyName = contact.FamilyName,
                Email = contact.Email,
                PhoneNumber = contact.TelephoneNumber
            };

            users.Add(user);
        }

        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();
    }

    private static async Task LoadUsers()
    {
        using DatabaseContext db = new DatabaseContext();

        //var usr = db.Users.AsAsyncEnumerable();
        var usr = await db.Users.ToArrayAsync();


        //await foreach (var user in usr)
        //{
        //    Console.WriteLine(user.Name);
        //}

        foreach (var user in usr)
        {
            Console.WriteLine(user.Name);
        }
    }
}