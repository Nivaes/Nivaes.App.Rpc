using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nivaes.App.RPC.Client;

namespace Nivaes.App.RPC.Sample.Client;

public class RpcSyncDatabaseContextFactory
    : IDesignTimeDbContextFactory<RpcSyncDatabaseContext>
{
    public RpcSyncDatabaseContext CreateDbContext(string[] args)
    {
        SQLitePCL.Batteries_V2.Init();

        var options = new DbContextOptionsBuilder<RpcSyncDatabaseContext>();

        options.UseSqlite("Data Source=client.db");

        return new RpcSyncDatabaseContext(options.Options);
    }
}
