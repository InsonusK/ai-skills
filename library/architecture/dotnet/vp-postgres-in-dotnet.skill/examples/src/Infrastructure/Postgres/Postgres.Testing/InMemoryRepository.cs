// Source: skill vp-postgres-in-dotnet

using System.Linq.Expressions;
using Ardalis.Specification;
using Postgres.Abstractions;

namespace Postgres.Testing;

/// <summary>
/// The repository a service's scenarios use instead of a database: the same
/// specifications picked from a list of rows, or the failure the scenario set.
/// It proves what the service does with the contract; what the database does
/// is proved by the scenarios on the real engine.
/// </summary>
public sealed class InMemoryRepository<TRow> : IRepository<TRow> where TRow : class
{
    private readonly List<TRow> _rows = [];

    /// <summary>What every call throws, when a scenario sets it.</summary>
    public Exception? Failure { get; set; }

    public Task InsertAsync(TRow row, CancellationToken cancellationToken)
    {
        Check();
        _rows.Add(row);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TRow row, CancellationToken cancellationToken)
    {
        Check();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TRow row, CancellationToken cancellationToken)
    {
        Check();
        _rows.Remove(row);
        return Task.CompletedTask;
    }

    public IRowSelector<TRow> SelectBy(ISpecification<TRow> specification) => new RowSelector(this, specification, null);

    public IResultSelector<TResult> SelectBy<TResult>(ISpecification<TRow, TResult> specification) where TResult : class =>
        new ResultSelector<TResult>(this, specification, null);

    private void Check()
    {
        if (Failure is not null)
        {
            throw Failure;
        }
    }

    private abstract class Selection<TResult>(InMemoryRepository<TRow> repository, Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? order)
        : IOrderedSelection<TResult> where TResult : class
    {
        protected InMemoryRepository<TRow> Repository { get; } = repository;
        protected Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? Order { get; } = order;
        protected abstract IEnumerable<TResult> Picked { get; }

        private List<TResult> Read()
        {
            Repository.Check();
            var rows = Picked.AsQueryable();
            return (Order is null ? rows : Order(rows)).ToList();
        }

        public Task<IReadOnlyList<TResult>> AllAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<TResult>>(Read());
        public Task<TResult?> SingleAsync(CancellationToken cancellationToken) => Task.FromResult(Read().SingleOrDefault());
        public Task<int> CountAsync(CancellationToken cancellationToken) => Task.FromResult(Read().Count);
        public Task<bool> AnyAsync(CancellationToken cancellationToken) => Task.FromResult(Read().Count > 0);
        public Task<TResult?> FirstAsync(CancellationToken cancellationToken) => Task.FromResult(Read().FirstOrDefault());
        public Task<IReadOnlyList<TResult>> TakeAsync(int count, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<TResult>>(Read().Take(count).ToList());
        public Task<IReadOnlyList<TResult>> PageAsync(int page, int size, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TResult>>(Read().Skip((page - 1) * size).Take(size).ToList());
    }

    private sealed class RowSelector(InMemoryRepository<TRow> repository, ISpecification<TRow> specification, Func<IQueryable<TRow>, IOrderedQueryable<TRow>>? order)
        : Selection<TRow>(repository, order), IOrderedRowSelector<TRow>
    {
        protected override IEnumerable<TRow> Picked => specification.Evaluate(Repository._rows);

        public async Task<int> UpdateAsync(Action<TRow> change, CancellationToken cancellationToken)
        {
            var rows = await AllAsync(cancellationToken);
            rows.ToList().ForEach(change);
            return rows.Count;
        }

        public async Task<int> DeleteAsync(CancellationToken cancellationToken)
        {
            var rows = await AllAsync(cancellationToken);
            rows.ToList().ForEach(row => Repository._rows.Remove(row));
            return rows.Count;
        }

        public IOrderedRowSelector<TRow> OrderBy<TKey>(Expression<Func<TRow, TKey>> key) => new RowSelector(Repository, specification, rows => rows.OrderBy(key));
        public IOrderedRowSelector<TRow> OrderByDescending<TKey>(Expression<Func<TRow, TKey>> key) => new RowSelector(Repository, specification, rows => rows.OrderByDescending(key));
        public IOrderedRowSelector<TRow> ThenBy<TKey>(Expression<Func<TRow, TKey>> key) => new RowSelector(Repository, specification, rows => Order!(rows).ThenBy(key));
    }

    private sealed class ResultSelector<TResult>(InMemoryRepository<TRow> repository, ISpecification<TRow, TResult> specification, Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? order)
        : Selection<TResult>(repository, order), IOrderedResultSelector<TResult> where TResult : class
    {
        protected override IEnumerable<TResult> Picked => specification.Evaluate(Repository._rows);

        public IOrderedResultSelector<TResult> OrderBy<TKey>(Expression<Func<TResult, TKey>> key) => new ResultSelector<TResult>(Repository, specification, rows => rows.OrderBy(key));
        public IOrderedResultSelector<TResult> OrderByDescending<TKey>(Expression<Func<TResult, TKey>> key) => new ResultSelector<TResult>(Repository, specification, rows => rows.OrderByDescending(key));
        public IOrderedResultSelector<TResult> ThenBy<TKey>(Expression<Func<TResult, TKey>> key) => new ResultSelector<TResult>(Repository, specification, rows => Order!(rows).ThenBy(key));
    }
}
