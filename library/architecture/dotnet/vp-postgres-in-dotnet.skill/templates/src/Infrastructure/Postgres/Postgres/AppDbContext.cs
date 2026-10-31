// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;

namespace Postgres;

/// <summary>
/// The one context of the application, over every module's tables, so one
/// migration history describes the database. It has no property per table:
/// every <c>IEntityTypeConfiguration</c> of this project maps its row, and a
/// store reaches its rows with <c>Set&lt;TRow&gt;()</c>, so a new table edits
/// no fixed file.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
