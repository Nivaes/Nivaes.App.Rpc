using Grpc.Core;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Nivaes.App.Rpc;

[Service]
public interface IEchoService
{   
    ValueTask<string> Echo(string message);

    ValueTask<string> EchoContext(string message, CallContext context = default);
}
