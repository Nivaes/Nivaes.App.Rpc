using Microsoft.EntityFrameworkCore;
using Nivaes.App.Rpc;
using Nivaes.App.Rpc.Client;

namespace Nivaes.App.RPC.Client;

internal class RpcSyncDatabaseContext : DbContext
{
    #region Constructors
    public RpcSyncDatabaseContext()
     : base()
    {
    }

    public RpcSyncDatabaseContext(DbContextOptions<RpcSyncDatabaseContext> options)
        : base(options)
    {
    }
    #endregion

    #region DbSet
    public DbSet<SyncSetting> SyncSetting { get; set; }

    public DbSet<SyncData> SyncDatas { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SyncSetting>(entity =>
        {
            entity.ToTable("SyncSetting");

            entity.HasKey(e => e.Key);

            entity.Property(up => up.Key)
              .IsRequired()
              .HasColumnName("Key");

            entity.Property(up => up.Value)
              .IsRequired()
              .HasColumnName("Value");
        });

        modelBuilder.Entity<SyncData>(entity =>
        {
            entity.ToTable("SyncData");

            entity.HasKey(e => e.Id);

            entity.Property(up => up.EntityType)
              .IsRequired()
              .HasColumnName("EntityType");

            entity.Property(e => e.Data)
                .IsRequired()
                .HasColumnName("Data");

            entity.HasIndex(up => up.TimeStampTicks)
                .IsUnique(false);

            entity.Property(up => up.TimeStampTicks)
                .IsRequired()
                .HasColumnName("TimeStampTicks");
        });
    }
}