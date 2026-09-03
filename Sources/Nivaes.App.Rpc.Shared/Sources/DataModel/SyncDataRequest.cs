using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncDataRequest
{
    [ProtoMember(1)]
    required public int IdTenant { get; set; }

    [ProtoMember(2)]
    required public int IdClient { get; set; }

    [ProtoMember(3)]
    public int IdChannel { get; set; }

    [ProtoMember(4)]
    required public long LastTimestampTicks { get; set; }
}
