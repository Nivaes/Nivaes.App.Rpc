using System.ServiceModel;
using Grpc.Core;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Nivaes.App.Rpc;

[Service]
public interface ISyncDataService
{
    ValueTask<SyncResult> SendData(IAsyncEnumerable<SyncData> datas, CallContext context = default);

    IAsyncEnumerable<SyncData> GetData(CallContext context = default);
}
