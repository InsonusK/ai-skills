// Source: skill vp-postgres-in-dotnet

using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Persistence;

namespace Postgres.Repositories;

/// <summary>
/// The repository of every aggregate root: Ardalis's <see cref="RepositoryBase{T}"/>
/// over the one context of the command. A write saves before it returns, so
/// inside the unit of work's transaction too. Reads track, so a root loaded,
/// changed, and saved in one command writes what changed, a removed child
/// included. A failure is told in the error kinds by the command middleware
/// (<see cref="PostgresFailures"/>), not here. A root with statements past
/// SaveChanges derives from this class and runs them through <see cref="WriteAsync"/>.
/// </summary>
public class PostgresRepository<TRoot>(DbContext context) : RepositoryBase<TRoot>(context), IRepository<TRoot>, IReadRepository<TRoot>
    where TRoot : class, IAggregateRoot
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
    public override Task<int> CountAsync(ISpecification<TRoot> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return base.CountAsync(specification, cancellationToken);
    }

    public override Task<bool> AnyAsync(ISpecification<TRoot> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return base.AnyAsync(specification, cancellationToken);
    }

    public override Task<int> DeleteRangeAsync(ISpecification<TRoot> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return base.DeleteRangeAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Runs a statement past SaveChanges: ExecuteUpdate, ExecuteDelete, raw SQL,
    /// a bulk copy. The save with nothing to send comes first because the unit
    /// of work variation point opens its transaction on a save, and this class
    /// must not know that variation point. Inside a transaction the savepoint
    /// keeps a failure from breaking it, as EF Core does for a save.
    /// </summary>
    protected async Task<int> WriteAsync(Func<Task<int>> statement, CancellationToken cancellationToken)
    {
        await Context.SaveChangesAsync(cancellationToken);
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
        catch (Exception failure) when (!PostgresFailures.IsUnavailable(failure))
        {
            // A connection that failed has no savepoint left to return to.
            await transaction.RollbackToSavepointAsync(Savepoint, cancellationToken);
            throw;
        }
    }
}
