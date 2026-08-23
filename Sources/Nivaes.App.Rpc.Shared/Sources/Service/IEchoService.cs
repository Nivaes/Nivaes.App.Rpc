using MagicOnion;

namespace Nivaes.App.Rpc;

public interface IEchoService : IService<IEchoService>
{

    UnaryResult<string> Echo(string message);
}
