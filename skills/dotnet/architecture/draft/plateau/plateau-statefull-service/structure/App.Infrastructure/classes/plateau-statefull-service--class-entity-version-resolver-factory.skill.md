---
name: plateau-statefull-service--class-entity-version-resolver-factory
description: Class EntityVersionResolverFactory in the statefull-service plateau
whenToUse: when reviewing how a stable entity-name string maps to the resolver that reads that entity's current version
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Map a stable business entity name to the `IEntityVersionResolver` that can read its current version, discovered by scanning assemblies rather than hardcoded

# Core Principles
- Scans module Domain assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`, and module Application assemblies for `IEntityVersionResolver` implementations — never a hardcoded dictionary
- Registered `Scoped` — resolvers it returns use `Scoped` repositories

# Implementation
```csharp
//Skill: class-entity-version-resolver-factory
//Plateau: statefull-service
//Version: 20260824100000

public sealed class EntityVersionResolverFactory(IServiceProvider services) : IEntityVersionResolverFactory
{
    private readonly Dictionary<string, IEntityVersionResolver> _resolvers =
        services.GetServices<IEntityVersionResolver>().ToDictionary(r => r.VersionedEntityName);

    public IEntityVersionResolver GetFor(string entityName) =>
        _resolvers.TryGetValue(entityName, out var resolver)
            ? resolver
            : throw new InvalidOperationException($"No IEntityVersionResolver registered for '{entityName}'.");
}
```

# Rules
MUST:
- Live in `App.Infrastructure/Concurrency`, registered `Scoped`
- Build its map from DI-resolved `IEntityVersionResolver` instances — never a hardcoded dictionary of type names
MUST NOT:
- Key the map by `nameof(TEntity)`/`type.Name` — only the stable `VersionedEntityName` constant

# Check list
- [ ] `EntityVersionResolverFactory` in `App.Infrastructure/Concurrency`, registered `Scoped`
- [ ] Map built from resolved `IEntityVersionResolver` instances, keyed by `VersionedEntityName`

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend/EntityVersionResolverFactory.cs.create.md|EntityVersionResolverFactory.cs.create]]
