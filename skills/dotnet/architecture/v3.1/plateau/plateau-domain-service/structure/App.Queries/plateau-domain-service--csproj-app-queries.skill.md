---
name: plateau-domain-service--csproj-app-queries
description: Project App.Queries in the plateau-domain-service plateau — the only project that reads across module boundaries, holding cross-module JOIN projection specifications
whenToUse: when a read needs to join entities from more than one module, or deciding whether a specification belongs in a module's Application or in App.Queries
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Host cross-module JOIN projection specifications (`Specification<T, TResult>`) — the only place that may reference more than one module's `Domain` entity types at once.
- Keep every single-module spec out: those belong in the owning module's `{Module}.Application/Specifications`.

__Applied solutions:__
- [[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]

# Core Principles
- Contains only `Specification<T, TResult>` projection specs and the DTOs they project to; no handlers, no entities, no write path.
- Every spec projects to a read DTO inside the query — the caller never receives a tracked entity graph.
- Reads go through `IReadRepository<T>` against `AppDbContext`; App.Queries never opens its own `DbContext`.
- A single-module read stays in that module; App.Queries exists only for the genuinely cross-module case.

# Structure

## Solution place
```
/src/App/App.Queries
```

## Project Structure
- /App.Queries
  - /Specifications/[{Thing}ProjectionSpec.cs](./classes/plateau-domain-service--class-cross-module-projection-spec.skill.md)
  - /DTOs/{Thing}ReadModel.cs
  - App.Queries.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Specifications/{Thing}ProjectionSpec.cs | Cross-module JOIN → read DTO | [[./classes/plateau-domain-service--class-cross-module-projection-spec.skill.md\|class-cross-module-projection-spec]] |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| Ardalis.Specification | `Specification<T, TResult>` |

## What Does NOT Belong Here
- Single-module specs — belong to `{Module}.Application/Specifications`.
- Handlers, validators, write paths, entities.

## Allowed Dependencies
- `Shared`, every `{Module}.Domain`, every `{Module}.Interfaces`

# Rules
MUST:
- Contain only projection specs (`Specification<T, TResult>`) and their read DTOs.
- Project to a DTO in every spec — never expose a tracked entity graph across the boundary.
- Never host a single-module spec; never open a `DbContext`; never contain a write operation.

__Applied solutions:__
- [[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Check list
- [ ] `App.Queries.csproj` references `Shared` + module `Domain`/`Interfaces` only.
- [ ] Every spec is `Specification<T, TResult>` projecting to a read DTO.
- [ ] No handler, no write path, no `DbContext`.
