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
        internal static IServiceProvider? Services;

        public static IHostApplicationBuilder AddRpcClient<TContext>(this IHostApplicationBuilder builder, Uri url, int idClient, 
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            //var databasePath = "client.db";

            builder.Services.AddPooledDbContextFactory<TContext>((sp, oa) =>
            {
                optionsAction.Invoke(sp, oa);

                oa.AddInterceptors(new RpcSyncDataInterceptor());
                //.UseSqlite($"Data Source={databasePath}")

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

        public static async Task<IHost> InitializeRpcClientAsync(this IHost host)
        {
            Services = host.Services;

            await using (var scope = host.Services.CreateAsyncScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<RpcSyncDatabaseContext>>();

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
