---
description: Add uint Version property with internal set and implement IVersioned
project_name: "{Module}.Domain"
name: "{Entity}.cs"
element_kind: class
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/entity-cs
---

# Goals
- Add `Version` as a required property on all mutable entities
- Make every mutable entity implement `IVersioned` so the concurrency infrastructure can discover and read versions without reflection

# Core Principles
- `Version` is `uint` with `internal set` — never set by application code, only by database
- Present on Internal Mutable and External Mutable entity types — absent on Immutable entities
- Read by `ConcurrencyBehavior` via the entity loaded from the repository — never passed as a domain parameter
- `IVersioned` is defined in Shared and implemented in Domain — no dependency on BuildingBlocks or App.Infrastructure

# Naming convention
| use case | property name pattern | property name | type |
| --- | --- | --- | --- |
| Concurrency token | `Version` | `Version` | `uint` |

# Implementation changes

Mutable entity must declare `Version` and implement `IVersioned`:

```csharp
// {Module}.Domain/Entities/{Entity}.cs
using Shared.Concurrency;

public class {Entity} : IVersioned
{
    public int Id { get; internal set; }
    // ... other properties
    public uint Version { get; internal set; }   // ← added by this solution

    internal void SomeDomainMethod() { ... }
}
```

> **Note:** The stable business name used by `EntityVersionResolverFactory`, `IHasVersions`, and `ETagEncoder` lives in `{Entity}Config.VersionedEntityName` — not on the entity class. This keeps entity metadata centralized in the EF configuration.
# Rule changes

## MUST
- All mutable entities have `public uint Version { get; internal set; }`
- All mutable entities implement `IVersioned`
- Every mutable entity config class declares a public `const string VersionedEntityName` with the stable business name
- Each `{Entity}VersionResolver` declares `public const string VersionedEntityName` matching `{Entity}Config.VersionedEntityName`
- Never immutable entities have `Version` — they are never updated
- Never application code assign `Version` — it is controlled exclusively by the database

## SHOULD
- Avoid `Version` with `public set` — application code must never modify it
- Avoid reading `Version` via reflection instead of `IVersioned` in `ConcurrencyBehavior`

# Check list
- [ ] `uint Version { get; internal set; }` present on mutable entity
- [ ] Mutable entity implements `IVersioned`
- [ ] Immutable entities do not have `Version`

# Unittest TestCases
- [ ] WHEN applied THEN Add Version as a required property on all mutable entities
- [ ] WHEN applied THEN Version is uint with internal set — never set by application code, only by database
- [ ] WHEN applied THEN Present on Internal Mutable and External Mutable entity types — absent on Immutable entities
- [ ] WHEN applied THEN Read by ConcurrencyBehavior via the entity loaded from the repository — never passed as a domain parameter
- [ ] WHEN applied THEN All mutable entities implement IVersioned
- [ ] WHEN verified THEN uint Version { get; internal set; } present on mutable entity
- [ ] WHEN verified THEN Mutable entity implements IVersioned
- [ ] WHEN verified THEN Immutable entities do not have Version
- [ ] WHEN naming 'Concurrency token' THEN pattern matches convention
