using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Aspire.Hosting.Testing;
using Nivaes.App.Rpc.Client;

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

        [Fact]
        public async Task ApiRpcInterceptor_Test()
        {
            var ct = CancellationToken.None;

            using var httpClient = fixture.GetHttpClient();

            var rpcSyncDataInterceptor = new RpcSyncDataInterceptor();
        }
    }
}
