---
name: project-feature-data-access
description: Generic template for any {feature}/data-access lib — Facade (public API) / Client (internal transport) / Mapper+Errors layering for one feature's HTTP data operations
domain: skill
type: template
plateau: data-capable
project_kind: library
version: 20260711140000
tags:
  - skill/template/project
  - plateau/data-capable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
---

> Generic pattern, not tied to one concrete feature — every business feature's `libs/{feature}/data-access` project follows this template, substituting `{Feature}`/`{feature}` with the real feature name. The bare project placeholder already existed in [[skills/angular/architecture/plateau/foundation/plateau-foundation.skill.md|foundation]]'s repository layout; `solution-api-http-layer` is the first solution to fill in its internal structure.

# Goal

- Give every feature a consistent internal structure for data operations: Facade (business logic, public API) → Client (transport/DTO mapping, internal) → shared `libs/shared/http-core`
- Give callers (a feature's Signal Store methods, or NgRx effects for global state) a single, typed, predictable error shape instead of raw HTTP errors

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Core Principles

- **Facade** is this feature's `data-access` public API: business validation and orchestration, calling the Client. It is the only thing this feature's Signal Store method is allowed to call.
- **Client** is internal, never exported: DTO mapping (via `{feature}.mapper.ts`) plus the actual HTTP call through `libs/shared/http-core`. It is the single point where a raw `HttpErrorResponse` is caught and converted into a typed domain error.
- DTO ↔ domain model mapping is always a hand-written function in `{feature}.mapper.ts`, including any enrichment from data not present in the DTO itself.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Structure

## Project Structure

```
/libs/{feature}/data-access
  /src
    /lib
      [{feature}.facade.ts](./classes/class-feature-facade.skill.md)
      [{feature}.client.ts](./classes/class-feature-client.skill.md)
      [{feature}.mapper.ts, {feature}.errors.ts](./classes/class-feature-mapper-and-errors.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.facade.ts` | Public API: business validation/orchestration, calls the Client, may enrich or coordinate across multiple Client calls. Exported from `index.ts`. | [[classes/class-feature-facade.skill.md\|class-feature-facade.skill]] |
| `{feature}.client.ts` | Internal: DTO mapping via the Mapper, calls `libs/shared/http-core`, catches `HttpErrorResponse` and throws a typed domain error. Never exported from `index.ts`. | [[classes/class-feature-client.skill.md\|class-feature-client.skill]] |
| `{feature}.mapper.ts` / `{feature}.errors.ts` | Internal: hand-written `dtoToModel`/`modelToDto` functions, and this feature's typed domain error hierarchy. The Facade may re-export the error types from `index.ts`. | [[classes/class-feature-mapper-and-errors.skill.md\|class-feature-mapper-and-errors.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/common (HttpClient, transitively via shared/http-core) | matching the Angular major version in use | Underlying HTTP transport |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]

## What Does NOT Belong Here

- UI components, feature-level state (Signal Store) — belong in this feature's own `libs/{feature}/feature`
- Direct `HttpClient` usage — every HTTP call goes through `libs/shared/http-core`'s base service

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

## Allowed Dependencies

- `libs/shared/http-core` (tag: `type:util`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- A feature's `{feature}.client.ts` MUST NOT be exported from that feature's `index.ts` — only the Facade (and, if useful, the feature's domain error types) is part of the public API.
- The Facade MUST be the only class in this project exported from `index.ts`, along with the feature's domain error types.

## MUST NOT
- A `type:data-access` project MUST only be imported by the `type:feature` project that shares its `scope`.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Putting DTO mapping or direct `HttpClient`/`http-core` calls inside the Facade**
  - Consequence: blurs the Facade/Client separation this solution exists to establish, making business logic and transport concerns hard to test independently
  - Instead: the Facade only calls the Client; all DTO/transport concerns stay inside the Client
- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation the Facade exists to enforce
  - Instead: the store always goes through the Facade

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Check list

- [ ] `index.ts` exports the Facade and domain error types only, never the Client or Mapper
- [ ] Every HTTP call in this project goes through `libs/shared/http-core`, not raw `HttpClient`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
