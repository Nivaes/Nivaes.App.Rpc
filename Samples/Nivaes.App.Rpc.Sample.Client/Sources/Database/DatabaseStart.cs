using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Nivaes.EntityFrameworkCore.Sqlite;

namespace Nivaes.App.RPC.Sample.Client;

public static class DatabaseStart
{
    //private static readonly VersionScriptMigration[] Migrations =
    //   [
    //       new(
    //            "20260819175954_InitialCreate",
    //            "Nivaes.App.RPC.Sample.Client.Sources.Database.Migrations.20260819175954_InitialCreate.sql"),

    //        //new(
    //        //    "20260818181000_AddStudent",
    //        //    "Nivaes.App.RPC.Sample.Client.MigrationScripts.20260818181000_AddStudent.sql"),

    //        //new(
    //        //    "20260818182000_AddEmail",
    //        //    "Nivaes.App.RPC.Sample.Client.MigrationScripts.20260818182000_AddEmail.sql")
    //   ];

    //public static async Task InitializeDatabase(string databasePath)
    //{
    //    //using var db = new DatabaseContext();
    //    //await db.Database.EnsureCreatedAsync();
    //    var assembly = typeof(DatabaseStart).Assembly;

    //    await ScriptDatabaseMigrator.MigrateAsync(databasePath, Migrations, assembly);
    //}

    public static async Task InitializeDatabase()
    {
        try
        {
            await CreateDatabase().ConfigureAwait(false);
        }
        catch (DbException)
        {
            using (var db = new DatabaseContext())
            {
                await db.Database.EnsureDeletedAsync().ConfigureAwait(false);
            }

            await CreateDatabase().ConfigureAwait(false);
        }
    }

    private static async Task CreateDatabase()
    {
        try
        {
            await using var db = new DatabaseContext();

            //await db.Database.MigrateAsync().ConfigureAwait(false);
            await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_NOTADB)
        {
        }
    }

    public static async Task ResetData()
    {
        using var db = new DatabaseContext();

        await db.Database.EnsureDeletedAsync().ConfigureAwait(false);
    }


}

