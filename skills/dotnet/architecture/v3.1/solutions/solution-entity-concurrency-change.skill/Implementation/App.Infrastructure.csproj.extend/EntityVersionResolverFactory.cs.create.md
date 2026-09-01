---
description: Factory that maps stable business entity names to Application-layer IEntityVersionResolver implementations
project_name: App.Infrastructure
name: EntityVersionResolverFactory.cs
element_kind: class
change_kind: create
tags:
  - solution/entity-concurrency-change
  - element/entityversionresolverfactory-cs
---

# Goals
- Provide the concrete mapping from stable entity name strings to the `IEntityVersionResolver` implementation that can read that entity's version
- Discover versioned entities from Domain config classes and wire them to Application resolver classes automatically
- Be the single place to update when new mutable entities are added — the config class already exists, just add `{Entity}VersionResolver`

# Core Principles
- Read-only map — populated once (static/lazy) from supplied assemblies, no runtime modification
- Keys are stable business names declared in `{Entity}Config.VersionedEntityName` and repeated on `{Entity}VersionResolver.VersionedEntityName`
- Domain assemblies are scanned to validate that every resolver references a real versioned entity
- Application assemblies are scanned for concrete `IEntityVersionResolver` implementations
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity version resolver factory | `EntityVersionResolverFactory` | `EntityVersionResolverFactory` | `EntityVersionResolverFactory.cs` | `EntityVersionResolverFactory.cs` |

# Implementation changes

```csharp
// App.Infrastructure/Concurrency/EntityVersionResolverFactory.cs
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Shared.Concurrency;

namespace App.Infrastructure.Concurrency;

public class EntityVersionResolverFactory : IEntityVersionResolverFactory
{
    private static readonly Dictionary<string, Type> _resolverTypes = new(StringComparer.Ordinal);
    private static readonly object _lock = new();
    private static bool _initialized;

    private readonly IServiceProvider _serviceProvider;

    public EntityVersionResolverFactory(
        IServiceProvider serviceProvider,
        IEnumerable<Assembly> domainAssemblies,
        IEnumerable<Assembly> applicationAssemblies)
    {
        _serviceProvider = serviceProvider;
        Initialize(domainAssemblies, applicationAssemblies);
    }

    public IEntityVersionResolver? GetFor(string entityName)
    {
        if (!_resolverTypes.TryGetValue(entityName, out var resolverType))
            return null;

        return (IEntityVersionResolver)_serviceProvider.GetRequiredService(resolverType);
    }

    private static void Initialize(IEnumerable<Assembly> domainAssemblies, IEnumerable<Assembly> applicationAssemblies)
    {
        if (_initialized)
            return;

        lock (_lock)
        {
            if (_initialized)
                return;

            foreach (var (name, type) in BuildMap(domainAssemblies, applicationAssemblies))
            {
                _resolverTypes[name] = type;
            }

            _initialized = true;
        }
    }

    private static Dictionary<string, Type> BuildMap(
        IEnumerable<Assembly> domainAssemblies,
        IEnumerable<Assembly> applicationAssemblies)
    {
        var validEntityNames = BuildEntityNameSet(domainAssemblies);
        var map = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var assembly in applicationAssemblies)
        {
            var resolverTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                    && typeof(IEntityVersionResolver).IsAssignableFrom(t));

            foreach (var resolverType in resolverTypes)
            {
                var entityName = GetVersionedEntityName(resolverType);
                if (string.IsNullOrEmpty(entityName))
                {
                    throw new InvalidOperationException(
                        $"'{resolverType.FullName}' must declare a public 'VersionedEntityName' constant.");
                }

                if (!validEntityNames.Contains(entityName))
                {
                    throw new InvalidOperationException(
                        $"'{resolverType.FullName}' references unknown entity name '{entityName}'.");
                }

                map[entityName] = resolverType;
            }
        }

        return map;
    }

    private static HashSet<string> BuildEntityNameSet(IEnumerable<Assembly> assemblies)
    {
        var configInterface = typeof(IEntityTypeConfiguration<>);
        var set = new HashSet<string>(StringComparer.Ordinal);

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

                var entityName = GetVersionedEntityName(configType);
                if (string.IsNullOrEmpty(entityName))
                {
                    throw new InvalidOperationException(
                        $"'{configType.FullName}' must declare a public 'VersionedEntityName' constant.");
                }

                set.Add(entityName);
            }
        }

        return set;
    }

    private static string? GetVersionedEntityName(Type type)
        => type.GetField(
                "VersionedEntityName",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            ?.GetValue(null) as string;
}
```

