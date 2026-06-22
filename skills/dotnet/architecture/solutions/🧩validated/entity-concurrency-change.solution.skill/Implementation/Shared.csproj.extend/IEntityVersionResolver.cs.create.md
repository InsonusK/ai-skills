---
description: Maps stable string entity names to C# types
project_name: Shared
name: IEntityVersionResolver.cs
element_kind: class
change_kind: create
---

# Goals
- Decouple `ConcurrencyBehavior` from concrete entity types by mapping string names to C# types at runtime
- Allow `ConcurrencyBehavior` to resolve `IReadRepository<TEntity>` from DI without knowing entity types at compile time

# Core Principles
- Single method: `Resolve(string entityName) → Type?`
- Returns `null` for unknown entity names — `ConcurrencyBehavior` returns `Result.Error` on null
- Implementation in App.Infrastructure — Shared owns only the interface

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity name to type resolver | `IEntityVersionResolver` | `IEntityVersionResolver` | `IEntityVersionResolver.cs` | `IEntityVersionResolver.cs` |

# Implementation changes

```csharp
// Shared/Concurrency/IEntityVersionResolver.cs
namespace Shared.Concurrency;

public interface IEntityVersionResolver
{
    Type? Resolve(string entityName);
}
```

# Rules

MUST:
- Return `null` for unknown entity names
- Shared owns only the interface

MUST NOT:
- Contain implementation or DI-registered services

# Anti-patterns
- Interface returns `Type` without nullable annotation — forces callers to suppress warnings

# Check list
- [ ] `IEntityVersionResolver` defined in `Shared/Concurrency/IEntityVersionResolver.cs`
- [ ] Method signature returns `Type?`

# Unittest TestCases
- [ ] WHEN applied THEN Decouple ConcurrencyBehavior from concrete entity types by mapping string names to C# types at runtime
- [ ] WHEN applied THEN Allow ConcurrencyBehavior to resolve IReadRepository<TEntity> from DI without knowing entity types at compile time
- [ ] WHEN applied THEN Single method: Resolve(string entityName) → Type?
- [ ] WHEN applied THEN Returns null for unknown entity names — ConcurrencyBehavior returns Result.Error on null
- [ ] WHEN applied THEN Implementation in App.Infrastructure — Shared owns only the interface
- [ ] WHEN verified THEN IEntityVersionResolver defined in Shared/Concurrency/IEntityVersionResolver.cs
- [ ] WHEN verified THEN Method signature returns Type?
- [ ] WHEN naming 'Entity name to type resolver' THEN pattern matches convention
