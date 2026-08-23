using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace Nivaes.App.Rpc;

[ProtoContract]
public class SyncData
{
    [ProtoMember(1)]
    public Guid Id { get; set; }

    [ProtoMember(2)]
    public string EntityType { get; set; } = "";

    [ProtoMember(3)]
    public byte[] Data { get; set; } = [];

    [ProtoIgnore]
    public DateTime TimeStamp
    {
        get => DateTime.FromBinary(TimeStampTicks);
        set => TimeStampTicks = value.Ticks;
    }

    [ProtoMember(4)]
    public long TimeStampTicks { get; set; }
}
