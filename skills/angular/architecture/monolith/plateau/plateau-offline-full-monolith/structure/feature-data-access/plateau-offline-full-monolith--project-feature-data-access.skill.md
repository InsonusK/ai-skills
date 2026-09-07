---
name: plateau-offline-full-monolith--project-feature-data-access
description: Generic template for any {feature}/data-access lib — Facade (public API) / Client (internal transport) / Mapper+Errors layering for one feature's HTTP data operations, each layer unit-tested at the correct boundary — offline-full-monolith plateau
domain: skill
type: template
whenToUse: when scaffolding or editing a libs/{feature}/data-access lib — the Facade/Client/Mapper/Errors layering — or checking only the Facade (and domain errors) is exported
plateau: offline-full-monolith
project_kind: library
version: 20260903120000
tags:
  - skill/template/project
  - plateau/offline-full-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"

> Generic pattern, not tied to one concrete feature — every business feature's `libs/{feature}/data-access` project follows this template. `solution-api-http-layer` fills in the Facade/Client/Mapper/Errors structure; VP4 adds the `status === 0` → `OfflineTransportError` branch to the Client; VP5 adds the `OfflineTransportError` → enqueue branch to the Facade (which now depends on `libs/shared/offline-sync`, `type:store`).

# Goal

- Give every feature a consistent internal structure for data operations: Facade (business logic, public API) → Client (transport/DTO mapping, internal) → shared `libs/shared/http-core`
- Give callers a single, typed, predictable error shape instead of raw HTTP errors
- Test the Client (the only place `HttpTestingController` is used) and the Facade (which fakes the Client) each in isolation, plus an occasional cross-layer integration test via MSW

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

# Core Principles

- **Facade** is this feature's `data-access` public API: business validation and orchestration, calling the Client. It is the only thing this feature's Signal Store method is allowed to call.
- **Client** is internal, never exported: DTO mapping (via `{feature}.mapper.ts`) plus the actual HTTP call through `libs/shared/http-core`. It is the single point where a raw `HttpErrorResponse` is caught and converted into a typed domain error.
- DTO ↔ domain model mapping is always a hand-written function in `{feature}.mapper.ts`, including any enrichment from data not present in the DTO itself.
- `HttpTestingController` is used only inside `{feature}.client.spec.ts` — every other spec fakes the layer directly beneath it.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

# Structure

## Project Structure

```
/libs/{feature}/data-access
  /src
    /lib
      [{feature}.facade.ts](./classes/plateau-offline-full-monolith--class-feature-facade.skill.md)
      {feature}.facade.spec.ts
      [{feature}.client.ts](./classes/plateau-offline-full-monolith--class-feature-client.skill.md)
      {feature}.client.spec.ts
      [{feature}.mapper.ts, {feature}.errors.ts](./classes/plateau-offline-full-monolith--class-feature-mapper-and-errors.skill.md)
      {feature}.integration.spec.ts        <- only for genuine cross-layer scenarios
    index.ts
```

### Multiple facets

When a feature's `data-access` lib has several distinct data facets, each facet keeps its own Facade/Client/Mapper trio and files are grouped by role under sub-folders:

```
libs/{feature}/data-access/src/lib
- facade/
  - {feature}_1.facade.ts
  - {feature}_2.facade.ts
- client/
  - {feature}_1.client.ts
  - {feature}_2.client.ts
- mapper/
  - {feature}_1.mapper.ts
  - {feature}_2.mapper.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.facade.ts` | Public API: business validation/orchestration, calls the Client. Exported from `index.ts`. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/classes/plateau-offline-full-monolith--class-feature-facade.skill\|class-feature-facade]] |
