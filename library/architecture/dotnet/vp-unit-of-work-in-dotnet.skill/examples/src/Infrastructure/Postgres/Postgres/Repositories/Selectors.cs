// Source: skill vp-postgres-in-dotnet

using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Postgres.Abstractions;

namespace Postgres.Repositories;

/// <summary>
/// The reads of a selector. Every call builds its query again, so a selector
/// holds no rows; an order is kept as a function, so ordering makes a new
/// selector and leaves this one as it is.
/// </summary>
internal abstract class Selection<TResult>(
    PostgresStatements statements,
    Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? order) : IOrderedSelection<TResult> where TResult : class
{
    protected static string Result { get; } = typeof(TResult).Name;

    protected PostgresStatements Statements { get; } = statements;

    protected Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? Order { get; } = order;

    /// <summary>The picked rows, in the order of no one.</summary>
    protected abstract IQueryable<TResult> Picked { get; }

    private IQueryable<TResult> InOrder => (Order is null ? Picked : Order(Picked)).AsNoTracking();

    public async Task<IReadOnlyList<TResult>> AllAsync(CancellationToken cancellationToken) =>
        await Statements.ReadAsync(() => InOrder.ToListAsync(cancellationToken), Result);

    public Task<TResult?> SingleAsync(CancellationToken cancellationToken) =>
        Statements.ReadAsync(() => Picked.AsNoTracking().SingleOrDefaultAsync(cancellationToken), Result);

    public Task<int> CountAsync(CancellationToken cancellationToken) =>
        Statements.ReadAsync(() => Picked.CountAsync(cancellationToken), Result);

    public Task<bool> AnyAsync(CancellationToken cancellationToken) =>
        Statements.ReadAsync(() => Picked.AnyAsync(cancellationToken), Result);

    public Task<TResult?> FirstAsync(CancellationToken cancellationToken) =>
        Statements.ReadAsync(() => InOrder.FirstOrDefaultAsync(cancellationToken), Result);

    public async Task<IReadOnlyList<TResult>> TakeAsync(int count, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        return await Statements.ReadAsync(() => InOrder.Take(count).ToListAsync(cancellationToken), Result);
    }

    public async Task<IReadOnlyList<TResult>> PageAsync(int page, int size, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
        return await Statements.ReadAsync(() => InOrder.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken), Result);
    }
}

/// <summary>The rows of a table: the reads, and what can be done to the rows.</summary>
internal sealed class RowSelector<TRow>(
    PostgresStatements statements,
    DbContext context,
    ISpecification<TRow> specification,
    Func<IQueryable<TRow>, IOrderedQueryable<TRow>>? order) : Selection<TRow>(statements, order), IOrderedRowSelector<TRow> where TRow : class
{
    protected override IQueryable<TRow> Picked => SpecificationEvaluator.Default.GetQuery(context.Set<TRow>(), specification);

    public async Task<int> UpdateAsync(Action<TRow> change, CancellationToken cancellationToken)
    {
        var rows = await Statements.ReadAsync(() => Picked.AsTracking().ToListAsync(cancellationToken), Result);
        rows.ForEach(change);
        await Statements.SaveAsync($"update of {Result}", cancellationToken);
        return rows.Count;
    }

    // Conditions only: a specification's Include cannot be part of a delete.
    public Task<int> DeleteAsync(CancellationToken cancellationToken) =>
        Statements.WriteAsync(
            () => SpecificationEvaluator.Default.GetQuery(context.Set<TRow>(), specification, evaluateCriteriaOnly: true).ExecuteDeleteAsync(cancellationToken),
            $"delete from {Result}",
            cancellationToken);

    public IOrderedRowSelector<TRow> OrderBy<TKey>(Expression<Func<TRow, TKey>> key) =>
        new RowSelector<TRow>(Statements, context, specification, rows => rows.OrderBy(key));

    public IOrderedRowSelector<TRow> OrderByDescending<TKey>(Expression<Func<TRow, TKey>> key) =>
        new RowSelector<TRow>(Statements, context, specification, rows => rows.OrderByDescending(key));

    public IOrderedRowSelector<TRow> ThenBy<TKey>(Expression<Func<TRow, TKey>> key) =>
        new RowSelector<TRow>(Statements, context, specification, rows => Order!(rows).ThenBy(key));
}

/// <summary>A projection: the reads only.</summary>
internal sealed class ResultSelector<TRow, TResult>(
    PostgresStatements statements,
    DbContext context,
    ISpecification<TRow, TResult> specification,
    Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? order) : Selection<TResult>(statements, order), IOrderedResultSelector<TResult>
    where TRow : class
    where TResult : class
{
    protected override IQueryable<TResult> Picked => SpecificationEvaluator.Default.GetQuery(context.Set<TRow>(), specification);

    public IOrderedResultSelector<TResult> OrderBy<TKey>(Expression<Func<TResult, TKey>> key) =>
        new ResultSelector<TRow, TResult>(Statements, context, specification, rows => rows.OrderBy(key));

    public IOrderedResultSelector<TResult> OrderByDescending<TKey>(Expression<Func<TResult, TKey>> key) =>
        new ResultSelector<TRow, TResult>(Statements, context, specification, rows => rows.OrderByDescending(key));

    public IOrderedResultSelector<TResult> ThenBy<TKey>(Expression<Func<TResult, TKey>> key) =>
        new ResultSelector<TRow, TResult>(Statements, context, specification, rows => Order!(rows).ThenBy(key));
}
