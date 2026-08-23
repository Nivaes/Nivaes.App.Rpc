using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Logging;

namespace Nivaes.App.Rpc.AspNetCore.Server;

public class EchoService(ILogger<EchoService> logger) 
    : ServiceBase<IEchoService>,
        IEchoService
{
    public async UnaryResult<string> Echo(string message)
    {
        logger.LogInformation($"Echo{message}");
        return message;
    }
}
