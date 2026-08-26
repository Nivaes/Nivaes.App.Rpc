using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncConnection
{
    [ProtoMember(1)]
    public int UserId { get; set; } = 0;

    [ProtoMember(2)]
    public int ChatId { get; set; } = 0;
}
