using Ardalis.Specification;
using Shared.Errors;
using Shared.Persistence;

namespace Memory;

/// <summary>
/// The repository's contract over <see cref="MemoryStore"/>: a specification is
/// evaluated over the kept roots by Ardalis's in-memory evaluator. It keeps the
/// root objects themselves, so a root a handler changed is stored as changed;
/// <see cref="UpdateAsync"/> has nothing left to write. A root whose <c>Id</c>
/// is kept already is a conflict, as the database's primary key answers it;
/// every other rule is the root's. Two commands that change one root at once
/// are not kept apart, nor is a change undone when its command fails later:
/// it is the store of an example and of the handlers' scenarios, not of a
/// deployment. The DB variation point replaces it with a repository over the
/// database; no handler changes.
/// </summary>
public class MemoryRepository<TRoot>(MemoryStore store) : IRepository<TRoot>, IReadRepository<TRoot> where TRoot : class, IAggregateRoot
{
    private static readonly InMemorySpecificationEvaluator Evaluator = InMemorySpecificationEvaluator.Default;

    public virtual Task<TRoot> AddAsync(TRoot entity, CancellationToken cancellationToken = default)
    {
        Write(roots => Keep(roots, [entity]));
        return Task.FromResult(entity);
    }

    public virtual Task<IEnumerable<TRoot>> AddRangeAsync(IEnumerable<TRoot> entities, CancellationToken cancellationToken = default)
    {
        var added = entities.ToList();
        Write(roots => Keep(roots, added));
        return Task.FromResult<IEnumerable<TRoot>>(added);
    }

    public virtual Task<int> UpdateAsync(TRoot entity, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => roots.Contains(entity) ? 1 : 0));

    public virtual Task<int> UpdateRangeAsync(IEnumerable<TRoot> entities, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => entities.Count(roots.Contains)));

    public virtual Task<int> DeleteAsync(TRoot entity, CancellationToken cancellationToken = default) =>
        Task.FromResult(Write(roots => roots.Remove(entity) ? 1 : 0));

    public virtual Task<int> DeleteRangeAsync(IEnumerable<TRoot> entities, CancellationToken cancellationToken = default)
    {
        var deleted = entities.ToList();
        return Task.FromResult(Write(roots => deleted.Count(entity => roots.Remove(entity))));
    }

    // A count, an existence check, and a delete refuse a page (SpecificationPage).
    public virtual Task<int> DeleteRangeAsync(ISpecification<TRoot> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return Task.FromResult(Write(roots =>
        {
            var deleted = roots.Cast<TRoot>().Where(specification.IsSatisfiedBy).ToList();
            deleted.ForEach(entity => roots.Remove(entity));
            return deleted.Count;
        }));
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

    public virtual Task<TRoot?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull =>
        Task.FromResult(Read(roots => roots.FirstOrDefault(root => Equals(IdOf(root), id))));

    public virtual Task<TRoot?> FirstOrDefaultAsync(ISpecification<TRoot> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => Evaluator.Evaluate(roots, specification).FirstOrDefault()));

    public virtual Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TRoot, TResult> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => Evaluator.Evaluate(roots, specification).FirstOrDefault()));

    public virtual Task<TRoot?> SingleOrDefaultAsync(ISingleResultSpecification<TRoot> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => Evaluator.Evaluate(roots, specification).SingleOrDefault()));

    public virtual Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<TRoot, TResult> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => Evaluator.Evaluate(roots, specification).SingleOrDefault()));

    public virtual Task<List<TRoot>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => roots.ToList()));

    public virtual Task<List<TRoot>> ListAsync(ISpecification<TRoot> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => Evaluator.Evaluate(roots, specification).ToList()));

    public virtual Task<List<TResult>> ListAsync<TResult>(ISpecification<TRoot, TResult> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => Evaluator.Evaluate(roots, specification).ToList()));

    public virtual Task<int> CountAsync(ISpecification<TRoot> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return Task.FromResult(Read(roots => roots.Count(specification.IsSatisfiedBy)));
    }

    public virtual Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => roots.Count));

    public virtual Task<bool> AnyAsync(ISpecification<TRoot> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return Task.FromResult(Read(roots => roots.Any(specification.IsSatisfiedBy)));
    }

    public virtual Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(roots => roots.Count > 0));

    public virtual async IAsyncEnumerable<TRoot> AsAsyncEnumerable(ISpecification<TRoot> specification)
    {
        foreach (var root in await ListAsync(specification))
        {
            yield return root;
        }
    }

    /// <summary>The key of a root, its <c>Id</c> property; null for a root with none, or with a key the store sets (0).</summary>
    private static object? IdOf(object root) =>
        typeof(TRoot).GetProperty("Id")?.GetValue(root) is { } id && !Equals(id, 0L) && !Equals(id, 0) ? id : null;

    /// <summary>Adds the roots, all or none; a key already kept, or given twice, is a conflict.</summary>
    private static void Keep(List<object> roots, IReadOnlyList<TRoot> added)
    {
        var kept = roots.Select(IdOf).Where(id => id is not null).ToHashSet();
        foreach (var root in added)
        {
            if (IdOf(root) is { } id && !kept.Add(id))
            {
                throw new ErrorKindException(ErrorKind.Conflict, $"{typeof(TRoot).Name} {id} already exists");
            }
        }
        roots.AddRange(added);
    }

    private TResult Read<TResult>(Func<List<TRoot>, TResult> read)
    {
        lock (store.Writing)
        {
            return read(store.Of<TRoot>().Cast<TRoot>().ToList());
        }
    }

    private TResult Write<TResult>(Func<List<object>, TResult> write)
    {
        lock (store.Writing)
        {
            return write(store.Of<TRoot>());
        }
    }

    private void Write(Action<List<object>> write) => Write(roots => { write(roots); return 0; });
}
