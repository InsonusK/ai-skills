// Source: skill vp-postgres-in-dotnet

using Ardalis.Specification;
using Shared.Errors;
using Shared.Persistence;

namespace Postgres.Testing;

/// <summary>
/// The repository's contract in memory, for the scenarios of the handlers, which
/// run with no database: a specification is evaluated over the kept entities by
/// Ardalis's in-memory evaluator. It keeps the
/// entity objects themselves, so an entity a handler changed is stored as changed;
/// <see cref="UpdateAsync"/> has nothing left to write. An entity whose <c>Id</c>
/// is kept already is a conflict, as the database's primary key answers it;
/// every other rule is the entity's. Two commands that change one entity at once
/// are not kept apart, nor is a change undone when its command fails later:
/// it serves one scenario. The database's behaviour is proved by Postgres.Tests.
/// </summary>
public class InMemoryRepository<TEntity> : IRepository<TEntity>, IReadRepository<TEntity> where TEntity : class
{
    private static readonly InMemorySpecificationEvaluator Evaluator = InMemorySpecificationEvaluator.Default;

    private readonly List<object> _entities = [];
    private readonly Lock _writing = new();

    public virtual Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Write(entities => Keep(entities, [entity]));
        return Task.FromResult(entity);
    }

    public virtual Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var added = entities.ToList();
        Write(entities => Keep(entities, added));
        return Task.FromResult<IEnumerable<TEntity>>(added);
    }

    public virtual Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => entities.Contains(entity) ? 1 : 0));

    public virtual Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => entities.Count(entities.Contains)));

    public virtual Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Task.FromResult(Write(entities => entities.Remove(entity) ? 1 : 0));

    public virtual Task<int> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var deleted = entities.ToList();
        return Task.FromResult(Write(entities => deleted.Count(entity => entities.Remove(entity))));
    }

    // A count, an existence check, and a delete refuse a page (SpecificationPage).
    public virtual Task<int> DeleteRangeAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return Task.FromResult(Write(entities =>
        {
            var deleted = entities.Cast<TEntity>().Where(specification.IsSatisfiedBy).ToList();
            deleted.ForEach(entity => entities.Remove(entity));
            return deleted.Count;
        }));
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

    public virtual Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull =>
        Task.FromResult(Read(entities => entities.FirstOrDefault(root => Equals(IdOf(root), id))));

    public virtual Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => Evaluator.Evaluate(entities, specification).FirstOrDefault()));

    public virtual Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => Evaluator.Evaluate(entities, specification).FirstOrDefault()));

    public virtual Task<TEntity?> SingleOrDefaultAsync(ISingleResultSpecification<TEntity> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => Evaluator.Evaluate(entities, specification).SingleOrDefault()));

    public virtual Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => Evaluator.Evaluate(entities, specification).SingleOrDefault()));

    public virtual Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => entities.ToList()));

    public virtual Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => Evaluator.Evaluate(entities, specification).ToList()));

    public virtual Task<List<TResult>> ListAsync<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => Evaluator.Evaluate(entities, specification).ToList()));

    public virtual Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return Task.FromResult(Read(entities => entities.Count(specification.IsSatisfiedBy)));
    }

    public virtual Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => entities.Count));

    public virtual Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        SpecificationPage.Refuse(specification);
        return Task.FromResult(Read(entities => entities.Any(specification.IsSatisfiedBy)));
    }

    public virtual Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Read(entities => entities.Count > 0));

    public virtual async IAsyncEnumerable<TEntity> AsAsyncEnumerable(ISpecification<TEntity> specification)
    {
        foreach (var entity in await ListAsync(specification))
        {
            yield return entity;
        }
    }

    /// <summary>The key of an entity, its <c>Id</c> property; null for an entity with none, or with a key the store sets (0).</summary>
    private static object? IdOf(object entity) =>
        typeof(TEntity).GetProperty("Id")?.GetValue(entity) is { } id && !Equals(id, 0L) && !Equals(id, 0) ? id : null;

    /// <summary>Adds the entities, all or none; a key already kept, or given twice, is a conflict.</summary>
    private static void Keep(List<object> entities, IReadOnlyList<TEntity> added)
    {
        var kept = entities.Select(IdOf).Where(id => id is not null).ToHashSet();
        foreach (var entity in added)
        {
            if (IdOf(entity) is { } id && !kept.Add(id))
            {
                throw new ErrorKindException(ErrorKind.Conflict, $"{typeof(TEntity).Name} {id} already exists");
            }
        }
        entities.AddRange(added);
    }

    private TResult Read<TResult>(Func<List<TEntity>, TResult> read)
    {
        lock (_writing)
        {
            return read(_entities.Cast<TEntity>().ToList());
        }
    }

    private TResult Write<TResult>(Func<List<object>, TResult> write)
    {
        lock (_writing)
        {
            return write(_entities);
        }
    }

    private void Write(Action<List<object>> write) => Write(entities => { write(entities); return 0; });
}
