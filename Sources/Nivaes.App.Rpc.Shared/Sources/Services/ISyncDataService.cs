using System.ServiceModel;
using Grpc.Core;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Nivaes.App.Rpc;

[Service]
public interface ISyncDataService
{
    IAsyncEnumerable<SyncData> GetData(long lastTimestampTicks, CallContext context = default);

    ValueTask<SyncResult> SendData(IAsyncEnumerable<SyncData> datas, CallContext context = default);    
}
