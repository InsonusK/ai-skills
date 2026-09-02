---
description: Per-entity resolver contract
project_name: Shared
name: IGuidResolver.cs
element_kind: class
change_kind: create
tags:
  - solution/external-created-entity
  - element/iguidresolver-cs
---

# Goals
- Define the per-entity contract for checking whether a Guid already exists and returning the existing command response
- Keep `GuidResolvingBehavior` generic — each entity type provides its own resolver implementation

# Core Principles
- Generic on `TResponse` — matches the command's response type exactly
- Returns `TResponse?` — null means Guid not found (first request), non-null means already exists (retry)
- The returned non-null value must be the same response type the handler would return, but marked as a conflict
- Uses `IReadRepository<T>` and a `{Entity}ByGuidSpec` — never hits the DB directly
- Lives in Shared so `{Module}.Application` resolvers and BuildingBlocks behavior can both consume it

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolver interface | `IGuidResolver<TResponse>` | `IGuidResolver<Result<CreateTaskResult>>` | `IGuidResolver.cs` | `IGuidResolver.cs` |

# Implementation changes

```csharp
// Shared/Guid/IGuidResolver.cs
namespace Shared.Guid;

public interface IGuidResolver<TResponse>
{
    Task<TResponse?> ResolveAsync(Guid guid, CancellationToken ct);
}
```
# Rule changes

## MUST
- `TResponse` matches the command's return type exactly — same type as the command's `ICommand<TResponse>`
- Return null when Guid not found — never throws
- Return the existing command response when Guid found — same type as handler success response
- `IHasGuid`, `IGuidResolver<TResponse>` defined in Shared
- Each `IGuidResolver<TResponse>` registered as `Scoped` in module DI registration
- `IGuidResolver<TResponse>` returns `Task<TResponse?>` — null means not found, non-null means conflict
- Never throw exceptions — null is the only "not found" signal
- Never return a different response type than the command handler — breaks API contract symmetry
- Never be defined in BuildingBlocks — it is a contract consumed by multiple layers
- Never register `IGuidResolver` as an open generic — each entity type registers its own concrete resolver

## SHOULD
- Avoid `IGuidResolver` without generic parameter — would require casting and lose type safety
- Avoid `IGuidResolver` defined in BuildingBlocks — forces module Application to reference BuildingBlocks for a contract
- Avoid resolver returning a response type different from the command handler

# Check list
- [ ] `IGuidResolver<TResponse>` defined in `Shared/Guid/IGuidResolver.cs`
- [ ] Returns `Task<TResponse?>`
- [ ] `TResponse` matches command handler response type

# Unittest TestCases
- [ ] WHEN applied THEN Define the per-entity contract for checking whether a Guid already exists and returning the existing command response
- [ ] WHEN applied THEN Keep GuidResolvingBehavior generic — each entity type provides its own resolver implementation
- [ ] WHEN applied THEN Generic on TResponse — matches the command's response type exactly
- [ ] WHEN applied THEN Returns TResponse? — null means Guid not found, non-null means already exists
- [ ] WHEN applied THEN One implementation per external-created entity type — registered in module DI registration
- [ ] WHEN applied THEN Uses IReadRepository<T> and a {Entity}ByGuidSpec — never hits the DB directly
- [ ] WHEN applied THEN Lives in Shared so {Module}.Application resolvers and BuildingBlocks behavior can both consume it
- [ ] WHEN verified THEN IGuidResolver<TResponse> defined in Shared/Guid/IGuidResolver.cs
- [ ] WHEN verified THEN Returns Task<TResponse?>
- [ ] WHEN verified THEN TResponse matches command handler response type
- [ ] WHEN naming 'Guid resolver interface' THEN pattern matches convention
