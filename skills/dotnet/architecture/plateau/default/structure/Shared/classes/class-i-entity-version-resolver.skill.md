---
name: class-i-entity-version-resolver
description: Reads the current concurrency version for a single versioned entity
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
---

# Goal
- Provide a typed, entity-specific contract for reading the current database-generated version of a versioned entity
- Let `ConcurrencyBehavior` assert versions without knowing entity types or repository details at compile time

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Single method: `GetCurrentVersionForAsync(int id, CancellationToken) -> Task<int>`
- Returns `0` when the entity does not exist — `ConcurrencyBehavior` returns `Result.NotFound`
- Implementations live in module Application projects and use the module's repositories/specifications

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity version resolver | `IEntityVersionResolver` | `IEntityVersionResolver` | `IEntityVersionResolver.cs` | `IEntityVersionResolver.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-entity-version-resolver
//Plateau: default
//Version: 20260628
```

```csharp
// Shared/Concurrency/IEntityVersionResolver.cs
namespace Shared.Concurrency;

public interface IEntityVersionResolver
{
    Task<int> GetCurrentVersionForAsync(int id, CancellationToken cancellationToken = default);
}
```

> **Note:** `0` is reserved as a not-found sentinel because PostgreSQL `xmin` is never `0` for a persisted row.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Rules
MUST:
- Return the current `Version` cast to `int`
- Return `0` when the entity is not found
- Be implemented in module Application projects

MUST NOT:
- Throw when the entity is missing — return `0` so `ConcurrencyBehavior` can produce `Result.NotFound`
- Reference EF Core or DbContext directly

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Returning a negative number for missing entities — complicates the contract
- Implementing the resolver in App.Infrastructure or BuildingBlocks — Application owns per-entity data access

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Check list
- [ ] `IEntityVersionResolver` defined in `Shared/Concurrency/IEntityVersionResolver.cs`
- [ ] Method signature returns `Task<int>`
- [ ] `0` reserved for not-found

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Provide a typed, entity-specific contract for reading the current database-generated version
- [ ] WHEN applied THEN Let `ConcurrencyBehavior` assert versions without knowing entity types or repository details at compile time
- [ ] WHEN applied THEN Single method: `GetCurrentVersionForAsync(int id, CancellationToken) -> Task<int>`
- [ ] WHEN applied THEN Returns `0` when the entity does not exist — `ConcurrencyBehavior` returns `Result.NotFound`
- [ ] WHEN applied THEN Implementations live in module Application projects and use the module's repositories/specifications
- [ ] WHEN verified THEN `IEntityVersionResolver` defined in `Shared/Concurrency/IEntityVersionResolver.cs`
- [ ] WHEN verified THEN Method signature returns `Task<int>`
- [ ] WHEN naming 'Entity version resolver' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]
