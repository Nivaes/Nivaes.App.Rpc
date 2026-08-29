namespace Nivaes.App.Rpc.Sample.Tests
{
    [Collection(nameof(AppApiRpcHostFixture))]
    public class RpcSyncDataInterceptorTests
    {
        private readonly AppApiRpcHostFixture fixture;

        public RpcSyncDataInterceptorTests(AppApiRpcHostFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}
