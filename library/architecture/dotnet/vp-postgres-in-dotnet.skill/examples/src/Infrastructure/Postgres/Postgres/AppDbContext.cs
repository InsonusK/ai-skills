using Microsoft.EntityFrameworkCore;
using Todo.Domain.Configurations;

namespace Postgres;

/// <summary>
/// The one context of the application, over every module's tables, so one
/// migration history describes the database. It has no property per table:
/// each module owns its tables, an <c>IEntityTypeConfiguration</c> per entity in
/// its <c>{Module}.Domain/Configurations</c>, and this context applies every
/// configuration of each module's assembly, one line per module; the repository
/// reaches the entities with <c>Set&lt;TEntity&gt;()</c>.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // A table of no module (an outbox, a cache of another system's data) is configured in this project.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // [modules] - one line per module that keeps data.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ItemConfiguration).Assembly);
    }
}
