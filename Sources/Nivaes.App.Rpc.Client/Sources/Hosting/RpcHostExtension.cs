using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nivaes.App.Rpc.Client.RpcSyncData;
using Nivaes.App.RPC.Client;
using ProtoBuf.Grpc.Client;

namespace Nivaes.App.Rpc.Client.Hosting
{
    public static class RpcHostExtension
    {
        public static IHostApplicationBuilder AddRpcClient(this IHostApplicationBuilder builder, Uri url)
        {
            builder.Services.AddPooledDbContextFactory<RpcSyncDatabaseContext>(options =>
            {
                options.UseSqlite("Data Source=syncache.db");
            });

            builder.Services.AddSingleton<ISyncDataService>(sp =>
            {
                var channel = GrpcChannel.ForAddress(url);

                return channel.CreateGrpcService<ISyncDataService>();
            });

            builder.Services.AddSingleton<RpcSyncDataSignal>();
            builder.Services.AddHostedService<RpcSyncDataWorker>();

            return builder;
        }

        public static async Task<IHost> InitializeRpcClientAsync(this IHost host)
        {
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var factory = scope.ServiceProvider
                    .GetRequiredService<IDbContextFactory<RpcSyncDatabaseContext>>();

                await using var db = await factory.CreateDbContextAsync();

                try
                {
                    await db.Database.EnsureCreatedAsync();
                }
                catch (DbException)
                {
                    await db.Database.EnsureDeletedAsync();

                    await db.Database.EnsureCreatedAsync();
                }
            }

            return host;
        }
    }
}
