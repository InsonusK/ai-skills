---
name: plateau-offline-sync-service--class-entity-version-resolver-factory
description: Class EntityVersionResolverFactory in the plateau-offline-sync-service plateau — maps a stable entity name to its Application-layer IEntityVersionResolver by scanning Domain configs and Application resolvers
whenToUse: when editing the version-resolver factory, adding a new versioned entity to the map, or checking how ConcurrencyBehavior resolves versions
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Provide the concrete `IEntityVersionResolverFactory`: given a stable entity name string, return the `{Module}.Application` resolver that can read that entity's current version — discovered automatically, never hardcoded.

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend/EntityVersionResolverFactory.cs.create.md|EntityVersionResolverFactory.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- In `/App.Infrastructure/Concurrency`. Constructor takes `IServiceProvider` + explicit Domain-assembly and Application-assembly lists.
- Builds the name→resolver-type map once (static, lazy, thread-safe double-check lock): scans Domain assemblies for `IEntityTypeConfiguration<T>` where `T : IVersioned` (valid names), scans Application assemblies for `IEntityVersionResolver` implementations, validates each resolver's `public const string VersionedEntityName` against the valid set.
- `GetFor(name)` resolves the mapped type from the request's provider — the factory is `Scoped`, the map is process-wide.
- Returns `null` for an unknown name (`ConcurrencyBehavior` maps that to `Result.Error`). Missing constant or unknown name = startup failure.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-entity-version-resolver-factory
// Plateau: domain-service
// Version: 20260902000000
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Concurrency;

namespace App.Infrastructure.Concurrency;

public sealed class EntityVersionResolverFactory(
    IServiceProvider provider,
    IEnumerable<Assembly> domainAssemblies,
    IEnumerable<Assembly> applicationAssemblies) : IEntityVersionResolverFactory
{
    // static thread-safe map name -> resolver Type, built once from the supplied assemblies;
    // GetFor(name) => (IEntityVersionResolver?)provider.GetService(mappedType)
}
```
Full scan logic is in the solution's implementation file.

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend/EntityVersionResolverFactory.cs.create.md|EntityVersionResolverFactory.cs.create]]

# Rules
MUST:
- Accept `IServiceProvider` + explicit Domain/Application assembly lists; build the map once, thread-safe.
- Scan Domain configs (`IEntityTypeConfiguration<T>` where `T : IVersioned`) and Application `IEntityVersionResolver` implementations; validate `VersionedEntityName` against the discovered set.
- Return `null` for an unknown name; be registered `Scoped`.
- Never hardcode a resolver dictionary; never key on a C# type name; never scan `AppDomain.CurrentDomain.GetAssemblies()`.
- Never apply several plateau templates per class.

# Check list
- [ ] In `/App.Infrastructure/Concurrency`; constructor takes provider + explicit assembly lists.
- [ ] Map built once, thread-safe; `GetFor` returns `null` for unknown names.
- [ ] Registered `Scoped`; no hardcoded map, no type-name keys.

# Unittest TestCases
- [ ] WHEN a resolver declares a `VersionedEntityName` not backed by a versioned Domain config THEN construction throws.
- [ ] WHEN `GetFor` is called with a known name THEN the mapped resolver instance is returned from the provider.
