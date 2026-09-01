---
description: Factory that returns the version resolver for a given stable entity name
project_name: Shared
name: IEntityVersionResolverFactory.cs
element_kind: class
change_kind: create
tags:
  - solution/entity-concurrency-change
  - element/ientityversionresolverfactory-cs
---

# Goals
- Decouple `ConcurrencyBehavior` from concrete entity types by resolving the correct `IEntityVersionResolver` at runtime from a string entity name
- Keep Shared as the home of cross-cutting concurrency contracts

# Core Principles
- Single method: `GetFor(string entityName) -> IEntityVersionResolver?`
- Returns `null` for unknown entity names — `ConcurrencyBehavior` returns `Result.Error` on null
- Implementation lives in App.Infrastructure — Shared owns only the interface

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity version resolver factory | `IEntityVersionResolverFactory` | `IEntityVersionResolverFactory` | `IEntityVersionResolverFactory.cs` | `IEntityVersionResolverFactory.cs` |

# Implementation changes

```csharp
// Shared/Concurrency/IEntityVersionResolverFactory.cs
namespace Shared.Concurrency;

public interface IEntityVersionResolverFactory
{
    IEntityVersionResolver? GetFor(string entityName);
}
```

# Rule changes

## MUST
- Return `null` for unknown entity names
- Shared owns only the interface
- Never contain implementation or DI-registered services

## SHOULD
- Avoid factory returns non-nullable `IEntityVersionResolver` — forces callers to suppress warnings or throw for unknown names

# Check list
- [ ] `IEntityVersionResolverFactory` defined in `Shared/Concurrency/IEntityVersionResolverFactory.cs`
- [ ] Method signature returns `IEntityVersionResolver?`

# Unittest TestCases
- [ ] WHEN applied THEN Decouple ConcurrencyBehavior from concrete entity types by resolving the correct IEntityVersionResolver at runtime from a string entity name
- [ ] WHEN applied THEN Keep Shared as the home of cross-cutting concurrency contracts
- [ ] WHEN applied THEN Single method: GetFor(string entityName) -> IEntityVersionResolver?
- [ ] WHEN applied THEN Returns null for unknown entity names — ConcurrencyBehavior returns Result.Error on null
- [ ] WHEN applied THEN Implementation lives in App.Infrastructure — Shared owns only the interface
- [ ] WHEN verified THEN IEntityVersionResolverFactory defined in Shared/Concurrency/IEntityVersionResolverFactory.cs
- [ ] WHEN verified THEN Method signature returns IEntityVersionResolver?
- [ ] WHEN naming 'Entity version resolver factory' THEN pattern matches convention
