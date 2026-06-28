---
name: class-i-guid-resolver
description: Per-entity resolver contract
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
---

# Goal
- Define the per-entity contract for checking whether a Guid already exists and returning the existing command response
- Keep `GuidResolvingBehavior` generic — each entity type provides its own resolver implementation

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs.create]]

# Core Principals
- Generic on `TResponse` — matches the command's response type exactly
- Returns `TResponse?` — null means Guid not found (first request), non-null means already exists (retry)
- The returned non-null value must be the same response type the handler would return, but marked as a conflict
- One implementation per external-created entity type — registered in module DI registration
- Uses `IReadRepository<T>` and a `{Entity}ByGuidSpec` — never hits the DB directly
- Lives in Shared so `{Module}.Application` resolvers and BuildingBlocks behavior can both consume it

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Guid resolver interface | `IGuidResolver<TResponse>` | `IGuidResolver<Result<CreateTaskResult>>` | `IGuidResolver.cs` | `IGuidResolver.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs.create]]

# Implementation
```csharp
// Shared/Guid/IGuidResolver.cs
namespace Shared.Guid;

public interface IGuidResolver<TResponse>
{
    Task<TResponse?> ResolveAsync(Guid guid, CancellationToken ct);
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs.create]]

# Rules
MUST:
	- `TResponse` matches the command's return type exactly — same type as the command's `ICommand<TResponse>`
	- Return null when Guid not found — never throws
	- Return the existing command response when Guid found — same type as handler success response
MUST NOT:
	- Throw exceptions — null is the only "not found" signal
	- Return a different response type than the command handler — breaks API contract symmetry
	- Be defined in BuildingBlocks — it is a contract consumed by multiple layers

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs.create]]

# Anti-patterns
- `IGuidResolver` without generic parameter — would require casting and lose type safety
- `IGuidResolver` defined in BuildingBlocks — forces module Application to reference BuildingBlocks for a contract
- Resolver returning a response type different from the command handler

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs.create]]

# Check list
- [ ] `IGuidResolver<TResponse>` defined in `Shared/Guid/IGuidResolver.cs`
- [ ] Returns `Task<TResponse?>`
- [ ] `TResponse` matches command handler response type

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs.create]]

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IGuidResolver.cs.create.md|IGuidResolver.cs.create]]
