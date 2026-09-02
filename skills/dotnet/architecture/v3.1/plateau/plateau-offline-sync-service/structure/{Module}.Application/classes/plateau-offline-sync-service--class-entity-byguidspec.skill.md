---
name: plateau-offline-sync-service--class-entity-byguidspec
description: Class {Entity}ByGuidSpec in the plateau-offline-sync-service plateau — the named Ardalis specification to look up an external-created entity by its client Guid
whenToUse: when adding Guid lookup for a new external-created entity, or editing the spec
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
---

# Goal
- Provide the named single-condition spec (`Query.Where(e => e.Guid == guid)`) used by `Create{Entity}GuidResolver` — and any feature needing Guid-based lookup.

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed class {Entity}ByGuidSpec : Specification<{Entity}>` in `/{Module}.Application/Specifications`; filters by `Guid` only.
- No DB call, no `DbContext`, no business logic. Every external-created entity has one.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-entity-byguidspec
// Plateau: offline-sync-service
// Version: 20260902000000
using Ardalis.Specification;
using {Module}.Domain.Entities;

namespace {Module}.Application.Specifications;

public sealed class {Entity}ByGuidSpec : Specification<{Entity}>
{
    public {Entity}ByGuidSpec(System.Guid guid) => Query.Where(e => e.Guid == guid);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]

# Rules
MUST:
- `Specification<{Entity}>` in `/{Module}.Application/Specifications`, filtering by `Guid` only.
- No DB call / `DbContext` / business logic; never placed in `{Module}.Domain`.
- Never apply several plateau templates per class.

# Check list
- [ ] `{Entity}ByGuidSpec` in `/Specifications`, filters by `Guid`.

# Unittest TestCases
- [ ] WHEN evaluated THEN it returns the entity with the matching Guid.
