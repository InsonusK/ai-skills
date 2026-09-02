---
name: plateau-domain-service--class-entity-version-resolver
description: Class {Entity}VersionResolver in the plateau-domain-service plateau — the per-entity IEntityVersionResolver implementation, reading one entity's current version via its {Entity}ByIdSpec
whenToUse: when adding a version resolver for a newly versioned entity, or editing an existing one
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Provide the entity-specific `IEntityVersionResolver` for `{Entity}`, reusing the module's `{Entity}ByIdSpec` and `IReadRepository<{Entity}>` so `ConcurrencyBehavior` can read the current version without knowing entity types.

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/{Module}.Application.csproj.extend/{Entity}VersionResolver.cs.create.md|{Entity}VersionResolver.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed class {Entity}VersionResolver : IEntityVersionResolver` in `/{Module}.Application/Concurrency`; no base class.
- Injects `IReadRepository<{Entity}>`; loads via `{Entity}ByIdSpec` (never inline LINQ, never `DbContext`).
- `public const string VersionedEntityName` mirrors `{Entity}Config.VersionedEntityName` — the factory routes on this.
- Returns `(int)entity.Version` when found, `0` when absent (never throws for a missing entity).
- Registered in the module (`services.AddScoped<{Entity}VersionResolver>()`) so `EntityVersionResolverFactory` can resolve it.

# Implementation
```csharp
// Skill: plateau-domain-service--class-entity-version-resolver
// Plateau: domain-service
// Version: 20260902000000
using Shared.Concurrency;
using Shared.Repositories;
using {Module}.Application.Specifications;
using {Module}.Domain.Entities;

namespace {Module}.Application.Concurrency;

public sealed class {Entity}VersionResolver(IReadRepository<{Entity}> repository) : IEntityVersionResolver
{
    public const string VersionedEntityName = "{Entity}";

    public async Task<int> GetCurrentVersionForAsync(int id, CancellationToken ct = default)
    {
        var entity = await repository.FirstOrDefaultAsync(new {Entity}ByIdSpec(id), ct);
        return entity is null ? 0 : (int)entity.Version;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/{Module}.Application.csproj.extend/{Entity}VersionResolver.cs.create.md|{Entity}VersionResolver.cs.create]]

# Rules
MUST:
- Implement `IEntityVersionResolver` in `/{Module}.Application/Concurrency`; use `IReadRepository<{Entity}>` + `{Entity}ByIdSpec`.
- Declare `public const string VersionedEntityName` matching `{Entity}Config.VersionedEntityName`.
- Return `0` for a missing entity; never throw; never reference EF Core / `DbContext`.
- Be registered `Scoped` in the module registration.
- Never apply several plateau templates per class.

# Check list
- [ ] `IEntityVersionResolver` in `/Concurrency`; `VersionedEntityName` matches the config.
- [ ] Loads via `{Entity}ByIdSpec` + `IReadRepository<{Entity}>`; returns `0` for absent.
- [ ] Registered `Scoped`.

# Unittest TestCases
- [ ] WHEN the entity exists THEN its `Version` (as `int`) is returned.
- [ ] WHEN the entity is absent THEN `0` is returned (no throw).
