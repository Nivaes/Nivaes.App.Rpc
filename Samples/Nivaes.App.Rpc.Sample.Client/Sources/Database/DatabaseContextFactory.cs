using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nivaes.App.RPC.Sample.Client;

public class DatabaseContextFactory
    : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>();

        options.UseSqlite("Data Source=client.db");

        return new DatabaseContext(options.Options);
    }
}
