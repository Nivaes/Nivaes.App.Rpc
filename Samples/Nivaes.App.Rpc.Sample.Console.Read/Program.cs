using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nivaes.App.Rpc.Client.Hosting;
using Nivaes.App.Rpc.Sample.Client;
using Nivaes.DataTestGenerator;

namespace Nivaes.App.Rpc.Sample.Reader;

internal static class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddFilter("Microsoft", LogLevel.Information);
        builder.Logging.AddFilter("Nivaes", LogLevel.Trace);
        builder.Logging.AddConsole();

        var url = builder.Configuration["services:RpcSampleSerice:Grpc:0"];

        builder.AddRpcClient<DatabaseContext>(new Uri(url!), 2);

        GeneratedRegisterRpcDataModelsExtensions.RegisterRpcDataModelsActions();
        //RpcDataModelTypeContainerHelper.RegisterRpcDataModels([
        //    RpcDataModelTypeContainerHelper.New<UserDataModel>()
        //]);

        using var host = builder.Build();

        await host.InitializeRpcClientAsync();

        await host.StartAsync();

        await Task.Delay(1000);

        try
        {
            await DatabaseStart.InitializeDatabase(host);

            //await SaveUsers(host);
            await Task.Delay(6000);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    //private static async Task SaveUsers(IHost host)
    //{
    //    //await using DatabaseContext db = new DatabaseContext();
    //    var factory = host.Services.GetService<IDbContextFactory<DatabaseContext>>();

    //    await using var db = await factory!.CreateDbContextAsync();

    //    var users = new List<UserDataModel>();

    //    for (int i = 1; i <= 1; i++)
    //    {
    //        var contact = ContactGenerator.GenerateContact();

    //        var user = new UserDataModel
    //        {
    //            IdUser = Guid.NewGuid(),
    //            Identification = $"ID{i:00000}",
    //            Name = contact.SortName,
    //            GivenName = contact.GivenName,
    //            FamilyName = contact.FamilyName,
    //            Email = contact.Email,
    //            PhoneNumber = contact.TelephoneNumber
    //        };

    //        users.Add(user);
    //    }

    //    await db.Users.AddRangeAsync(users);
    //    await db.SaveChangesAsync();
    //}
}