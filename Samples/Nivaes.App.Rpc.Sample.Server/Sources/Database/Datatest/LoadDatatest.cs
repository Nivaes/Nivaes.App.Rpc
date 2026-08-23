using Microsoft.EntityFrameworkCore;
using Nivaes.DataTestGenerator;

namespace Nivaes.App.RPC.Sample.Server;

public static class LoadDatatest
{
    public static async Task InitializeLoadDatatest(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServerDatabaseContext>();
            await db.Database.MigrateAsync();

            await db.LoadUsers();
        }
    }

    private static async Task LoadUsers(this ServerDatabaseContext db)
    {
        var users = new List<UserDataModel>();

        for (int i = 1; i <= 1000; i++) {

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

        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();
    }
}
