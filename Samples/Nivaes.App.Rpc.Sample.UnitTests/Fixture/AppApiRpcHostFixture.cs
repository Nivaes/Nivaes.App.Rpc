using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc.Client;

namespace Nivaes.App.Rpc.Sample.Tests
{
    public class AppApiRpcHostFixture : IAsyncLifetime
    {
        public IDistributedApplicationTestingBuilder? AppHost;
        public DistributedApplication? App;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

        private CancellationTokenSource CancellationTokenSource = new();
        public CancellationToken CancellationToken;

        public AppApiRpcHostFixture()
        {
            CancellationToken = CancellationTokenSource.Token;
        }

        public async ValueTask InitializeAsync()
        {
            RpcDataModelTypeContainerHelper.RegisterRpcDataModels([
                RpcDataModelTypeContainerHelper.New<UserDataModel>()
            ]);

            AppHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Nivaes_App_Rpc_Sample_AppHost>();

            AppHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddFilter(AppHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
                logging.AddFilter("Nivaes", LogLevel.Trace);
                logging.AddConsole();
            });

            AppHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            App = await AppHost.BuildAsync(CancellationToken)
                .WaitAsync(DefaultTimeout, CancellationToken);

            try{
                Console.WriteLine("Starting Aspire...");
                    await App.StartAsync()
                        .WaitAsync(DefaultTimeout, CancellationToken);
                Console.WriteLine("Aspire started.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Aspire START FAILED");
                Console.WriteLine(ex);

                Console.WriteLine("Resources:");

                foreach (var resource in AppHost.Resources)
                {
                    Console.WriteLine(
                        $"{resource.Name} - {resource.GetType().Name}");
                }

                throw;
            }
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
