using Microsoft.EntityFrameworkCore;
using Sample.Domain.Entities;
using Shared.Timestamps;

namespace App.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskItem).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private void OnBeforeSaving()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<ICreationInfoModel>().Where(e => e.State == EntityState.Added))
            entry.Property(nameof(ICreationInfoModelReadOnly.ServerCreatedDateTime)).CurrentValue = now;

        foreach (var entry in ChangeTracker.Entries<IUpdateInfoModel>().Where(e => e.State is EntityState.Added or EntityState.Modified))
            entry.Property(nameof(IUpdateInfoModelReadOnly.ServerUpdatedDateTime)).CurrentValue = now;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(ct);
    }
}
