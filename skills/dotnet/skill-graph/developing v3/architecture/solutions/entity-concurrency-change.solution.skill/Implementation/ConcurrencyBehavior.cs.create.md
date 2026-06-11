---
description: Pipeline behavior validating versions before handler runs
name: ConcurrencyBehavior.cs
change_kind: create
---

# Goals
- Validate all entity versions carried by an update command before the handler runs
- Return `Result.Conflict` immediately on any version mismatch — handler never executes for stale updates
- Run after `ValidationBehavior` and before `UnitOfWorkBehavior` — stale commands never open a unit of work

# Core Principles
- Constrained on `where TRequest : IHasVersions` — only activates for commands that carry versions
- Resolves `IReadRepository<TEntity>` from DI dynamically using `IServiceProvider` — entity type known only at runtime
- Loads each entity by Id using a `ByIdSpec` — returns `Result.NotFound` if entity missing during version check
- Compares loaded `entity.Version` against `expectedVersion` from command — returns `Result.Conflict` on mismatch
- Checks all entities before deciding — first mismatch short-circuits entire command
- Does not call `SaveChangesAsync` — purely a read and guard operation

# Pipeline position
```
ValidationBehavior      ← validation-behavior.solution.skill — rejects invalid input
    ↓
ConcurrencyBehavior     ← this solution — rejects stale versions
    ↓
UnitOfWorkBehavior      ← unit-of-work.solution.skill — commits on success
    ↓
Handler
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Concurrency pipeline behavior | `ConcurrencyBehavior<TRequest, TResponse>` | `ConcurrencyBehavior<UpdateTaskCommand, Result>` | `ConcurrencyBehavior.cs` | `ConcurrencyBehavior.cs` |

# Implementation changes

```csharp
// BuildingBlocks/MediatR/ConcurrencyBehavior.cs
public class ConcurrencyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions
    where TResponse : IResult
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityVersionResolver _resolver;

    public ConcurrencyBehavior(
        IServiceProvider serviceProvider,
        IEntityVersionResolver resolver)
    {
        _serviceProvider = serviceProvider;
        _resolver = resolver;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        foreach (var (entityName, idVersions) in request.Versions)
        {
            var entityType = _resolver.Resolve(entityName);
            if (entityType is null)
                return (TResponse)Result.Error($"Unknown entity type: '{entityName}'.");

            // resolve IReadRepository<TEntity> from DI at runtime
            var repoType = typeof(IReadRepository<>).MakeGenericType(entityType);
            var repo = _serviceProvider.GetRequiredService(repoType);

            foreach (var (id, expectedVersion) in idVersions)
            {
                // load entity — uses ByIdSpec resolved dynamically
                var entity = await LoadEntityAsync(repo, entityType, id, ct);

                if (entity is null)
                    return (TResponse)Result.NotFound();

                // compare versions — mismatch means client has stale data
                var actualVersion = (uint)entityType
                    .GetProperty(nameof(IVersioned.Version))!
                    .GetValue(entity)!;

                if (actualVersion != expectedVersion)
                    return (TResponse)Result.Conflict(
                        $"'{entityName}' with Id {id} was modified by another user. " +
                        $"Expected version {expectedVersion}, found {actualVersion}.");
            }
        }

        return await next();
    }

    private static async Task<object?> LoadEntityAsync(
        object repo, Type entityType, int id, CancellationToken ct)
    {
        // invoke FirstOrDefaultAsync via reflection — entity type known only at runtime
        var method = repo.GetType()
            .GetMethod(nameof(IReadRepository<object>.FirstOrDefaultAsync),
                new[] { typeof(ISpecification<>).MakeGenericType(entityType),
                        typeof(CancellationToken) })!;

        var specType = typeof(EntityByIdSpec<>).MakeGenericType(entityType);
        var spec = Activator.CreateInstance(specType, id)!;

        var task = (Task)method.Invoke(repo, new[] { spec, ct })!;
        await task.ConfigureAwait(false);

        return ((dynamic)task).Result;
    }
}
```

> **Note on `IVersioned`:** Entities accessed by `ConcurrencyBehavior` must expose `Version` via a shared interface or the behavior uses reflection. A clean alternative is to define `IVersioned` in Shared:
> ```csharp
> // Shared/Concurrency/IVersioned.cs
> public interface IVersioned
> {
>     uint Version { get; }
> }
> ```
> All mutable entities implement `IVersioned`. `ConcurrencyBehavior` casts loaded entities to `IVersioned` instead of using reflection. This is the recommended approach.

# Rules

MUST:
- Constrained to `where TRequest : IHasVersions` and `where TResponse : IResult`
- Returns `Result.Conflict` on version mismatch — handler never runs
- Returns `Result.NotFound` if entity missing during version check
- Returns `Result.Error` for unknown entity name
- Never calls `SaveChangesAsync`

MUST NOT:
- Activate on commands without `IHasVersions` — only update/patch commands carry versions
- Modify any entity state during version check

# Anti-patterns
- `ConcurrencyBehavior` registered after `UnitOfWorkBehavior` — stale commands open a unit of work unnecessarily
- Handler catches `DbUpdateConcurrencyException` instead of relying on `ConcurrencyBehavior`

# Check list
- [ ] `ConcurrencyBehavior` constrained to `where TRequest : IHasVersions`
- [ ] Returns `Result.Conflict` on version mismatch
- [ ] Returns `Result.NotFound` if entity missing during version check
- [ ] Returns `Result.Error` for unknown entity name
- [ ] Never calls `SaveChangesAsync`
