using Microsoft.EntityFrameworkCore;
using Sample.Domain.Configurations;
using Shared.Concurrency;
using Shared.Timestamps;

namespace App.Infrastructure.Persistence;

// The service's single DbContext. Applies every module's IEntityTypeConfiguration by
// assembly scan, assigns server timestamps, and bumps the concurrency token before save.
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // App.Infrastructure's own configs (cross-module FKs) + one line per module Domain assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoItemConfig).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private void OnBeforeSaving()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<ICreationInfoModel>().Where(e => e.State == EntityState.Added))
            entry.Entity.ServerCreatedDateTime = now;

        foreach (var entry in ChangeTracker.Entries<IUpdateInfoModel>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
            entry.Entity.ServerUpdatedDateTime = now;

        // Example stand-in for PostgreSQL xmin: bump the concurrency token on every write.
        // Real deployments map Version to xmin (ValueGeneratedOnAddOrUpdate) and delete this loop.
        foreach (var entry in ChangeTracker.Entries<IVersioned>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            var prop = entry.Property(nameof(IVersioned.Version));
            prop.CurrentValue = (uint)(prop.CurrentValue ?? 0u) + 1u;
        }
    }
}
