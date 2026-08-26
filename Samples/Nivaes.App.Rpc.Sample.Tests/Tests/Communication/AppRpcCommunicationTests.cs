using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Aspire.Hosting.Testing;
using AutoFixture;
using AutoFixture.Xunit3;
using Grpc.Net.Client;
using MemoryPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine;
using Nivaes.App.Rpc.Client;
using Nivaes.App.RPC.Sample;
using Nivaes.DataTestGenerator;
using ProtoBuf.Grpc.Client;

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
            var items = GetUsers();

            await syncDataService.SendData(items, fixture.CancellationToken);

            var itemsCopy = syncDataService.GetData(new SyncRequest {  LastTimestampTicks = 0 }, fixture.CancellationToken);

            var syncDataCopy = await itemsCopy.FirstAsync();
            syncDataCopy.ShouldNotBeNull();

            var syncDataType = Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType[syncDataCopy.EntityType];
          
            var itemData = MemoryPackSerializer.Deserialize(syncDataType, syncDataCopy.Data);

            itemData.ShouldNotBeNull();
        }

        [Fact]
        public async Task ApiRpc_Communication_Connection_SyncData_Test()
        {
            var syncDataService = fixture.CreateGrpcService<ISyncDataService>();

            Task connectionTask = Task.Run(async() =>
            {
                var connection = syncDataService.Connect(new SyncConnection(), fixture.CancellationToken);

                await foreach (var item in connection)
                {
                    item.ShouldNotBeNull();

                    var syncDataType = Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType[item.EntityType];

                    var itemData = MemoryPackSerializer.Deserialize(syncDataType, item.Data);

                    itemData.ShouldNotBeNull();
                }
            });

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
            var items = GetUsers();

            await syncDataService.SendData(items, fixture.CancellationToken);
        }
    }
}
