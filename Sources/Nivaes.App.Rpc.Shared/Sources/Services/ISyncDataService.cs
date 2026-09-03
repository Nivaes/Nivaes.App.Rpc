using System.ServiceModel;
using Grpc.Core;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Nivaes.App.Rpc;

[Service("rpc.sync.data")]
public interface ISyncDataService
{
    IAsyncEnumerable<SyncData> Connect(SyncConnectionRequest request, CallContext context = default);

    IAsyncEnumerable<SyncData> GetData(SyncDataRequest request, CallContext context = default);

    ValueTask<SyncDataResult> SendData(IAsyncEnumerable<SyncData> datas, CallContext context = default);
}
