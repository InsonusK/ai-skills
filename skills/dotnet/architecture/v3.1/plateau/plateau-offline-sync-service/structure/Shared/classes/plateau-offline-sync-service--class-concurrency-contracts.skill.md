---
name: plateau-offline-sync-service--class-concurrency-contracts
description: Classes IVersioned / IHasVersions / IEntityVersionResolver / IEntityVersionResolverFactory in the plateau-offline-sync-service plateau — the Shared/Concurrency contracts every layer of the optimistic-concurrency stack references
whenToUse: when creating or editing any of the four Shared/Concurrency interfaces, or wiring a new versioned entity into the concurrency stack
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
- Declare the four cross-cutting concurrency contracts in `Shared/Concurrency` so Domain, Application, Api, and Infrastructure can reference them without coupling to BuildingBlocks or App.Infrastructure.

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]

# Core Principles
- Apply ONE plateau template per class (this skill covers a contract family: one file per interface, all in `Shared/Concurrency`).
- `IVersioned` — `uint Version { get; }`, read-only; every mutable entity implements it. Never on an immutable entity, a DTO, or a command.
- `IHasVersions` — `IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }`; every update/patch command implements it. Keys are stable business names, never C# type names. Never on a create/delete command.
- `IEntityVersionResolver` — `Task<int> GetCurrentVersionForAsync(int id, CancellationToken)`; returns `0` when absent. Implemented in `{Module}.Application`, one per versioned entity.
- `IEntityVersionResolverFactory` — `IEntityVersionResolver? GetFor(string entityName)`; `null` for an unknown name. Implemented in `App.Infrastructure`.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-concurrency-contracts
// Plateau: domain-service
// Version: 20260902000000
namespace Shared.Concurrency;

public interface IVersioned { uint Version { get; } }

public interface IHasVersions
{
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }
}

public interface IEntityVersionResolver
{
    Task<int> GetCurrentVersionForAsync(int id, CancellationToken ct = default);
}

public interface IEntityVersionResolverFactory
{
    IEntityVersionResolver? GetFor(string entityName);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]

# Rules
MUST:
- Keep all four in `Shared/Concurrency`, one file per interface, member-free beyond the single declared member/method.
- `IVersioned.Version` read-only `uint`; `IHasVersions` keys stable business strings; `IEntityVersionResolver` returns `0` for absent; `IEntityVersionResolverFactory.GetFor` returns `null` for unknown.
- Never define any of these in BuildingBlocks; never add a method/setter to a marker.
- Never apply several plateau templates per class.

# Check list
- [ ] `IVersioned`, `IHasVersions`, `IEntityVersionResolver`, `IEntityVersionResolverFactory` each in its own file in `Shared/Concurrency`.
- [ ] `IVersioned.Version` read-only; resolver returns `0` for absent; factory returns `null` for unknown.
- [ ] No implementation, no BuildingBlocks/infrastructure reference.

# Unittest TestCases
- [ ] WHEN a mutable entity is inspected THEN it implements `IVersioned` and `Version` has no setter on the interface.
- [ ] WHEN an update command is inspected THEN it implements `IHasVersions` with stable string keys.
