using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncDataResult
{
    [ProtoMember(1)]
    public bool Success { get; set; }
}
