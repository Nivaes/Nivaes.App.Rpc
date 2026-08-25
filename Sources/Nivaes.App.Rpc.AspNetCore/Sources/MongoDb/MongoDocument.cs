using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace Nivaes.App.Rpc.AspNetCore.Server;

public class MongoDocument
{
    required public Guid Id { get; set; }

    //required public string EntityType { get; set; }

    //required public BsonBinaryData DataItem { get; set; }
    
    required public IRpcDataModel? DataItem { get; set; }

    required public long TimeStampTicks { get; set; }
}
