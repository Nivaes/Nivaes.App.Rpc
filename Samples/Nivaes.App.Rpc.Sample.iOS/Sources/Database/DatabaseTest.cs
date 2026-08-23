using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Nivaes.App.RPC.Sample.Client;
using Nivaes.DataTestGenerator;

namespace Nivaes.App.RPC.Sample.iOS;

internal static class DatabaseTest
{
    public static async Task InitializeTest()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Console.WriteLine("!!! UnhandledException !!!");
            Console.WriteLine(e.ExceptionObject);
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Console.WriteLine("!!! UnobservedTaskException !!!");
            Console.WriteLine(e.Exception);
        };

        ObjCRuntime.Runtime.MarshalManagedException += (s, e) =>
        {
            Console.WriteLine("!!! MarshalManagedException !!!");
        };

        ObjCRuntime.Runtime.MarshalObjectiveCException += (s, e) =>
        {
            Console.WriteLine("!!! MarshalObjectiveCException !!!");
        };

        //await DatabaseStart.InitializeDatabase("Data Source=client.db");
        string fileDatabase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "client.db");
        DatabaseContext.databasePath = fileDatabase;

        //await DatabaseStart.InitializeDatabase(fileDatabase);
        await DatabaseStart.InitializeDatabase();

        await SaveUsers();
        await LoadUsers();

        Console.WriteLine("Hello, World!");
    }

    private static async Task SaveUsers()
    {
        await using DatabaseContext db = new DatabaseContext();

        var users = new List<UserDataModel>();

        for (int i = 1; i <= 1000; i++)
        {
            var contact = ContactGenerator.GenerateContact();

            var user = new UserDataModel
            {
                IdUser = Guid.NewGuid(),
                Identification = $"ID{i:00000}",
                Name = contact.SortName,
                GivenName = contact.GivenName,
                FamilyName = contact.FamilyName,
                Email = contact.Email,
                PhoneNumber = contact.TelephoneNumber
            };

            users.Add(user);
        }

        try
        {
            var aa = db.ChangeTracker;
            aa.AutoDetectChangesEnabled = false;

            db.Users.AddRange(users);

            await db.Users.AddRangeAsync(users);
            
            await db.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Console.WriteLine(ex.Message);

            throw;
        }
    }

    private static async Task LoadUsers()
    {
        await using var db = new DatabaseContext();

        //var usr = db.Users.AsAsyncEnumerable();
        var usr = await db.Users.ToArrayAsync();

        //await foreach (var user in usr)
        //{
        //    Console.WriteLine(user.Name);
        //}

        foreach (var user in usr)
        {
            Debug.WriteLine(user.Name);
        }
    }
}