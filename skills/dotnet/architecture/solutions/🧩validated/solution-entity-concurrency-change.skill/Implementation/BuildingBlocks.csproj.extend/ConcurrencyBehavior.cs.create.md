---
description: Pipeline behavior validating versions before handler runs
project_name: BuildingBlocks
name: ConcurrencyBehavior.cs
element_kind: class
change_kind: create
---

# Goals
- Validate all entity versions carried by an update command before the handler runs
- Return `Result.Conflict` immediately on any version mismatch — handler never executes for stale updates

# Core Principles
- Constrained on `where TRequest : IHasVersions` — only activates for commands that carry versions
- Gets the correct `IEntityVersionResolver` from `IEntityVersionResolverFactory` per entity name
- Loads the current version through the resolver — returns `Result.NotFound` if the resolver reports `0`
- Compares current version against `expectedVersion` from the command — returns `Result.Conflict` on mismatch
- Checks all entities before deciding — first mismatch short-circuits entire command
- Does not call `SaveChangesAsync` — purely a guard operation

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Concurrency pipeline behavior | `ConcurrencyBehavior<TRequest, TResponse>` | `ConcurrencyBehavior<UpdateTaskCommand, Result>` | `ConcurrencyBehavior.cs` | `ConcurrencyBehavior.cs` |

# Implementation changes

```csharp
// BuildingBlocks/MediatR/ConcurrencyBehavior.cs
using Shared.Concurrency;

namespace BuildingBlocks.MediatR;

public class ConcurrencyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions
    where TResponse : IResult
{
    private readonly IEntityVersionResolverFactory _factory;

    public ConcurrencyBehavior(IEntityVersionResolverFactory factory)
    {
        _factory = factory;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        foreach (var (entityName, idVersions) in request.Versions)
        {
            var resolver = _factory.GetFor(entityName);
            if (resolver is null)
                return (TResponse)Result.Error($"Unknown entity: '{entityName}'.");

            foreach (var (id, expectedVersion) in idVersions)
            {
                var actualVersion = await resolver.GetCurrentVersionForAsync(id, ct);

                if (actualVersion == 0)
                    return (TResponse)Result.NotFound();

                if ((uint)actualVersion != expectedVersion)
                    return (TResponse)Result.Conflict(
                        $"'{entityName}' with Id {id} was modified by another user. " +
                        $"Expected version {expectedVersion}, found {actualVersion}.");
            }
        }

        return await next();
    }
}
```
# Rule changes

## MUST
- Constrained to `where TRequest : IHasVersions` and `where TResponse : IResult`
- Uses `IEntityVersionResolverFactory` from Shared
- Returns `Result.Conflict` on version mismatch — handler never runs
- Never calls `SaveChangesAsync`
- `ETagEncoder` and `ConcurrencyBehavior` live in BuildingBlocks
- `ConcurrencyBehavior` returns `Result.Conflict` on version mismatch — never throws
- `ConcurrencyBehavior` returns `Result.NotFound` if resolver reports `0`
- `ConcurrencyBehavior` returns `Result.Error` for unknown entity name
## MUST NOT
- Activate on commands without `IHasVersions`
- Reference EF Core, repositories, or specifications directly
- Modify any entity state during version check
- Handler check versions manually — `ConcurrencyBehavior` owns this
- `ConcurrencyBehavior` call `SaveChangesAsync`
- `ConcurrencyBehavior` load entities directly — it delegates to `IEntityVersionResolver`
- Entity name keys use C# type names — breaks on entity rename

# Anti-patterns
- Handler catches `DbUpdateConcurrencyException` instead of relying on `ConcurrencyBehavior`
- Loading entities inside the behavior instead of delegating to `IEntityVersionResolver`

# Check list
- [ ] `ConcurrencyBehavior` constrained to `where TRequest : IHasVersions`
- [ ] Uses `IEntityVersionResolverFactory`
- [ ] Returns `Result.Conflict` on version mismatch
- [ ] Returns `Result.NotFound` if resolver reports `0`
- [ ] Returns `Result.Error` for unknown entity name
- [ ] Never calls `SaveChangesAsync`

# Unittest TestCases
- [ ] WHEN applied THEN Validate all entity versions carried by an update command before the handler runs
- [ ] WHEN applied THEN Return Result.Conflict immediately on any version mismatch — handler never executes for stale updates
- [ ] WHEN applied THEN Constrained on where TRequest : IHasVersions — only activates for commands that carry versions
- [ ] WHEN applied THEN Gets the correct IEntityVersionResolver from IEntityVersionResolverFactory per entity name
- [ ] WHEN applied THEN Loads the current version through the resolver — returns Result.NotFound if the resolver reports 0
- [ ] WHEN applied THEN Compares current version against expectedVersion from the command — returns Result.Conflict on mismatch
- [ ] WHEN applied THEN Checks all entities before deciding — first mismatch short-circuits entire command
- [ ] WHEN applied THEN Does not call SaveChangesAsync — purely a guard operation
- [ ] WHEN verified THEN ConcurrencyBehavior constrained to where TRequest : IHasVersions
- [ ] WHEN verified THEN Uses IEntityVersionResolverFactory
- [ ] WHEN verified THEN Returns Result.Conflict on version mismatch
- [ ] WHEN verified THEN Returns Result.NotFound if resolver reports 0
- [ ] WHEN verified THEN Returns Result.Error for unknown entity name
- [ ] WHEN verified THEN Never calls SaveChangesAsync
- [ ] WHEN naming 'Concurrency pipeline behavior' THEN pattern matches convention
