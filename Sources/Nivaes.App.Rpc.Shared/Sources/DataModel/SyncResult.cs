using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncResult
{
    [ProtoMember(1)]
    public bool Success { get; set; }
}
