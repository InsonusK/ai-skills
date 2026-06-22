---
uid: 4e1c8d5f-6a7b-4c8d-9e0f-1a2b3c4d5e6f
name: ientityversionresolverfactory-class
description: Factory contract that maps stable business entity names to IEntityVersionResolver implementations
domain: skill
type: template
version: 20260622
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
---

# Goal
- Decouple `ConcurrencyBehavior` from concrete entity types by resolving the correct `IEntityVersionResolver` at runtime from a string entity name
- Keep Shared as the home of cross-cutting concurrency contracts

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Core Principals
- Single method: `GetFor(string entityName) -> IEntityVersionResolver?`
- Returns `null` for unknown entity names — `ConcurrencyBehavior` returns `Result.Error` on null
- Implementation lives in App.Infrastructure — Shared owns only the interface

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity version resolver factory | `IEntityVersionResolverFactory` | `IEntityVersionResolverFactory` | `IEntityVersionResolverFactory.cs` | `IEntityVersionResolverFactory.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Implementation
```csharp
// Shared/Concurrency/IEntityVersionResolverFactory.cs
namespace Shared.Concurrency;

public interface IEntityVersionResolverFactory
{
    IEntityVersionResolver? GetFor(string entityName);
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Rules
MUST:
- Return `null` for unknown entity names
- Shared owns only the interface

MUST NOT:
- Contain implementation or DI-registered services

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Anti-patterns
- Factory returns non-nullable `IEntityVersionResolver` — forces callers to suppress warnings or throw for unknown names

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

# Check list
- [ ] `IEntityVersionResolverFactory` defined in `Shared/Concurrency/IEntityVersionResolverFactory.cs`
- [ ] Method signature returns `IEntityVersionResolver?`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]

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
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IEntityVersionResolverFactory.cs.create.md|IEntityVersionResolverFactory.cs.create]]
