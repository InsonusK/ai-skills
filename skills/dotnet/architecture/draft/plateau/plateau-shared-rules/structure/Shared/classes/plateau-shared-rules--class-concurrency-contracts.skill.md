---
name: class-concurrency-contracts
description: Classes IVersioned/IHasVersions/IEntityVersionResolverFactory/IEntityVersionResolver in the shared-rules plateau
whenToUse: when an entity needs optimistic concurrency control, or an update command needs to carry client-supplied version information
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Give every mutable entity a stable `Version` contract (`IVersioned`), every update command a way to carry expected versions (`IHasVersions`), and a factory/resolver pair that reads an entity's current version by its stable business name

# Core Principles
- Entity name string keys are stable business names — never C# type names — decoupling the HTTP/transport contract from assembly structure
- The factory maps a name to a resolver; each resolver reads one entity type's current version via `IReadRepository<T>` + `{Entity}ByIdSpec`

# Implementation
```csharp
//Skill: class-concurrency-contracts
//Plateau: shared-rules
//Version: 20260824163000

public interface IVersioned
{
    uint Version { get; }
}

public interface IHasVersions
{
    IReadOnlyDictionary<string, uint> Versions { get; }
}

public interface IEntityVersionResolver
{
    string VersionedEntityName { get; }
    Task<uint?> GetCurrentVersionForAsync(int id, CancellationToken ct);
}

public interface IEntityVersionResolverFactory
{
    IEntityVersionResolver GetFor(string entityName);
}
```

# Rules
MUST:
- All four live in `Shared/Concurrency`
- `IHasVersions` keys are stable business names, never `nameof(TEntity)`/type names
MUST NOT:
- Define these in BuildingBlocks — Shared owns all common contracts

# Check list
- [ ] `IVersioned`, `IHasVersions`, `IEntityVersionResolverFactory`, `IEntityVersionResolver` all defined in `Shared/Concurrency`

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend.md|Shared.csproj.extend]]