| `facade/{feature}_N.facade.ts` | Same as above, used when a feature has multiple distinct data facets. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/classes/plateau-offline-full-monolith--class-feature-facade.skill\|class-feature-facade]] |
| `{feature}.facade.spec.ts` | Vitest unit test faking the Client. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/classes/plateau-offline-full-monolith--class-feature-facade.skill\|class-feature-facade]] |
| `{feature}.client.ts` | Internal: DTO mapping via the Mapper, calls `libs/shared/http-core`, catches `HttpErrorResponse` and throws a typed domain error. Never exported. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/classes/plateau-offline-full-monolith--class-feature-client.skill\|class-feature-client]] |
| `client/{feature}_N.client.ts` | Same as above, used when a feature has multiple distinct data facets. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/classes/plateau-offline-full-monolith--class-feature-client.skill\|class-feature-client]] |
| `{feature}.client.spec.ts` | Vitest unit test using `HttpTestingController` — the only place it is used. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/classes/plateau-offline-full-monolith--class-feature-client.skill\|class-feature-client]] |
| `{feature}.mapper.ts` / `{feature}.errors.ts` | Internal: hand-written mapping functions and this feature's typed domain error hierarchy. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/classes/plateau-offline-full-monolith--class-feature-mapper-and-errors.skill\|class-feature-mapper-and-errors]] |
| `mapper/{feature}_N.mapper.ts` / `{feature}_N.errors.ts` | Same as above, used when a feature has multiple distinct data facets. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/feature-data-access/classes/plateau-offline-full-monolith--class-feature-mapper-and-errors.skill\|class-feature-mapper-and-errors]] |
| `{feature}.integration.spec.ts` | Reserved for the rare case that genuinely needs Store → Facade → Client wired together, using MSW at the network boundary. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create.md|Testing/{feature}.integration.spec.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/common (HttpClient, transitively via shared/http-core) | matching the Angular major version in use | Underlying HTTP transport |
| msw | latest compatible | Network-boundary mocking, reserved for genuine cross-layer integration specs |
| vitest | matching workspace configuration | Unit/integration test runner |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create.md|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create.md|Testing/{feature}.integration.spec.ts.create]]

## What Does NOT Belong Here

- UI components, feature-level state (Signal Store) — belong in this feature's own `libs/{feature}/feature`
- Direct `HttpClient` usage — every HTTP call goes through `libs/shared/http-core`'s base service
- `HttpTestingController` usage anywhere outside `{feature}.client.spec.ts`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

## Allowed Dependencies

- `libs/shared/http-core` (tag: `type:data-access`, `scope:shared`) — base HTTP service + `OfflineTransportError`
- `libs/shared/util` (tag: `type:util`, `scope:shared`)
- `libs/shared/offline-sync` (tag: `type:store`, `scope:shared`) — VP5: the Facade enqueues a queueable op via `MutationQueueService`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]

# Rules

## MUST
- A feature's `{feature}.client.ts` must never be exported from that feature's `index.ts` — only the Facade (and domain error types) is part of the public API.
- When a feature has multiple distinct data facets, each facet's files must be grouped under `facade/`, `client/`, and `mapper/` with names `{feature}_N.{kind}.ts`; every Facade is exported from `index.ts`, but no Client or Mapper is exported.
- The Facade must be the only class in this project exported from `index.ts`, along with the feature's domain error types.
- Every test in `{feature}.client.spec.ts` must use `HttpTestingController` to assert the exact request; `httpTesting.verify()` must run in `afterEach`.
- A Facade test must fake its Client directly — it must never use `HttpTestingController` or MSW.
- MSW must be used only for `{feature}.integration.spec.ts`-style tests that deliberately span more than one architectural layer.

- A `type:data-access` project must only be imported by the `type:feature` project that shares its `scope`.
- A component or Signal Store method must never import a feature's Client directly, bypassing the Facade.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]


- **Putting DTO mapping or direct `HttpClient`/`http-core` calls inside the Facade**
  - Consequence: blurs the Facade/Client separation, making business logic and transport concerns hard to test independently
  - Instead: the Facade only calls the Client; all DTO/transport concerns stay inside the Client
- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation the Facade exists to enforce
  - Instead: the store always goes through the Facade
- **Using `HttpTestingController` inside a Facade spec "to save time faking the Client"**
  - Consequence: the same HTTP call ends up asserted in two different, potentially inconsistent ways
  - Instead: fake the Client directly in the Facade spec
- **Reaching for the integration-spec pattern as the default way to test a Facade**
  - Consequence: slower, more complex tests than necessary, and duplicated mocking risk
  - Instead: default to the narrower unit-test pattern; reserve integration specs for genuine cross-layer scenarios

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]

# Check list

- [ ] `index.ts` exports the Facade and domain error types only, never the Client or Mapper
- [ ] When a feature has multiple data facets, each facet's files are grouped under `facade/`, `client/`, and `mapper/` with names `{feature}_N.{kind}.ts`
- [ ] Every HTTP call in this project goes through `libs/shared/http-core`, not raw `HttpClient`
- [ ] `HttpTestingController` appears only in `{feature}.client.spec.ts`
- [ ] Every Facade method has a spec faking the Client

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/Implementation/Repository.extend.md|Repository.extend]]
