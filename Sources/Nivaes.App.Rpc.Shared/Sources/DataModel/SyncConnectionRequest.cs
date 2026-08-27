using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncConnectionRequest
{
    [ProtoMember(1)]
    required public int IdClient { get; set; };

    [ProtoMember(2)]
    public int IdChannel { get; set; } = 0;
}
