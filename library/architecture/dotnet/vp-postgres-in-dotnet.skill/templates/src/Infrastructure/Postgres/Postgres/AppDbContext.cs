// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;

namespace Postgres;

/// <summary>
/// The one context of the application, over every module's tables, so one
/// migration history describes the database. It has no property per table:
/// every <c>IEntityTypeConfiguration</c> of this project maps its aggregate, and
/// the repository reaches them with <c>Set&lt;TRoot&gt;()</c>, so a new aggregate edits
/// no fixed file.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
