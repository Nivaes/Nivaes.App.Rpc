using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;

namespace Nivaes.App.Rpc.AspNetCore.Server;

public class EchoService(ILogger<EchoService> logger) 
    : IEchoService
{
    public ValueTask<string> Echo(string message, CallContext context = default)
    {
        if (context.CancellationToken.IsCancellationRequested)
            return ValueTask.FromResult(string.Empty);

        logger.LogInformation($"Echo{message}");
        return ValueTask.FromResult(message);
    }
}
