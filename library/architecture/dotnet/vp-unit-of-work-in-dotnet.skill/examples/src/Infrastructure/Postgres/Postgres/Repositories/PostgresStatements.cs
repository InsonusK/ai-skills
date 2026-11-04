// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Postgres.Repositories;

/// <summary>
/// The one way a statement reaches the database, so that every failure is
/// translated once and the context holds nothing between two calls. It knows no
/// table and no operation: the operations are the repository's.
/// </summary>
public abstract class PostgresStatements(DbContext context)
{
    private const string Savepoint = "repository_write";

    protected DbContext Context { get; } = context;

    /// <summary>
    /// Saves what the context tracks. It empties the tracker whatever happens: a
    /// failed row must not go out again with the next save, and a later read must
    /// see the database, not a row remembered here.
    /// </summary>
    protected internal async Task SaveAsync(string what, CancellationToken cancellationToken)
    {
        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception error) when (StoreFailures.Outcome(error, what) is { } outcome)
        {
            throw outcome;
        }
        finally
        {
            Context.ChangeTracker.Clear();
        }
    }

    /// <summary>Runs a query.</summary>
    protected internal async Task<TResult> ReadAsync<TResult>(Func<Task<TResult>> query, string what)
    {
        try
        {
            return await query();
        }
        catch (Exception error) when (StoreFailures.Outcome(error, what) is { } outcome)
        {
            throw outcome;
        }
    }

    /// <summary>
    /// Runs a statement past SaveChanges: ExecuteUpdate, ExecuteDelete, raw SQL, a
    /// bulk copy. The save with nothing to send comes first because the unit of
    /// work variation point opens its transaction on a save, and this class must
    /// not know that variation point. Inside a transaction the savepoint keeps a
    /// reported failure from breaking it, as EF Core does for a save.
    /// </summary>
    protected internal async Task<int> WriteAsync(Func<Task<int>> statement, string what, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            await Context.SaveChangesAsync(cancellationToken);
            transaction = Context.Database.CurrentTransaction;
            if (transaction is not null)
            {
                await transaction.CreateSavepointAsync(Savepoint, cancellationToken);
            }
            var written = await statement();
            if (transaction is not null)
            {
                await transaction.ReleaseSavepointAsync(Savepoint, cancellationToken);
            }
            return written;
        }
        catch (Exception error) when (StoreFailures.Outcome(error, what) is { } outcome)
        {
            // A connection that failed has no savepoint left to return to.
            if (transaction is not null && !StoreFailures.IsUnavailable(error))
            {
                await transaction.RollbackToSavepointAsync(Savepoint, cancellationToken);
            }
            throw outcome;
        }
    }
}
