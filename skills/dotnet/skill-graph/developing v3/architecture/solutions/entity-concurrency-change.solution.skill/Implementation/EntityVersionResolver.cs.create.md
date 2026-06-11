---
description: Maps stable business entity names to C# types
name: EntityVersionResolver.cs
change_kind: create
---

# Goals
- Provide the concrete mapping from stable entity name strings to C# entity types
- Be the single place to update when new mutable entities are added to the solution

# Core Principles
- Static readonly dictionary — populated at startup, no runtime modification
- Keys are stable business names agreed with the frontend — changing a key is a breaking API change
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity name resolver | `EntityVersionResolver` | `EntityVersionResolver` | `EntityVersionResolver.cs` | `EntityVersionResolver.cs` |

# Implementation changes

```csharp
// App.Infrastructure/Concurrency/EntityVersionResolver.cs
public class EntityVersionResolver : IEntityVersionResolver
{
    private static readonly Dictionary<string, Type> _map = new()
    {
        ["{Entity}"] = typeof({EntityName}),
        // add new mutable entity types here when introduced
    };

    public Type? Resolve(string entityName)
        => _map.GetValueOrDefault(entityName);
}
```

# Rules

MUST:
- Every mutable entity registered in `_map`
- Keys match the entity name strings used in `IHasVersions` command properties and `ETagEncoder.Encode` calls
- Registered as `Singleton` in DI — static map, no request-scope state

# Anti-patterns
- `nameof({EntityName})` as dictionary key — breaks on class rename

# Check list
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Every mutable entity registered in `_map`
- [ ] Keys match business names used in `IHasVersions` and `ETagEncoder`
