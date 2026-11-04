// Source: skill vp-postgres-in-dotnet

using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Postgres.Abstractions;

namespace Postgres.Repositories;

/// <summary>
/// The contract of every table. A repository saves at once: a command that must
/// keep several writes together gets a transaction around them from the unit of
/// work variation point, with no change here. A table with statements of its
/// own derives from this class and passes them through <see cref="PostgresStatements.ReadAsync"/>
/// or <see cref="PostgresStatements.WriteAsync"/>.
/// </summary>
public class PostgresRepository<TRow>(DbContext context) : PostgresStatements(context), IRepository<TRow> where TRow : class
{
    protected static string Table { get; } = typeof(TRow).Name;

    public Task InsertAsync(TRow row, CancellationToken cancellationToken)
    {
        Context.Set<TRow>().Add(row);
        return SaveAsync($"insert into {Table}", cancellationToken);
    }

    public Task UpdateAsync(TRow row, CancellationToken cancellationToken)
    {
        Context.Set<TRow>().Update(row);
        return SaveAsync($"update of {Table}", cancellationToken);
    }

    public Task DeleteAsync(TRow row, CancellationToken cancellationToken)
    {
        Context.Set<TRow>().Remove(row);
        return SaveAsync($"delete from {Table}", cancellationToken);
    }

    public IRowSelector<TRow> SelectBy(ISpecification<TRow> specification)
    {
        RefuseOrderAndPage(specification);
        return new RowSelector<TRow>(this, Context, specification, null);
    }

    public IResultSelector<TResult> SelectBy<TResult>(ISpecification<TRow, TResult> specification) where TResult : class
    {
        RefuseOrderAndPage(specification);
        return new ResultSelector<TRow, TResult>(this, Context, specification, null);
    }

    /// <summary>A page in a specification would hold for a read and be lost on a delete, so the rows of the two would differ.</summary>
    private static void RefuseOrderAndPage(ISpecification<TRow> specification)
    {
        if (specification.Skip > 0 || specification.Take >= 0 || specification.OrderExpressions.Any())
        {
            throw new ArgumentException("a specification picks rows; the order and the page are asked of the selector", nameof(specification));
        }
    }
}
