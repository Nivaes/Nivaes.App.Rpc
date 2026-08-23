using System.ServiceModel;
using Grpc.Core;
using ProtoBuf.Grpc.Configuration;

namespace Nivaes.App.Rpc;

[Service]
public interface ISyncDataContract
{
    [Operation]
    //ValueTask<string> Echo(string message, ServerCallContext? context = default);
    //[Operation]
    ValueTask<string> Echo(string message);

    //ValueTask<SyncData> GetData(IAsyncStreamReader<SyncData> requestStream, ServerCallContext context = default);
    ValueTask<SyncData> GetData(IAsyncStreamReader<SyncData> requestStream);
}
