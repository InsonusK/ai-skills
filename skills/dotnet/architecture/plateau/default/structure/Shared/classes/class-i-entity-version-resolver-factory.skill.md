---
name: class-i-entity-version-resolver-factory
description: Factory contract that maps stable business entity names to IEntityVersionResolver implementations
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
- Decouple `ConcurrencyBehavior` from concrete entity types by resolving the correct `IEntityVersionResolver` at runtime from a string entity name
- Keep Shared as the home of cross-cutting concurrency contracts

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Core Principals
- Apply ONE plateau template per class
- Single method: `GetFor(string entityName) -> IEntityVersionResolver?`
- Returns `null` for unknown entity names — `ConcurrencyBehavior` returns `Result.Error` on null
- Implementation lives in App.Infrastructure — Shared owns only the interface

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity version resolver factory | `IEntityVersionResolverFactory` | `IEntityVersionResolverFactory` | `IEntityVersionResolverFactory.cs` | `IEntityVersionResolverFactory.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-entity-version-resolver-factory
//Plateau: default
//Version: 20260628
```

```csharp
// Shared/Concurrency/IEntityVersionResolverFactory.cs
namespace Shared.Concurrency;

public interface IEntityVersionResolverFactory
{
    IEntityVersionResolver? GetFor(string entityName);
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Rules
MUST:
- Return `null` for unknown entity names
- Shared owns only the interface

MUST NOT:
- Contain implementation or DI-registered services

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Factory returns non-nullable `IEntityVersionResolver` — forces callers to suppress warnings or throw for unknown names

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Check list
- [ ] `IEntityVersionResolverFactory` defined in `Shared/Concurrency/IEntityVersionResolverFactory.cs`
- [ ] Method signature returns `IEntityVersionResolver?`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Decouple `ConcurrencyBehavior` from concrete entity types by resolving the correct `IEntityVersionResolver` at runtime from a string entity name
- [ ] WHEN applied THEN Keep Shared as the home of cross-cutting concurrency contracts
- [ ] WHEN applied THEN Single method: `GetFor(string entityName) -> IEntityVersionResolver?`
- [ ] WHEN applied THEN Returns `null` for unknown entity names — `ConcurrencyBehavior` returns `Result.Error` on null
- [ ] WHEN applied THEN Implementation lives in App.Infrastructure — Shared owns only the interface
- [ ] WHEN verified THEN `IEntityVersionResolverFactory` defined in `Shared/Concurrency/IEntityVersionResolverFactory.cs`
- [ ] WHEN verified THEN Method signature returns `IEntityVersionResolver?`
- [ ] WHEN naming 'Entity version resolver factory' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]
