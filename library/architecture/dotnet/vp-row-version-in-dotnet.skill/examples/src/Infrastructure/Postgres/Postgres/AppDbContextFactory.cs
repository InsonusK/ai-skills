// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Postgres;

/// <summary>
/// How dotnet ef builds the context to generate a migration: from the model
/// alone, so make migration needs no database. Nothing calls it at run time.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql().Options);
}
