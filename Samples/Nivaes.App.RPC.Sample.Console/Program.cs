using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nivaes.App.Rpc;
using Nivaes.App.RPC.Sample.Client;
using Nivaes.DataTestGenerator;

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

        using var host = builder.Build();

        //var service = host.Services.GetRequiredService<MyService>();
        //await service.RunAsync();

        //await DatabaseStart.InitializeDatabase("Data Source=client.db");
        //await DatabaseStart.InitializeDatabase("client.db");
        await DatabaseStart.InitializeDatabase();

        await SaveUsers();
        //await LoadUsers();

        //GrpcClientFactory.AllowUnencryptedHttp2 = true;

        var url = builder.Configuration["services:SampleServer:Grpc:0"];
        //var url = builder.Configuration["services:SampleServer:https:0"];

        var channel = GrpcChannel.ForAddress(url!);
        var service = MagicOnionClient.Create<ISyncDataService>(channel);

        var message = await service.Echo("Message");

        Console.Write(message);
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