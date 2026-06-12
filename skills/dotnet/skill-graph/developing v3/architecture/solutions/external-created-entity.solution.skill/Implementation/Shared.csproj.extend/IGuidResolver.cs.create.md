---
description: Per-entity resolver contract
project_name: Shared
name: IGuidResolver.cs
element_kind: class
change_kind: create
---

# Goals
- Define the per-entity contract for checking whether a Guid already exists and returning the existing result
- Keep `GuidResolvingBehavior` generic — each entity type provides its own resolver implementation

# Core Principles
- Generic on `TResult` — matches the command's result type exactly
- Returns `TResult?` — null means Guid not found (first request), non-null means already exists (retry)
- One implementation per external-created entity type — registered in module DI registration
- Uses `IReadRepository<T>` and a `{Entity}ByGuidSpec` — never hits the DB directly
- Lives in Shared so `{Module}.Application` resolvers and BuildingBlocks behavior can both consume it

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolver interface | `IGuidResolver<TResult>` | `IGuidResolver<Result<CreateTaskResult>>` | `IGuidResolver.cs` | `IGuidResolver.cs` |

# Implementation changes

```csharp
// Shared/Guid/IGuidResolver.cs
public interface IGuidResolver<TResult>
{
    Task<TResult?> ResolveAsync(Guid guid, CancellationToken ct);
}
```

# Rules

MUST:
- `TResult` matches the command's return type exactly — same type as the command's `ICommand<TResult>`
- Returns null when Guid not found — never throws
- Returns existing result when Guid found — `GuidResolvingBehavior` throws on non-null

MUST NOT:
- Throw exceptions — null is the only "not found" signal
- Be defined in BuildingBlocks — it is a contract consumed by multiple layers

# Anti-patterns
- `IGuidResolver` without generic parameter — would require casting and lose type safety
- `IGuidResolver` defined in BuildingBlocks — forces module Application to reference BuildingBlocks for a contract

# Check list
- [ ] `IGuidResolver<TResult>` defined in `Shared/Guid/IGuidResolver.cs`
- [ ] Returns `Task<TResult?>`

# Unittest TestCases
- [ ] WHEN applied THEN Define the per-entity contract for checking whether a Guid already exists and returning the existing result
- [ ] WHEN applied THEN Keep GuidResolvingBehavior generic — each entity type provides its own resolver implementation
- [ ] WHEN applied THEN Generic on TResult — matches the command's result type exactly
- [ ] WHEN applied THEN Returns TResult? — null means Guid not found (first request), non-null means already exists (retry)
- [ ] WHEN applied THEN One implementation per external-created entity type — registered in module DI registration
- [ ] WHEN applied THEN Uses IReadRepository<T> and a {Entity}ByGuidSpec — never hits the DB directly
- [ ] WHEN applied THEN Lives in Shared so {Module}.Application resolvers and BuildingBlocks behavior can both consume it
- [ ] WHEN verified THEN IGuidResolver<TResult> defined in Shared/Guid/IGuidResolver.cs
- [ ] WHEN verified THEN Returns Task<TResult?>
- [ ] WHEN naming 'Guid resolver interface' THEN pattern matches convention