> **Note on resolver discovery:** `{Entity}VersionResolver` in `{Module}.Application` must implement `IEntityVersionResolver` and declare:
> ```csharp
> public class TodoTaskVersionResolver : IEntityVersionResolver
> {
>     public const string VersionedEntityName = "Task";
>     // ...
> }
> ```
> `EntityVersionResolverFactory` finds every resolver class, checks that its `VersionedEntityName` matches a versioned entity discovered from Domain configs, and wires the name to the resolver type. Missing the constant or an unknown name causes startup failure.
>
> **Note on lifetime:** The resolver-type map is stored in a static dictionary and initialized only once (thread-safe double-check locking). The factory itself remains `Scoped` so it resolves `IEntityVersionResolver` instances from the request's service provider.
# Rule changes

## MUST
- Accept `IServiceProvider`, `IEnumerable<Assembly> domainAssemblies`, and `IEnumerable<Assembly> applicationAssemblies`
- Scan Domain assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- Scan Application assemblies for concrete `IEntityVersionResolver` implementations
- Validate that every resolver's `VersionedEntityName` maps to a discovered Domain entity
- Return `null` for unknown entity names
- Build the resolver-type map only once (static, lazy, thread-safe)
- Be registered as `Scoped` in DI because it resolves `Scoped` resolvers from the request service provider
- `IVersioned`, `IHasVersions`, `IEntityVersionResolverFactory`, and `IEntityVersionResolver` live in Shared
- `EntityVersionResolverFactory` lives in App.Infrastructure and maps entity names to resolver types by scanning module Domain config classes and module Application resolver classes
- Entity name keys in `IHasVersions` and `EntityVersionResolverFactory` are stable business strings — never C# type names
- Never use a hardcoded dictionary of resolver types
- Never read the entity name from the entity class or C# type name
- Never create resolvers without using the DI container

## SHOULD
- Avoid `nameof({EntityName})` or `type.Name` as dictionary key — fragile, breaks on class rename
- Avoid manually listing every resolver in a hardcoded dictionary — duplicates the entity list and is easy to forget
- Avoid pulling in all loaded assemblies via `AppDomain.CurrentDomain.GetAssemblies()` — can include unrelated assemblies and dynamic types; explicit assembly list from the composition root is safer

# Check list
- [ ] `EntityVersionResolverFactory` defined in `App.Infrastructure/Concurrency/EntityVersionResolverFactory.cs`
- [ ] Constructor accepts `IServiceProvider`, Domain assemblies, and Application assemblies
- [ ] Scans Domain assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- [ ] Scans Application assemblies for `IEntityVersionResolver` implementations
- [ ] Validates resolver `VersionedEntityName` against discovered Domain entities
- [ ] Returns `null` for unknown entity names
- [ ] Resolver-type map is built only once and thread-safe

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide the concrete mapping from stable entity name strings to the IEntityVersionResolver implementation
- [ ] WHEN applied THEN Discover versioned entities from Domain config classes and wire them to Application resolver classes automatically
- [ ] WHEN applied THEN Be the single place to update when new mutable entities are added — the config class already exists, just add {Entity}VersionResolver
- [ ] WHEN applied THEN Read-only map — populated once (static/lazy) from supplied assemblies, no runtime modification
- [ ] WHEN applied THEN Keys are stable business names declared in {Entity}Config.VersionedEntityName and {Entity}VersionResolver.VersionedEntityName
- [ ] WHEN applied THEN Returns null for unknown names — ConcurrencyBehavior returns Result.Error on null
- [ ] WHEN verified THEN EntityVersionResolverFactory defined in App.Infrastructure/Concurrency/EntityVersionResolverFactory.cs
- [ ] WHEN verified THEN Constructor accepts IServiceProvider, domainAssemblies, and applicationAssemblies
- [ ] WHEN verified THEN Scans Domain assemblies for IEntityTypeConfiguration<T> configs
- [ ] WHEN verified THEN Scans Application assemblies for IEntityVersionResolver implementations
- [ ] WHEN naming 'Entity version resolver factory' THEN pattern matches convention
