// Source: skill vp-postgres-in-dotnet

using Ardalis.Specification;

namespace Postgres.Abstractions;

/// <summary>
/// The rows of one table, the one contract of every table: a table needs its row
/// and its mapping, and gets this without a line of its own. A write saves before
/// it returns, so the caller sees the database's outcome. A table adds methods
/// only for what this cannot express, on an interface of its own that extends
/// this one. Every method reports the outcomes of StoreExceptions.cs.
/// </summary>
public interface IRepository<TRow> where TRow : class
{
    /// <summary>Saves a new row; a generated key is set on it.</summary>
    Task InsertAsync(TRow row, CancellationToken cancellationToken);

    /// <summary>Saves every column of the row; a row no longer there throws <see cref="RowChangedException"/>.</summary>
    Task UpdateAsync(TRow row, CancellationToken cancellationToken);

    /// <summary>Removes the row; a row no longer there throws <see cref="RowChangedException"/>.</summary>
    Task DeleteAsync(TRow row, CancellationToken cancellationToken);

    /// <summary>The rows the specification picks; no statement runs until the selector is asked.</summary>
    /// <exception cref="ArgumentException">The specification holds an order or a page, which belong to the selector.</exception>
    IRowSelector<TRow> SelectBy(ISpecification<TRow> specification);

    /// <summary>The projection the specification picks: the same reads, with nothing to update or delete.</summary>
    /// <exception cref="ArgumentException">The specification holds an order or a page, which belong to the selector.</exception>
    IResultSelector<TResult> SelectBy<TResult>(ISpecification<TRow, TResult> specification) where TResult : class;
}
