---
description: Maps stable business entity names to C# types by scanning config classes for IVersioned entities
project_name: App.Infrastructure
name: EntityVersionResolver.cs
element_kind: class
change_kind: create
---

# Goals
- Provide the concrete mapping from stable entity name strings to C# entity types
- Discover versioned entities automatically by scanning supplied assemblies for `IEntityTypeConfiguration<T>` classes whose entity `T` implements `IVersioned`
- Be the single place to update when new mutable entities are added to the solution — the config class already exists, just add `VersionedEntityName`

# Core Principles
- Read-only dictionary — populated once at startup from the supplied assemblies, no runtime modification
- Keys are stable business names declared in `{Entity}Config.VersionedEntityName` — changing a key is a breaking API change
- Entity types are discovered from config classes in assemblies supplied during registration — typically module Domain assemblies
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity name resolver | `EntityVersionResolver` | `EntityVersionResolver` | `EntityVersionResolver.cs` | `EntityVersionResolver.cs` |

# Implementation changes

```csharp
// App.Infrastructure/Concurrency/EntityVersionResolver.cs
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Shared.Concurrency;

namespace App.Infrastructure.Concurrency;

public class EntityVersionResolver : IEntityVersionResolver
{
    private readonly IReadOnlyDictionary<string, Type> _map;

    public EntityVersionResolver(IEnumerable<Assembly> assemblies)
    {
        _map = BuildMap(assemblies);
    }

    private static Dictionary<string, Type> BuildMap(IEnumerable<Assembly> assemblies)
    {
        var configInterface = typeof(IEntityTypeConfiguration<>);
        var map = new Dictionary<string, Type>();

        foreach (var assembly in assemblies)
        {
            var configMappings = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == configInterface)
                    .Select(i => new { ConfigType = t, EntityType = i.GetGenericArguments()[0] }));

            foreach (var (configType, entityType) in configMappings)
            {
                if (!typeof(IVersioned).IsAssignableFrom(entityType))
                    continue;

                var key = configType.GetField(
                        "VersionedEntityName",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    ?.GetValue(null) as string;

                if (string.IsNullOrEmpty(key))
                {
                    throw new InvalidOperationException(
                        $"'{configType.FullName}' must declare a public 'VersionedEntityName' constant.");
                }

                map[key] = entityType;
            }
        }

        return map;
    }

    public Type? Resolve(string entityName)
        => _map.GetValueOrDefault(entityName);
}
```

> **Note on config discovery:** `{Entity}Config` must implement `IEntityTypeConfiguration<{Entity}>` and declare:
> ```csharp
> public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
> {
>     public const string VersionedEntityName = "Task";
>     // ...
> }
> ```
> `EntityVersionResolver` finds every config class, checks whether the configured entity implements `IVersioned`, and reads `VersionedEntityName` from the config. Missing the constant causes startup failure.

# Rules

MUST:
- Every mutable entity implements `IVersioned`
- Every mutable entity config class declares `public const string VersionedEntityName`
- Discover configs via `IEntityTypeConfiguration<T>` generic argument
- Keys match the entity name strings used in `IHasVersions` command properties and `ETagEncoder.Encode` calls
- Registered as `Singleton` in DI — map is built once at startup, no request-scope state
- Accepts `IEnumerable<Assembly>` in its constructor so the composition root supplies the assemblies to scan

MUST NOT:
- Read the entity name from the entity class or C# type name
- Use a hardcoded dictionary of entity types

# Anti-patterns
- `nameof({EntityName})` or `type.Name` as dictionary key — fragile, breaks on class rename
- Manually listing every entity in a hardcoded dictionary — duplicates the entity list and is easy to forget
- Pulling in all loaded assemblies via `AppDomain.CurrentDomain.GetAssemblies()` — can include unrelated assemblies and dynamic types; explicit assembly list from the composition root is safer
- Putting `VersionedEntityName` on the entity class — spreads configuration across the domain

# Check list
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Constructor accepts `IEnumerable<Assembly>`
- [ ] Scans supplied assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- [ ] Every mutable entity implements `IVersioned`
- [ ] Every mutable entity config class declares `VersionedEntityName`
- [ ] Keys match business names used in `IHasVersions` and `ETagEncoder`

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide the concrete mapping from stable entity name strings to C# entity types
- [ ] WHEN applied THEN Discover versioned entities automatically by scanning config classes for IEntityTypeConfiguration<T> where T implements IVersioned
- [ ] WHEN applied THEN Be the single place to update when new mutable entities are added to the solution — the config class already exists, just add VersionedEntityName
- [ ] WHEN applied THEN Read-only dictionary — populated once at startup from the supplied assemblies, no runtime modification
- [ ] WHEN applied THEN Keys are stable business names declared in {Entity}Config.VersionedEntityName — changing a key is a breaking API change
- [ ] WHEN applied THEN Returns null for unknown names — ConcurrencyBehavior returns Result.Error on null
- [ ] WHEN verified THEN EntityVersionResolver defined in App.Infrastructure/Concurrency/EntityVersionResolver.cs
- [ ] WHEN verified THEN Constructor accepts IEnumerable<Assembly>
- [ ] WHEN verified THEN Scans supplied assemblies for IEntityTypeConfiguration<T> configs
- [ ] WHEN verified THEN Every mutable entity implements IVersioned
- [ ] WHEN verified THEN Every mutable entity config class declares VersionedEntityName
- [ ] WHEN naming 'Entity name resolver' THEN pattern matches convention
