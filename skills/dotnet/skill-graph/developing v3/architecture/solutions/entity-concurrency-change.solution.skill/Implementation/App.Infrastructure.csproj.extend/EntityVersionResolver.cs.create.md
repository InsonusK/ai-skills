---
description: Maps stable business entity names to C# types by scanning assemblies for IVersioned implementations
project_name: App.Infrastructure
name: EntityVersionResolver.cs
element_kind: class
change_kind: create
---

# Goals
- Provide the concrete mapping from stable entity name strings to C# entity types
- Discover versioned entities automatically by scanning supplied assemblies for `IVersioned` implementations
- Be the single place to update when new mutable entities are added to the solution — usually just adding the entity type is enough

# Core Principles
- Read-only dictionary — populated once at startup from the supplied assemblies, no runtime modification
- Keys are stable business names agreed with the frontend — changing a key is a breaking API change
- Entity types are discovered from assemblies supplied during registration — typically module Domain assemblies
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity name resolver | `EntityVersionResolver` | `EntityVersionResolver` | `EntityVersionResolver.cs` | `EntityVersionResolver.cs` |

# Implementation changes

```csharp
// App.Infrastructure/Concurrency/EntityVersionResolver.cs
using System.Reflection;
using Shared.Concurrency;

namespace App.Infrastructure.Concurrency;

public class EntityVersionResolver : IEntityVersionResolver
{
    private readonly IReadOnlyDictionary<string, Type> _map;

    public EntityVersionResolver(IEnumerable<Assembly> assemblies)
    {
        _map = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IVersioned).IsAssignableFrom(t))
            .ToDictionary(GetEntityKey, t => t);
    }

    private static string GetEntityKey(Type type)
    {
        var key = type.GetField(
                "VersionedEntityName",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?.GetValue(null) as string;

        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException(
                $"Versioned entity '{type.FullName}' must declare a public 'VersionedEntityName' constant.");
        }

        return key;
    }

    public Type? Resolve(string entityName)
        => _map.GetValueOrDefault(entityName);
}
```

> **Note on entity name discovery:** Each `IVersioned` entity MUST declare a public constant for its stable business name:
> ```csharp
> public class TodoTask : IVersioned
> {
>     public const string VersionedEntityName = "Task";
>     // ...
> }
> ```
> `EntityVersionResolver` reads this constant at startup. Missing the constant causes startup failure so the contract is explicit and never falls back to fragile C# type names.

# Rules

MUST:
- Every mutable entity implements `IVersioned` so it is discovered automatically
- Every mutable entity declares a public `VersionedEntityName` constant with its stable business name
- Keys match the entity name strings used in `IHasVersions` command properties and `ETagEncoder.Encode` calls
- Registered as `Singleton` in DI — map is built once at startup, no request-scope state
- Accepts `IEnumerable<Assembly>` in its constructor so the composition root supplies the assemblies to scan

# Anti-patterns
- `nameof({EntityName})` or `type.Name` as dictionary key — fragile, breaks on class rename
- Manually listing every entity in a hardcoded dictionary — duplicates the entity list and is easy to forget
- Pulling in all loaded assemblies via `AppDomain.CurrentDomain.GetAssemblies()` — can include unrelated assemblies and dynamic types; explicit assembly list from the composition root is safer

# Check list
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Constructor accepts `IEnumerable<Assembly>`
- [ ] Scans supplied assemblies for `IVersioned` implementations
- [ ] Every mutable entity implements `IVersioned`
- [ ] Every mutable entity declares a public `VersionedEntityName` constant
- [ ] Keys match business names used in `IHasVersions` and `ETagEncoder`

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide the concrete mapping from stable entity name strings to C# entity types
- [ ] WHEN applied THEN Discover versioned entities automatically by scanning supplied assemblies for IVersioned implementations
- [ ] WHEN applied THEN Be the single place to update when new mutable entities are added to the solution — usually just adding the entity type is enough
- [ ] WHEN applied THEN Read-only dictionary — populated once at startup from the supplied assemblies, no runtime modification
- [ ] WHEN applied THEN Keys are stable business names declared as VersionedEntityName constants — changing a key is a breaking API change
- [ ] WHEN applied THEN Returns null for unknown names — ConcurrencyBehavior returns Result.Error on null
- [ ] WHEN verified THEN EntityVersionResolver defined in App.Infrastructure/Concurrency/EntityVersionResolver.cs
- [ ] WHEN verified THEN Constructor accepts IEnumerable<Assembly>
- [ ] WHEN verified THEN Scans supplied assemblies for IVersioned implementations
- [ ] WHEN verified THEN Every mutable entity implements IVersioned
- [ ] WHEN verified THEN Every mutable entity declares a public VersionedEntityName constant
- [ ] WHEN naming 'Entity name resolver' THEN pattern matches convention
