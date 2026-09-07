---
description: Pipeline behavior validating versions before handler runs
project_name: BuildingBlocks
name: ConcurrencyBehavior.cs
element_kind: class
change_kind: create
tags:
  - solution/entity-concurrency-change
  - element/concurrencybehavior-cs
---

# Goals
- Validate all entity versions carried by an update command before the handler runs
- Return `Result.Conflict` immediately on any version mismatch — handler never executes for stale updates

# Core Principles
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
using Ardalis.Result;
using MediatR;
using Shared.Concurrency;

namespace BuildingBlocks.MediatR;

public sealed class ConcurrencyBehavior<TRequest, TResponse>(IEntityVersionResolverFactory factory)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions
    where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        foreach (var (entityName, idVersions) in request.Versions)
        {
            var resolver = factory.GetFor(entityName);
            if (resolver is null)
                return Error($"Unknown entity: '{entityName}'.");

            foreach (var (id, expectedVersion) in idVersions)
            {
                var actualVersion = await resolver.GetCurrentVersionForAsync(id, ct);

                if (actualVersion == 0)
                    return NoArg(nameof(Result.NotFound));

                if ((uint)actualVersion != expectedVersion)
                    return Params(nameof(Result.Conflict),
                        $"'{entityName}' with Id {id} was modified by another user. " +
                        $"Expected version {expectedVersion}, found {actualVersion}.");
            }
        }

        return await next();
    }

    // TResponse is Result or Result<T>; invoke its own static factory. A plain
    // (TResponse)Result.X(...) cast throws at runtime for Result<T> (a generic-parameter
    // cast never runs Ardalis.Result's implicit Result -> Result<T> conversion).
    private static TResponse Error(string message)
        => (TResponse)typeof(TResponse).GetMethod(nameof(Result.Error), [typeof(string)])!.Invoke(null, [message])!;

    private static TResponse NoArg(string method)
        => (TResponse)typeof(TResponse).GetMethod(method, Type.EmptyTypes)!.Invoke(null, [])!;

    private static TResponse Params(string method, string message)
        => (TResponse)typeof(TResponse).GetMethod(method, [typeof(string[])])!.Invoke(null, [new[] { message }])!;
}
```
# Rule changes

## MUST
- Constrained to `where TRequest : IHasVersions` and `where TResponse : IResult`
- Uses `IEntityVersionResolverFactory` from Shared
- Build the short-circuit result via the closed `TResponse`'s own static `Error`/`NotFound`/`Conflict` (reflection), never `(TResponse)Result.X(...)` — a generic-parameter cast throws for `Result<T>`
- Returns `Result.Conflict` on version mismatch — handler never runs
- Never calls `SaveChangesAsync`
- `ETagEncoder` and `ConcurrencyBehavior` live in BuildingBlocks
- `ConcurrencyBehavior` returns `Result.NotFound` if resolver reports `0`
- `ConcurrencyBehavior` returns `Result.Error` for unknown entity name
- Never activate on commands without `IHasVersions`
- Never reference EF Core, repositories, or specifications directly
- Never modify any entity state during version check
- Never check versions manually in a handler — `ConcurrencyBehavior` owns this
- Never call `SaveChangesAsync` from `ConcurrencyBehavior`
- Never load entities directly in `ConcurrencyBehavior` — it delegates to `IEntityVersionResolver`
- Never use a C# type name as an entity-name key — it breaks on rename
## SHOULD
- Avoid catching `DbUpdateConcurrencyException` in a handler instead of relying on `ConcurrencyBehavior`
- Avoid loading entities inside the behavior instead of delegating to `IEntityVersionResolver`

# Check list
- [ ] `ConcurrencyBehavior` constrained to `where TRequest : IHasVersions`
- [ ] Uses `IEntityVersionResolverFactory`
- [ ] Returns `Result.Conflict` on version mismatch
- [ ] Returns `Result.NotFound` if resolver reports `0`
- [ ] Returns `Result.Error` for unknown entity name

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
