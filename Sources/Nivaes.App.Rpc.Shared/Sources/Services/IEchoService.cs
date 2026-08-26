using Grpc.Core;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace Nivaes.App.Rpc;

[Service("rpc.echo")]
public interface IEchoService
{   
    ValueTask<string> Echo(string message, CallContext context = default);
}
