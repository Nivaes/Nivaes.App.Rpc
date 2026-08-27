using System.Net;
using AutoFixture.Xunit3;
using Grpc.Core;
using MemoryPack;
using Microsoft.AspNetCore.Components.Web;
using Nivaes.App.RPC.Sample;
using Nivaes.DataTestGenerator;


namespace Nivaes.App.Rpc.Sample.Tests
{
    [Collection(nameof(AppApiRpcHostFixture))]
    public class AppRpcCommunicationTests
    {
        private readonly AppApiRpcHostFixture fixture;
        private readonly ITestOutputHelper output;

        public AppRpcCommunicationTests(AppApiRpcHostFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.output = output;
       
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
                    PhoneNumber = contact.TelephoneNumber,
                    TimeStamp = DateTime.UtcNow
                };
                var itemData = MemoryPackSerializer.Serialize(item.GetType(), item);

                yield return new SyncData
                {
                    Id = item.Id,
                    Data = itemData,
                    EntityType = item.GetType().FullName!,
                    TimeStampTicks = DateTime.UtcNow.Ticks
                };
            }
            var items = GetUsers();

            var requestSend = new SyncDataRequest
            {
                IdClient = 1,
                LastTimestampTicks = DateTime.UtcNow.Ticks
            };

            await syncDataService.SendData(items,
                  new ProtoBuf.Grpc.CallContext(
                    new CallOptions(
                        headers: new Metadata { { "IdUser", "1" } },
                        //credentials: 
                        cancellationToken: fixture.CancellationToken)));

            var requestGet = new SyncDataRequest
            {
                IdClient = 1,
                LastTimestampTicks = DateTime.UtcNow.Ticks
            };

            var itemsCopy = syncDataService.GetData(requestGet, fixture.CancellationToken);

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

            var connectionTask = Task.Run(async() =>
            {
                var requestSend = new SyncConnectionRequest
                {
                    IdClient = 1
                };
                var connection = syncDataService.Connect(requestSend, fixture.CancellationToken);

                await foreach (var item in connection)
                {
                    item.ShouldNotBeNull();

                    var syncDataType = Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType[item.EntityType];

                    var itemData = MemoryPackSerializer.Deserialize(syncDataType, item.Data);

                    itemData.ShouldNotBeNull();
                    var user = (UserDataModel)itemData;

                    user.ShouldNotBeNull();
                    user.Name.ShouldNotBeNull();
                    output.WriteLine($"{user.Identification}:{user.Name}");
                }
            });
            await Task.Delay(50);

            async IAsyncEnumerable<SyncData> GetUsers()
            {
                for (int i = 1; i <= 10; i++)
                {
                    var contact = ContactGenerator.GenerateContact();

                    var item = new UserDataModel
                    {
                        IdUser = Guid.NewGuid(),
                        Identification = $"ID{i:00000}",
                        Name = contact.SortName,
                        GivenName = contact.GivenName,
                        FamilyName = contact.FamilyName,
                        Email = contact.Email,
                        PhoneNumber = contact.TelephoneNumber,
                        TimeStamp = DateTime.UtcNow
                    };
                    var itemData = MemoryPackSerializer.Serialize(item.GetType(), item);

                    yield return new SyncData
                    {
                        Id = item.Id,
                        Data = itemData,
                        EntityType = item.GetType().FullName!,
                        TimeStampTicks = DateTime.UtcNow.Ticks
                    };
                }
            }
            var items = GetUsers();
            var requestSend = new SyncDataRequest
            {
                IdClient = 1,
                LastTimestampTicks = DateTime.UtcNow.Ticks
            };

            await syncDataService.SendData(items,
                  new ProtoBuf.Grpc.CallContext(
                    new CallOptions(
                        headers: new Metadata { { "IdUser", "1" } },
                        //credentials: 
                        cancellationToken: fixture.CancellationToken)));
        }
    
        [Fact]
        public async Task ApiRpc_Communication_Multi_Connection_SyncData_Test()
        {
            var syncDataService = fixture.CreateGrpcService<ISyncDataService>();

            async Task Connection(int i)
            {
                var requestSend = new SyncConnectionRequest
                {
                    IdClient = i
                };
                var connection = syncDataService.Connect(requestSend, fixture.CancellationToken);

                try
                {
                    await foreach (var item in connection)
                            //.WithCancellation(fixture.CancellationToken))
                    {
                        item.ShouldNotBeNull();

                        var syncDataType = Singleton<RpcDataModelsTypeContainer>.Instance.RpcDataModelsType[item.EntityType];

                        var itemData = MemoryPackSerializer.Deserialize(syncDataType, item.Data);

                        itemData.ShouldNotBeNull();
                        var user = (UserDataModel)itemData;

                        user.ShouldNotBeNull();
                        user.Name.ShouldNotBeNull();

                        output.WriteLine($"{i} - {user.Identification}:{user.Name}");
                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine(ex.ToString());
                }
            }
            
            var taskConnection1 = Task.Run(async () => await Connection(1));
            var taskConnection2 = Task.Run(async () => await Connection(2));
            var taskConnection3 = Task.Run(async () => await Connection(3));
            var taskConnection4 = Task.Run(async () => await Connection(4));

            await Task.Delay(5000);

            async IAsyncEnumerable<SyncData> GetUsers()
            {
                for (int i = 1; i <= 3; i++)
                {
                    var contact = ContactGenerator.GenerateContact();

                    var item = new UserDataModel
                    {
                        IdUser = Guid.NewGuid(),
                        Identification = $"ID{i:00000}",
                        Name = contact.SortName,
                        GivenName = contact.GivenName,
                        FamilyName = contact.FamilyName,
                        Email = contact.Email,
                        PhoneNumber = contact.TelephoneNumber,
                        TimeStamp = DateTime.UtcNow
                    };
                    var itemData = MemoryPackSerializer.Serialize(item.GetType(), item);

                    await Task.Delay(100);

                    yield return new SyncData
                    {
                        Id = item.Id,
                        Data = itemData,
                        EntityType = item.GetType().FullName!,
                        TimeStampTicks = DateTime.UtcNow.Ticks
                    };
                }
            }
            var items = GetUsers();
            var requestSend = new SyncDataRequest
            {
                IdClient = 1,
                LastTimestampTicks = DateTime.UtcNow.Ticks
            };

            await syncDataService.SendData(items, 
                new ProtoBuf.Grpc.CallContext(
                    new CallOptions(
                        headers: new Metadata { { "IdUser", "1" } },
                        cancellationToken: fixture.CancellationToken)));
        }
    }
}
