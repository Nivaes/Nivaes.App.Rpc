using Microsoft.EntityFrameworkCore;
using Nivaes.App.Rpc.Client;

namespace Nivaes.App.Rpc.Sample.Client;

public class DatabaseContext : DbContext
{
    #region Constructors
    //public DatabaseContext()
    // : base()
    //{
    //}

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
    #endregion

    #region DbSet
    public DbSet<UserDataModel> Users { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserDataModel>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(e => e.IdUser);

            entity.Property(up => up.IdUser)
              .IsRequired()
              .HasColumnName("IdUser");

            entity.Property(e => e.Identification)
                .IsRequired()
                .HasColumnName("Identification");

            entity.Ignore(i => i.ProfileAvatar);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("Name");

            entity.HasIndex(e => e.Name);

            entity.Property(e => e.GivenName)
                .IsRequired()
                .HasColumnName("GivenName");

            entity.HasIndex(e => e.GivenName);

            entity.Property(e => e.FamilyName)
                .IsRequired()
                .HasColumnName("FamilyName");

            entity.HasIndex(e => e.FamilyName);

            entity.Property(e => e.Email)
                .HasColumnName("Email");

            entity.Property(e => e.PhoneNumber)
                .HasColumnName("PhoneNumber");

            entity.HasIndex(up => up.TimeStampTicks)
                .IsUnique(false);

            entity.Property(up => up.TimeStampTicks)
                .IsRequired()
                .HasColumnName("TimeStampTicks");
        });
    }

    //public static string databasePath = "client.db";

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//    {
//        SQLitePCL.Batteries_V2.Init();

//        optionsBuilder.UseSqlite($"Data Source={databasePath}");
//            //.RpcIntegration();
//            //.UseModel(Client.Database.DatabaseContextModel.Instance);

//        base.OnConfiguring(optionsBuilder);
//    }
}