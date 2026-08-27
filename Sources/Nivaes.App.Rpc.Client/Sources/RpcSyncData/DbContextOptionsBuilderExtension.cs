using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nivaes.App.Rpc.Client.Hosting;
using Nivaes.App.Rpc.Client.RpcSyncData;

namespace Nivaes.App.Rpc.Client;

public static class DbContextOptionsBuilderExtension
{
    //public static DbContextOptionsBuilder RpcIntegration(this DbContextOptionsBuilder dbContextBuilder)
    //{
    //    dbContextBuilder.AddInterceptors(new RpcSyncDataInterceptor());
    //    return dbContextBuilder;
    //}

    //public static async Task ReceivedSyncData(DbContextOptionsBuilder dbContextBuilder)
    //{
    //    var signal = RpcHostExtension.Services!.GetRequiredService<RpcSyncDataSignal>();

    //    dbContextBuilder.Cre
    //}
}
