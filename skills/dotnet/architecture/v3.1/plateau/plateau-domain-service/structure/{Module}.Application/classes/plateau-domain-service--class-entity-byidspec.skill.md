---
name: plateau-domain-service--class-entity-byidspec
description: Class {Entity}ByIdSpec in the plateau-domain-service plateau — the named Ardalis specification to load one entity by Id, in {Module}.Application/Specifications
whenToUse: when a handler, check, or version resolver needs to load an entity by its internal Id, or writing a new named specification
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
---

# Goal
- Provide one reusable named query per entity for loading it by internal Id, so handlers express query intent by name — no inline `Where(...)` LINQ.

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed class {Entity}ByIdSpec : Specification<{Entity}>` in `/{Module}.Application/Specifications`; constructor takes the id and calls `Query.Where(e => e.Id == id)`.
- Entity-filter spec uses `Specification<T>`; a projection spec uses `Specification<T, TResult>`.
- Never touches the database, never references `DbContext`, never contains a business rule.
- Name reflects intent, not field names (`{Entity}ByIdSpec`, not `{Entity}WhereId`); no generic `ByIdSpec` shared across entities.

# Implementation
```csharp
// Skill: plateau-domain-service--class-entity-byidspec
// Plateau: domain-service
// Version: 20260902000000
using Ardalis.Specification;
using {Module}.Domain.Entities;

namespace {Module}.Application.Specifications;

public sealed class {Entity}ByIdSpec : Specification<{Entity}>
{
    public {Entity}ByIdSpec(int id) => Query.Where(e => e.Id == id);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]]

# Rules
MUST:
- Inherit `Specification<{Entity}>`, be named `{Entity}ByIdSpec`, live in `/{Module}.Application/Specifications`.
- Contain only `Query.Where(...)` — no DB call, no `DbContext`, no business logic.
- Never use a generic `ByIdSpec` across entities; never put a spec in `{Module}.Domain`; every cross-module JOIN spec goes to `App.Queries`.
- Never apply several plateau templates per class.

# Check list
- [ ] `Specification<{Entity}>` in `/Specifications`; constructor applies `Query.Where(e => e.Id == id)`.
- [ ] No DB call / `DbContext` / business rule.

# Unittest TestCases
- [ ] WHEN evaluated against a set of entities THEN it returns the one with the matching Id.
