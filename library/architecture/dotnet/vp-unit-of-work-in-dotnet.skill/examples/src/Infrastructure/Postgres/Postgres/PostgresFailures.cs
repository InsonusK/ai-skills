// Source: skill vp-postgres-in-dotnet

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Errors;
using Shared.Persistence;

namespace Postgres;

/// <summary>
/// What a failure of EF Core or Npgsql means for the caller, so the codes of
/// PostgreSQL are read in one place. The driver's error arrives alone from a
/// statement past SaveChanges, inside a <see cref="DbUpdateException"/> from a
/// save, and inside an <see cref="InvalidOperationException"/> from a query that
/// lost its connection, so every question walks the chain of causes. The
/// message names the entity of the failed save, never a table, a column, or a
/// value: the failure itself stays the cause, which only the log writes.
/// </summary>
public sealed partial class PostgresFailures : IStoreFailures
{
    public ErrorKindException? KindOf(Exception failure)
    {
        if (ErrorKindException.KindOf(failure) is not null)
        {
            return null;
        }
        var subject = Subject(failure);
        if (Causes(failure).Any(cause => cause is DbUpdateConcurrencyException))
        {
            return new ErrorKindException(ErrorKind.ConcurrentChange, $"{subject} was changed meanwhile", failure);
        }
        if (Has(failure, PostgresErrorCodes.UniqueViolation))
        {
            return new ErrorKindException(ErrorKind.Conflict, $"{subject} already exists", failure);
        }
        if (Has(failure, PostgresErrorCodes.ForeignKeyViolation))
        {
            return new ErrorKindException(ErrorKind.NotFound, $"{subject} refers to something that is not there", failure);
        }
        return IsUnavailable(failure)
            ? new ErrorKindException(ErrorKind.Unavailable, "the database is unavailable", failure)
            : null;
    }

    /// <summary>A retry may cure it: Npgsql marks as transient a lost connection, a deadlock, and a serialization failure.</summary>
    public static bool IsUnavailable(Exception failure) => Causes(failure).Any(cause => cause is NpgsqlException { IsTransient: true });

    /// <summary>The entity of the first entry of a failed save, in words ("item list"), or "a row" for any other failure.</summary>
    private static string Subject(Exception failure)
    {
        var entity = Causes(failure).OfType<DbUpdateException>().SelectMany(update => update.Entries).FirstOrDefault()?.Metadata.ClrType.Name;
        return entity is null ? "a row" : Words().Replace(entity, " $1").Trim().ToLowerInvariant();
    }

    private static bool Has(Exception failure, string sqlState) =>
        Causes(failure).Any(cause => cause is PostgresException error && error.SqlState == sqlState);

    private static IEnumerable<Exception> Causes(Exception failure)
    {
        for (Exception? cause = failure; cause is not null; cause = cause.InnerException)
        {
            yield return cause;
        }
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex Words();
}
