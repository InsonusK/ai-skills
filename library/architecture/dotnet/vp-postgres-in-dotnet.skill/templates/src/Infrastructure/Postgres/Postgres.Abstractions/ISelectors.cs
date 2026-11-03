// Source: skill vp-postgres-in-dotnet

using System.Linq.Expressions;

namespace Postgres.Abstractions;

/// <summary>
/// The reads of every selector. Each call asks the database again: a selector is
/// no snapshot, so two calls may disagree, and the number of rows written comes
/// from the write. A selector stays inside the service that made it.
/// </summary>
public interface ISelection<TResult> where TResult : class
{
    /// <summary>A list of every picked row, empty when none is picked.</summary>
    Task<IReadOnlyList<TResult>> AllAsync(CancellationToken cancellationToken);

    /// <summary>The one picked row, or null; more than one throws <see cref="InvalidOperationException"/>, a defect with no outcome.</summary>
    Task<TResult?> SingleAsync(CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    Task<bool> AnyAsync(CancellationToken cancellationToken);
}

/// <summary>The reads that mean something only in an order, so an unordered selector does not have them.</summary>
public interface IOrderedSelection<TResult> : ISelection<TResult> where TResult : class
{
    /// <summary>The first row of the order, or null.</summary>
    Task<TResult?> FirstAsync(CancellationToken cancellationToken);

    /// <summary>A list of up to <paramref name="count"/> rows, every row when fewer exist.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is below 1.</exception>
    Task<IReadOnlyList<TResult>> TakeAsync(int count, CancellationToken cancellationToken);

    /// <summary>A list of the rows of one page, numbered from 1, empty past the end.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> or <paramref name="size"/> is below 1.</exception>
    Task<IReadOnlyList<TResult>> PageAsync(int page, int size, CancellationToken cancellationToken);
}

/// <summary>The rows a specification picks, with what can be done to them.</summary>
public interface IRowSelector<TRow> : ISelection<TRow> where TRow : class
{
    /// <summary>Applies <paramref name="change"/> to every picked row in one save, all or nothing; answers how many. It loads the rows, so a mass update is a method of the table's own.</summary>
    Task<int> UpdateAsync(Action<TRow> change, CancellationToken cancellationToken);

    /// <summary>Deletes every picked row; answers how many.</summary>
    Task<int> DeleteAsync(CancellationToken cancellationToken);

    /// <summary>A new selector in this order; this one stays as it is.</summary>
    IOrderedRowSelector<TRow> OrderBy<TKey>(Expression<Func<TRow, TKey>> key);

    /// <summary>A new selector in this order; this one stays as it is.</summary>
    IOrderedRowSelector<TRow> OrderByDescending<TKey>(Expression<Func<TRow, TKey>> key);
}

public interface IOrderedRowSelector<TRow> : IRowSelector<TRow>, IOrderedSelection<TRow> where TRow : class
{
    /// <summary>A new selector with one more key.</summary>
    IOrderedRowSelector<TRow> ThenBy<TKey>(Expression<Func<TRow, TKey>> key);
}

/// <summary>A projection a specification picks: rows of no table, so nothing to update or delete.</summary>
public interface IResultSelector<TResult> : ISelection<TResult> where TResult : class
{
    /// <summary>A new selector in this order; this one stays as it is.</summary>
    IOrderedResultSelector<TResult> OrderBy<TKey>(Expression<Func<TResult, TKey>> key);

    /// <summary>A new selector in this order; this one stays as it is.</summary>
    IOrderedResultSelector<TResult> OrderByDescending<TKey>(Expression<Func<TResult, TKey>> key);
}

public interface IOrderedResultSelector<TResult> : IResultSelector<TResult>, IOrderedSelection<TResult> where TResult : class
{
    /// <summary>A new selector with one more key.</summary>
    IOrderedResultSelector<TResult> ThenBy<TKey>(Expression<Func<TResult, TKey>> key);
}
