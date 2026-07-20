---
name: plateau-online-monolith--project-feature-data-access
description: Generic template for any {feature}/data-access lib — Facade (public API) / Client (internal transport) / Mapper+Errors layering for one feature's HTTP data operations, each layer unit-tested at the correct boundary — online-monolith plateau
domain: skill
type: template
plateau: online-monolith
project_kind: library
version: 20260711180000
tags:
  - skill/template/project
  - plateau/online-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]]"
---

> Generic pattern, not tied to one concrete feature — every business feature's `libs/{feature}/data-access` project follows this template, substituting `{Feature}`/`{feature}` with the real feature name. The bare project placeholder already exists by [[skills/angular/architecture/plateau/plateau-online-monolith/structure/plateau-online-monolith--repo-online-monolith.skill.md|repo-online-monolith]]'s repository layout; `solution-api-http-layer` is the first solution to fill in its internal structure.

# Goal

- Give every feature a consistent internal structure for data operations: Facade (business logic, public API) → Client (transport/DTO mapping, internal) → shared `libs/shared/http-core`
- Give callers a single, typed, predictable error shape instead of raw HTTP errors
- Test the Client (the only place `HttpTestingController` is used) and the Facade (which fakes the Client) each in isolation, plus an occasional cross-layer integration test via MSW

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] - [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Core Principles

- **Facade** is this feature's `data-access` public API: business validation and orchestration, calling the Client. It is the only thing this feature's Signal Store method is allowed to call.
- **Client** is internal, never exported: DTO mapping (via `{feature}.mapper.ts`) plus the actual HTTP call through `libs/shared/http-core`. It is the single point where a raw `HttpErrorResponse` is caught and converted into a typed domain error.
- DTO ↔ domain model mapping is always a hand-written function in `{feature}.mapper.ts`, including any enrichment from data not present in the DTO itself.
- `HttpTestingController` is used only inside `{feature}.client.spec.ts` — every other spec fakes the layer directly beneath it.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] - [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Structure

## Project Structure

```
/libs/{feature}/data-access
  /src
    /lib
      [{feature}.facade.ts](./classes/plateau-online-monolith--class-feature-facade.skill.md)
      {feature}.facade.spec.ts
      [{feature}.client.ts](./classes/plateau-online-monolith--class-feature-client.skill.md)
      {feature}.client.spec.ts
      [{feature}.mapper.ts, {feature}.errors.ts](./classes/plateau-online-monolith--class-feature-mapper-and-errors.skill.md)
      {feature}.integration.spec.ts        <- only for genuine cross-layer scenarios
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.facade.ts` | Public API: business validation/orchestration, calls the Client. Exported from `index.ts`. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-data-access/classes/plateau-online-monolith--class-feature-facade.skill\|class-feature-facade]] |
| `{feature}.facade.spec.ts` | Vitest unit test faking the Client. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-data-access/classes/plateau-online-monolith--class-feature-facade.skill\|class-feature-facade]] |
| `{feature}.client.ts` | Internal: DTO mapping via the Mapper, calls `libs/shared/http-core`, catches `HttpErrorResponse` and throws a typed domain error. Never exported. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-data-access/classes/plateau-online-monolith--class-feature-client.skill\|class-feature-client]] |
| `{feature}.client.spec.ts` | Vitest unit test using `HttpTestingController` — the only place it is used. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-data-access/classes/plateau-online-monolith--class-feature-client.skill\|class-feature-client]] |
| `{feature}.mapper.ts` / `{feature}.errors.ts` | Internal: hand-written mapping functions and this feature's typed domain error hierarchy. | [[skills/angular/architecture/plateau/plateau-online-monolith/structure/feature-data-access/classes/plateau-online-monolith--class-feature-mapper-and-errors.skill\|class-feature-mapper-and-errors]] |
| `{feature}.integration.spec.ts` | Reserved for the rare case that genuinely needs Store → Facade → Client wired together, using MSW at the network boundary. | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] - [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create|Testing/{feature}.integration.spec.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/common (HttpClient, transitively via shared/http-core) | matching the Angular major version in use | Underlying HTTP transport |
| msw | latest compatible | Network-boundary mocking, reserved for genuine cross-layer integration specs |
| vitest | matching workspace configuration | Unit/integration test runner |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] - [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create|Testing/{feature}.integration.spec.ts.create]]

## What Does NOT Belong Here

- UI components, feature-level state (Signal Store) — belong in this feature's own `libs/{feature}/feature`
- Direct `HttpClient` usage — every HTTP call goes through `libs/shared/http-core`'s base service
- `HttpTestingController` usage anywhere outside `{feature}.client.spec.ts`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] - [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]

## Allowed Dependencies

- `libs/shared/http-core` (tag: `type:util`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- A feature's `{feature}.client.ts` MUST NOT be exported from that feature's `index.ts` — only the Facade (and domain error types) is part of the public API.
- The Facade MUST be the only class in this project exported from `index.ts`, along with the feature's domain error types.
- Every test in `{feature}.client.spec.ts` MUST use `HttpTestingController` to assert the exact request; `httpTesting.verify()` MUST run in `afterEach`.
- A Facade test MUST fake its Client directly — it MUST NOT use `HttpTestingController` or MSW.
- MSW MUST be used only for `{feature}.integration.spec.ts`-style tests that deliberately span more than one architectural layer.

## MUST NOT
- A `type:data-access` project MUST only be imported by the `type:feature` project that shares its `scope`.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] - [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

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
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] - [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]

# Check list

- [ ] `index.ts` exports the Facade and domain error types only, never the Client or Mapper
- [ ] Every HTTP call in this project goes through `libs/shared/http-core`, not raw `HttpClient`
- [ ] `HttpTestingController` appears only in `{feature}.client.spec.ts`
- [ ] Every Facade method has a spec faking the Client

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] - [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/Implementation/Repository.extend|Repository.extend]]
