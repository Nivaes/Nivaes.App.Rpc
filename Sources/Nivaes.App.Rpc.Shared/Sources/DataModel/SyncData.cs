using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
internal class SyncData
{
    [ProtoMember(1)]
    required public Guid Id { get; set; }

    [ProtoMember(2)]
    required public string EntityType { get; set; }

    [ProtoMember(3)]
    required public byte[] Data { get; set; } = [];

    [ProtoIgnore]
    public DateTime TimeStamp
    {
        get => DateTime.FromBinary(TimeStampTicks);
        set => TimeStampTicks = value.Ticks;
    }

    [ProtoMember(4)]
    required public long TimeStampTicks { get; set; }
}
