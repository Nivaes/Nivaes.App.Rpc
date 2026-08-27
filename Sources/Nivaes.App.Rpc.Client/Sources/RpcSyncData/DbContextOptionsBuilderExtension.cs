using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Nivaes.App.Rpc.Client;

public static class DbContextOptionsBuilderExtension
{
    public static DbContextOptionsBuilder RpcIntegration(this DbContextOptionsBuilder dbContextBuilder)
    {
        dbContextBuilder.AddInterceptors(new RpcSyncDataInterceptor());
        return dbContextBuilder;
    }
}
