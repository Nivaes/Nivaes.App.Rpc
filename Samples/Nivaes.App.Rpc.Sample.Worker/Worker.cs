using Microsoft.EntityFrameworkCore;
using Nivaes.App.Rpc.Sample.Client;
using Nivaes.DataTestGenerator;

namespace Nivaes.App.Rpc.Sample.Worker
{
    internal class Worker(IDbContextFactory<DatabaseContext> factory, ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int i = 0;
            while(!stoppingToken.IsCancellationRequested)
            {
                await using (var db = await factory.CreateDbContextAsync())
                {
                    var contact = ContactGenerator.GenerateContact();

                    var user = new UserDataModel
                    {
                        IdUser = Guid.NewGuid(),
                        Identification = $"ID{++i:00000}",
                        Name = contact.SortName,
                        GivenName = contact.GivenName,
                        FamilyName = contact.FamilyName,
                        Email = contact.Email,
                        PhoneNumber = contact.TelephoneNumber
                    };

                    await db.Users.AddAsync(user);
                    await db.SaveChangesAsync();

                    logger.LogInformation($"Save {user.Id}: {user.Name}");
                }
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
