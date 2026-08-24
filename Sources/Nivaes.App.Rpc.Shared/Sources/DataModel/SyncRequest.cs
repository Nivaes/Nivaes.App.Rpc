using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncRequest
{
    [ProtoMember(1)]
    public long LastTimestampTicks { get; set; }
}
