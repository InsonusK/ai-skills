// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Postgres.Abstractions;

namespace Postgres;

/// <summary>
/// What a repository asks of an exception of EF Core or Npgsql, so that the codes
/// of PostgreSQL are read in one place. The driver's error arrives alone from a
/// statement past SaveChanges, inside a <see cref="DbUpdateException"/> from a
/// save, and inside an <see cref="InvalidOperationException"/> from a query that
/// lost its connection, so every question walks the chain of causes.
/// </summary>
public static class StoreFailures
{
    /// <summary>The outcome <paramref name="error"/> means for the caller, or null for a failure with no outcome: a defect that stays as it is.</summary>
    public static Exception? Outcome(Exception error, string what)
    {
        if (error is DbUpdateConcurrencyException)
        {
            return new RowChangedException($"{what}: the row is gone or was changed meanwhile", error);
        }
        if (Has(error, PostgresErrorCodes.UniqueViolation))
        {
            return new DuplicateKeyException($"{what}: duplicate key", error);
        }
        if (Has(error, PostgresErrorCodes.ForeignKeyViolation))
        {
            return new MissingReferenceException($"{what}: the row it refers to is not there", error);
        }
        return IsUnavailable(error)
            ? new StoreUnavailableException($"{what}: the database does not answer", error)
            : null;
    }

    /// <summary>A retry may cure it: Npgsql marks as transient a lost connection, a deadlock, and a serialization failure.</summary>
    public static bool IsUnavailable(Exception error) => Causes(error).Any(cause => cause is NpgsqlException { IsTransient: true });

    private static bool Has(Exception error, string sqlState) =>
        Causes(error).Any(cause => cause is PostgresException failure && failure.SqlState == sqlState);

    private static IEnumerable<Exception> Causes(Exception error)
    {
        for (Exception? cause = error; cause is not null; cause = cause.InnerException)
        {
            yield return cause;
        }
    }
}
