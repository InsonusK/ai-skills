---
name: iguid-resolver
description: defines the IGuidResolver contract for checking Guid existence before a command handler runs
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - guid
  - idempotency
triggers:
  - IGuidResolver interface
  - guid resolver contract
---
# Goal
Define the `IGuidResolver<TRequest, TResponse>` interface. Each module implements this for every command that carries a Guid. The pipeline uses it to check existence and short-circuit with existing entity data before the handler runs.

# Governed by
- guid-resolving.solution.skill.md — full pipeline this contract participates in

# Structure
## Place in csproj
Defined in `shared.csproj.skill.md`
```
/Shared
  /Mediatr
    IGuidResolver.cs
```

## Naming convention
```
interface name: IGuidResolver<TRequest, TResponse>
file name: IGuidResolver.cs
```

# Contracts
```csharp
public interface IGuidResolver<TRequest, TResponse>
    where TRequest : ICommand<TResponse>, IHasGuid
{
    Task<TResponse?> ResolveAsync(TRequest request, CancellationToken ct);
}
```

Returning `null` means Guid not found — handler should run.
Returning non-null means Guid already exists — pipeline short-circuits with that value.

# Rules
MUST:
- Return `null` when entity does not exist
- Return `Result.Conflict(existingResult)` when entity exists
- One implementation per `IHasGuid` command — in `{Module}.Application/Resolvers`
MUST NOT:
- Return `Result.Success` for existing entity — must be `Result.Conflict`
- Modify any state — read-only check only

# Relations
- shared.csproj.skill.md — lives here
- module-application-resolver.class.skill.md — implementation of this interface
- guid-resolving.solution.skill.md — full flow
