using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncConnection
{
    [ProtoMember(1)]
    public string UserId { get; set; } = string.Empty;

    [ProtoMember(2)]
    public string ChatId { get; set; } = string.Empty;
}
