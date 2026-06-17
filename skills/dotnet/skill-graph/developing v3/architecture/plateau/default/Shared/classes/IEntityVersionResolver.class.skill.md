---
uid: 076b10b7-62f4-49f6-8847-1b5f958e5b9b
name: ientityversionresolver-class
description: Maps stable string entity names to C# types
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
---

# Goal
- Decouple `ConcurrencyBehavior` from concrete entity types by mapping string names to C# types at runtime
- Allow `ConcurrencyBehavior` to resolve `IReadRepository<TEntity>` from DI without knowing entity types at compile time

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Core Principals
- Single method: `Resolve(string entityName) → Type?`
- Returns `null` for unknown entity names — `ConcurrencyBehavior` returns `Result.Error` on null
- Implementation in App.Infrastructure — Shared owns only the interface

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity name to type resolver | `IEntityVersionResolver` | `IEntityVersionResolver` | `IEntityVersionResolver.cs` | `IEntityVersionResolver.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Implementation
```csharp
// Shared/Concurrency/IEntityVersionResolver.cs
namespace Shared.Concurrency;

public interface IEntityVersionResolver
{
    Type? Resolve(string entityName);
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Rules
MUST:
	- Return `null` for unknown entity names
	- Shared owns only the interface
MUST NOT:
	- Contain implementation or DI-registered services

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Anti-patterns
- Interface returns `Type` without nullable annotation — forces callers to suppress warnings

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Check list
- [ ] `IEntityVersionResolver` defined in `Shared/Concurrency/IEntityVersionResolver.cs`
- [ ] Method signature returns `Type?`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Decouple ConcurrencyBehavior from concrete entity types by mapping string names to C# types at runtime
- [ ] WHEN applied THEN Allow ConcurrencyBehavior to resolve IReadRepository<TEntity> from DI without knowing entity types at compile time
- [ ] WHEN applied THEN Single method: Resolve(string entityName) → Type?
- [ ] WHEN applied THEN Returns null for unknown entity names — ConcurrencyBehavior returns Result.Error on null
- [ ] WHEN applied THEN Implementation in App.Infrastructure — Shared owns only the interface
- [ ] WHEN verified THEN IEntityVersionResolver defined in Shared/Concurrency/IEntityVersionResolver.cs
- [ ] WHEN verified THEN Method signature returns Type?
- [ ] WHEN naming 'Entity name to type resolver' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolver.cs.create.md|IEntityVersionResolver.cs.create]]
