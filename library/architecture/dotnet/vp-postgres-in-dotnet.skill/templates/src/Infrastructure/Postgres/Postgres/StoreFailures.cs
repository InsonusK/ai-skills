// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Postgres;

/// <summary>
/// What a store asks of an exception of EF Core or Npgsql, so that the codes of
/// PostgreSQL are read in one place. A failed save arrives as a
/// <see cref="DbUpdateException"/> around the driver's error; a query that lost
/// its connection as an <see cref="InvalidOperationException"/> around it.
/// </summary>
public static class StoreFailures
{
    /// <summary>The insert or update broke a unique index.</summary>
    public static bool IsDuplicateKey(Exception error) =>
        error is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } };

    /// <summary>The database did not answer: Npgsql marks every failure a retry may cure as transient.</summary>
    public static bool IsUnavailable(Exception error)
    {
        for (Exception? cause = error; cause is not null; cause = cause.InnerException)
        {
            if (cause is NpgsqlException { IsTransient: true })
            {
                return true;
            }
        }
        return false;
    }
}
