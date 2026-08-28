using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nivaes.App.Rpc.Sample.Client;

public static class DatabaseStart
{
    public static async Task InitializeDatabase(IHost host)
    {
        try
        {
            await CreateDatabase(host).ConfigureAwait(false);
        }
        catch (DbException)
        {
            var factory = host.Services.GetService<IDbContextFactory<DatabaseContext>>();

            await using var db = await factory!.CreateDbContextAsync();

            //using (var db = new DatabaseContext())
            //{
                await db.Database.EnsureDeletedAsync().ConfigureAwait(false);
            //}

            await CreateDatabase(host).ConfigureAwait(false);
        }
    }

    private static async Task CreateDatabase(IHost host)
    {
        try
        {
            var factory = host.Services.GetService<IDbContextFactory<DatabaseContext>>();

            await using var db = await factory!.CreateDbContextAsync();
            //await using var db = new DatabaseContext();

            //await db.Database.MigrateAsync().ConfigureAwait(false);
            await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_NOTADB)
        {
        }
    }

    public static async Task ResetData(IHost host)
    {
        //using var db = new DatabaseContext();
        var factory = host.Services.GetService<IDbContextFactory<DatabaseContext>>();

        await using var db = await factory!.CreateDbContextAsync();

        await db.Database.EnsureDeletedAsync().ConfigureAwait(false);
    }


}

