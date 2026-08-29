using System.Data.Common;
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
        internal static IServiceProvider? ServiceProvider;

        public static IHostApplicationBuilder AddRpcClient<TContext>(this IHostApplicationBuilder builder, Uri url, int idClient, 
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            builder.Services.AddPooledDbContextFactory<TContext>((sp, optionsbuilder) =>
            {
                optionsAction.Invoke(sp, optionsbuilder);

                var rpcSyncDbFactory = sp.GetRequiredService<IDbContextFactory<RpcSyncDatabaseContext>>();
                var rpcSyncDataSignal = sp.GetRequiredService<RpcSyncDataSignal>();

                optionsbuilder.AddInterceptors(new RpcSyncDataInterceptor(rpcSyncDbFactory, rpcSyncDataSignal));
            });

            builder.Services.AddPooledDbContextFactory<RpcSyncDatabaseContext>(options =>
            {
                options.UseSqlite("Data Source=syncache.db");
            });

            builder.Services.AddSingleton<GrpcChannel>(sp =>
            {
                return GrpcChannel.ForAddress(url, new GrpcChannelOptions
                {
                    HttpHandler = new SocketsHttpHandler
                    {
                        KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
                        EnableMultipleHttp2Connections = true
                    }
                });
            });

            builder.Services.AddSingleton<ISyncDataService>(sp =>
            {
                var channel = sp.GetRequiredService<GrpcChannel>();

                return channel.CreateGrpcService<ISyncDataService>();
            });

            builder.Services.AddSingleton<RpcSyncDataSignal>();
            builder.Services.AddHostedService<RpcSyncDataWorker<TContext>>();
            builder.Services.AddSingleton<SyncClientConfiguration>(new SyncClientConfiguration { IdClient = idClient });

            return builder;
        }

        public static async Task<IServiceProvider> InitializeRpcClientAsync(this IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<RpcSyncDatabaseContext>>();

                await using var db = await factory.CreateDbContextAsync();

                try
                {
                    await db.Database.MigrateAsync();
                }
                catch (DbException)
                {
                    await db.Database.EnsureDeletedAsync();

                    await db.Database.MigrateAsync();
                }
            }

            return serviceProvider;
        }
    }
}
