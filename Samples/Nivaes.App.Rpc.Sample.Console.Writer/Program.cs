using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nivaes.App.Cross;
using Nivaes.App.Rpc.Client.Hosting;
using Nivaes.App.RPC.Sample.Client;
using Nivaes.DataTestGenerator;

namespace Nivaes.App.RPC.Sample;

internal static class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddFilter("Microsoft", LogLevel.Information);
        builder.Logging.AddFilter("Nivaes", LogLevel.Trace);

        //var url = builder.Configuration["services:RpcSampleSerice:Grpc:0"];
        var url = "https://localhost:7121";

        builder.AddRpcClient<DatabaseContext>(new Uri(url!));

        RpcDataModelTypeContainerHelper.RegisterCombiners([
            RpcDataModelTypeContainerHelper.New<UserDataModel>()
        ]);

        using var host = builder.Build();

        await host.InitializeRpcClientAsync();

        await host.StartAsync();

        await Task.Delay(1000);

        try
        {
            await DatabaseStart.InitializeDatabase(host);

            await SaveUsers(host);

            await Task.Delay(60000);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static async Task SaveUsers(IHost host)
    {
        int i = 0;
        while (true)
        {
            var factory = host.Services.GetService<IDbContextFactory<DatabaseContext>>();

            await using var db = await factory!.CreateDbContextAsync();
    
            var contact = ContactGenerator.GenerateContact();

            var user = new UserDataModel
            {
                IdUser = Guid.NewGuid(),
                Identification = $"ID{i++:00000}",
                Name = contact.SortName,
                GivenName = contact.GivenName,
                FamilyName = contact.FamilyName,
                Email = contact.Email,
                PhoneNumber = contact.TelephoneNumber
            };

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            Console.WriteLine($"Write object {user.Identification}");

            Console.ReadLine();
        
        }

    }
}