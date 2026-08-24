using System.ServiceModel;
using Grpc.Core;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Nivaes.App.Rpc;

[Service]
public interface ISyncDataService
{
    IAsyncEnumerable<SyncData> Connect(SyncConnection request, CallContext context = default);

    IAsyncEnumerable<SyncData> GetData(SyncRequest request, CallContext context = default);

    ValueTask<SyncResult> SendData(IAsyncEnumerable<SyncData> datas, CallContext context = default);    
}
