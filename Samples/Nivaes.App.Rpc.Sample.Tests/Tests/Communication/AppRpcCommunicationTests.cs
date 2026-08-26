using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Aspire.Hosting.Testing;
using Grpc.Net.Client;
using MemoryPack;
using Microsoft.Extensions.Configuration;
using Nivaes.App.Rpc.Client;
using Nivaes.App.RPC.Sample;
using Nivaes.DataTestGenerator;
using ProtoBuf.Grpc.Client;
using AutoFixture.Xunit3;

namespace Nivaes.App.Rpc.Sample.Tests
{
    [Collection(nameof(AppApiRpcHostFixture))]
    public class AppRpcCommunicationTests
    {
        private readonly AppApiRpcHostFixture fixture;

        public AppRpcCommunicationTests(AppApiRpcHostFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ApiRpcService_Test()
        {
            var ct = CancellationToken.None;

            using var httpClient = fixture.GetHttpClient();

            using (var response = await httpClient.GetAsync("/health", ct))
            {
                response.IsSuccessStatusCode.ShouldBeTrue();
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
            }
        }

        [Theory, AutoData()]
        public async Task ApiRpc_Communication_Echo_Test(string message)
        {
            var ct = CancellationToken.None;

            var echoService = fixture.CreateGrpcService<IEchoService>();

            var messageEcho = await echoService.Echo(message);

            messageEcho.ShouldBe(message);
        }
         
        [Fact]
        public async Task ApiRpc_Communication_SyncData_Test()
        {
            var ct = CancellationToken.None;

            var syncDataService = fixture.CreateGrpcService<ISyncDataService>();

            async IAsyncEnumerable<SyncData> GetUsers()
            {
                var contact = ContactGenerator.GenerateContact();

                int i = 1;
                var item = new UserDataModel
                {
                    IdUser = Guid.NewGuid(),
                    Identification = $"ID{i:00000}",
                    Name = contact.SortName,
                    GivenName = contact.GivenName,
                    FamilyName = contact.FamilyName,
                    Email = contact.Email,
                    PhoneNumber = contact.TelephoneNumber
                };
                var itemData = MemoryPackSerializer.Serialize(item.GetType(), item);

                yield return new SyncData
                {
                    Id = item.Id,
                    Data = itemData,
                    EntityType = item.GetType().FullName!
                };
            }
            var users = GetUsers();

            await syncDataService.SendData(users);
        }
    }
}
