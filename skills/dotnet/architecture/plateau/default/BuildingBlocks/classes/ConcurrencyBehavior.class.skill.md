---
uid: 272bbcee-7417-4d62-8244-2e902f68d334
name: concurrencybehavior-class
description: Pipeline behavior validating versions before handler runs
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
---

# Goal
- Validate all entity versions carried by an update command before the handler runs
- Return `Result.Conflict` immediately on any version mismatch — handler never executes for stale updates

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]

# Core Principals
- Constrained on `where TRequest : IHasVersions` — only activates for commands that carry versions
- Resolves `IReadRepository<TEntity>` from DI dynamically using `IServiceProvider` — entity type known only at runtime
- Loads each entity by Id using a `ByIdSpec` — returns `Result.NotFound` if entity missing during version check
- Compares loaded `entity.Version` against `expectedVersion` from command — returns `Result.Conflict` on mismatch
- Checks all entities before deciding — first mismatch short-circuits entire command
- Does not call `SaveChangesAsync` — purely a read and guard operation
- Entities expose `Version` through `IVersioned` from Shared — no reflection needed

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Concurrency pipeline behavior | `ConcurrencyBehavior<TRequest, TResponse>` | `ConcurrencyBehavior<UpdateTaskCommand, Result>` | `ConcurrencyBehavior.cs` | `ConcurrencyBehavior.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]

# Implementation
```csharp
// BuildingBlocks/MediatR/ConcurrencyBehavior.cs
using BuildingBlocks.Specifications;
using Shared.Concurrency;
using Shared.Repositories;

namespace BuildingBlocks.MediatR;

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
                var actualVersion = ((IVersioned)entity).Version;

                if (actualVersion != expectedVersion)
                    return (TResponse)Result.Conflict(
                        $"'{entityName}' with Id {id} was modified by another user. " +
                        $"Expected version {expectedVersion}, found {actualVersion}.");
            }
        }

        return await next();
    }

    private static async Task<IVersioned?> LoadEntityAsync(
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

        return ((dynamic)task).Result as IVersioned;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]

# Rules
MUST:
	- Constrained to `where TRequest : IHasVersions` and `where TResponse : IResult`
	- Returns `Result.Conflict` on version mismatch — handler never runs
	- Returns `Result.NotFound` if entity missing during version check
	- Returns `Result.Error` for unknown entity name
	- Never calls `SaveChangesAsync`
	- Casts loaded entities to `IVersioned` from Shared — no reflection on `Version`
MUST NOT:
	- Activate on commands without `IHasVersions` — only update/patch commands carry versions
	- Modify any entity state during version check

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]

# Anti-patterns
- Handler catches `DbUpdateConcurrencyException` instead of relying on `ConcurrencyBehavior`
- Reading `Version` via reflection instead of `IVersioned`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]

# Check list
- [ ] `ConcurrencyBehavior` constrained to `where TRequest : IHasVersions`
- [ ] Returns `Result.Conflict` on version mismatch
- [ ] Returns `Result.NotFound` if entity missing during version check
- [ ] Returns `Result.Error` for unknown entity name
- [ ] Never calls `SaveChangesAsync`
- [ ] Uses `IVersioned` from Shared to read `Version`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Validate all entity versions carried by an update command before the handler runs
- [ ] WHEN applied THEN Return Result.Conflict immediately on any version mismatch — handler never executes for stale updates
- [ ] WHEN applied THEN Constrained on where TRequest : IHasVersions — only activates for commands that carry versions
- [ ] WHEN applied THEN Resolves IReadRepository<TEntity> from DI dynamically using IServiceProvider — entity type known only at runtime
- [ ] WHEN applied THEN Loads each entity by Id using a ByIdSpec — returns Result.NotFound if entity missing during version check
- [ ] WHEN applied THEN Compares loaded entity.Version against expectedVersion from command — returns Result.Conflict on mismatch
- [ ] WHEN applied THEN Checks all entities before deciding — first mismatch short-circuits entire command
- [ ] WHEN applied THEN Does not call SaveChangesAsync — purely a read and guard operation
- [ ] WHEN applied THEN Casts loaded entities to IVersioned from Shared — no reflection on Version
- [ ] WHEN verified THEN ConcurrencyBehavior constrained to where TRequest : IHasVersions
- [ ] WHEN verified THEN Returns Result.Conflict on version mismatch
- [ ] WHEN verified THEN Returns Result.NotFound if entity missing during version check
- [ ] WHEN verified THEN Returns Result.Error for unknown entity name
- [ ] WHEN verified THEN Never calls SaveChangesAsync
- [ ] WHEN verified THEN Uses IVersioned from Shared to read Version
- [ ] WHEN naming 'Concurrency pipeline behavior' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]]
