using System;
using System.Collections.Generic;
using System.Text;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nivaes.App.Cross;
using Nivaes.App.RPC.Sample;
using ProtoBuf.Grpc.Client;

namespace Nivaes.App.Rpc.Sample.Tests
{
    public class AppApiRpcHostFixture : IAsyncLifetime
    {
        public IDistributedApplicationTestingBuilder? AppHost;
        public DistributedApplication? App;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(300);

        private CancellationTokenSource CancellationTokenSource = new();
        public CancellationToken CancellationToken;

        public AppApiRpcHostFixture()
        {
            CancellationToken = CancellationTokenSource.Token;
        }

        public async ValueTask InitializeAsync()
        {
            RpcDataModelTypeContainerHelper.RegisterCombiners([
                RpcDataModelTypeContainerHelper.New<UserDataModel>()
            ]);

            AppHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Nivaes_App_Rpc_Sample_AppHost>();

            AppHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddFilter(AppHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
                logging.AddConsole();
            });

            AppHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            App = await AppHost.BuildAsync(CancellationToken)
                .WaitAsync(DefaultTimeout, CancellationToken);

            await App.StartAsync()
                .WaitAsync(DefaultTimeout, CancellationToken);
        }

        public HttpClient GetHttpClient()
        {
            return App!.CreateHttpClient("RpcSampleSerice");
        }

        public TService CreateGrpcService<TService>()
            where TService : class
        {
            var httpClient = App!.CreateHttpClient("RpcSampleSerice", "grpc");

            var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!);

            return channel.CreateGrpcService<TService>();
        }

        public async ValueTask DisposeAsync()
        {
            await App!.DisposeAsync();
            await AppHost!.DisposeAsync();

            await CancellationTokenSource.CancelAsync();
        }
    }
}
