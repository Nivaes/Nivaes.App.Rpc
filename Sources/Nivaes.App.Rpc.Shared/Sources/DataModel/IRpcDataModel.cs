using System;
using System.Collections.Generic;
using System.Text;

namespace Nivaes.App.Rpc;

public interface IRpcDataModel
{
    Guid Id { get; }

    long TimeStampTicks { get; set; }
}
