namespace Nivaes.App.Rpc;

public interface IRpcDataModel
{
    Guid Id { get; }

    long DateTimeStampTicks { get; set; }

    long? DeleteDateTimeStampTicks { get; set; }
}
