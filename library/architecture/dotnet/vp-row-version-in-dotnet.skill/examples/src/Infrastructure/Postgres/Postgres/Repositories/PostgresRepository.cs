// Source: skill vp-postgres-in-dotnet

using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Persistence;

namespace Postgres.Repositories;

/// <summary>
/// The repository of every entity: Ardalis's <see cref="RepositoryBase{T}"/>
/// over the one context of the command. A write saves before it returns, so
/// inside the unit of work's transaction too. Reads track, so a root loaded,
/// changed, and saved in one command writes what changed, a removed child
/// included. A failure is told in the error kinds by the command middleware
/// (<see cref="PostgresFailures"/>), not here. A root with statements past
/// SaveChanges derives from this class and runs them through <see cref="WriteAsync"/>.
/// </summary>
public class PostgresRepository<TEntity>(DbContext context) : RepositoryBase<TEntity>(context), IRepository<TEntity>, IReadRepository<TEntity>
    where TEntity : class
{
    private const string Savepoint = "repository_write";

    protected DbContext Context { get; } = context;

    /// <summary>
    /// Every write of the base class saves through here. A failed save leaves
    /// nothing tracked: the failed root must not go out again with the next
    /// save, and a later read must see the database.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            Context.ChangeTracker.Clear();
            throw;
        }
    }

    // A count, an existence check, and a delete apply a specification's
    // conditions only, so each refuses one with a page (SpecificationPage).
    public override Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return base.CountAsync(specification, cancellationToken);
    }

    public override Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return base.AnyAsync(specification, cancellationToken);
    }

    public override Task<int> DeleteRangeAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return base.DeleteRangeAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Runs a statement past SaveChanges: ExecuteUpdate, ExecuteDelete, raw SQL,
    /// a bulk copy. The save with nothing to send comes first because the unit
    /// of work variation point opens its transaction on a save, and this class
    /// must not know that variation point; it is this class's save, so a failure
    /// of it clears the tracker too. Inside a transaction the savepoint keeps a
    /// failure of the statement from breaking it, as EF Core does for a save.
    /// </summary>
    protected async Task<int> WriteAsync(Func<Task<int>> statement, CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
        var transaction = Context.Database.CurrentTransaction;
        if (transaction is null)
        {
            return await statement();
        }
        await transaction.CreateSavepointAsync(Savepoint, cancellationToken);
        try
        {
            var written = await statement();
            await transaction.ReleaseSavepointAsync(Savepoint, cancellationToken);
            return written;
        }
        catch
        {
            await ReturnToSavepointAsync(transaction);
            throw;
        }
    }

    /// <summary>
    /// Returns the transaction to the state before the statement, so a duplicate
    /// key, a deadlock, or a serialization failure leaves it usable. A connection
    /// that is gone has no savepoint left: the failure of the statement is the
    /// one to report, so the failure of the rollback is dropped.
    /// </summary>
    private static async Task ReturnToSavepointAsync(IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackToSavepointAsync(Savepoint, CancellationToken.None);
        }
        catch (Exception rollback) when (PostgresFailures.IsUnavailable(rollback) || rollback is InvalidOperationException)
        {
            // the connection is gone, and the server dropped the transaction with it
        }
    }
}
