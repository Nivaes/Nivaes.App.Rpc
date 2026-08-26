using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncDataRequest
{
    [ProtoMember(1)]
    required public int IdClient { get; set; } = 0;

    [ProtoMember(2)]
    public int IdChannel { get; set; } = 0;

    [ProtoMember(3)]
    required public long LastTimestampTicks { get; set; }
}
