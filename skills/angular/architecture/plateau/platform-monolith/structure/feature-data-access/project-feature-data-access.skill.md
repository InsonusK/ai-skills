---
name: project-feature-data-access
description: Generic template for any {feature}/data-access lib — Facade (public API) / Client (internal transport) / Mapper+Errors layering, the Client distinguishing an offline network failure from a genuine server rejection, and the Facade now able to queue a mutation for later sync
domain: skill
type: template
plateau: platform-monolith
project_kind: library
version: 20260711210000
tags:
  - skill/template/project
  - plateau/platform-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

> Generic pattern, not tied to one concrete feature — every business feature's `libs/{feature}/data-access` project follows this template, substituting `{Feature}`/`{feature}` with the real feature name. The bare project placeholder already existed in [[skills/angular/architecture/plateau/online-monolith/plateau-online-monolith.skill.md|online-monolith]]; `solution-offline-first` extends the Client's error handling, `solution-offline-sync` extends the Facade with the queueing decision.

# Goal

- Give every feature a consistent internal structure for data operations: Facade (business logic, public API) → Client (transport/DTO mapping, internal) → shared `libs/shared/http-core`
- Give callers a single, typed, predictable error shape instead of raw HTTP errors
- Let the Client distinguish "this failed because we're offline" from "the server rejected this request", via the shared `OfflineTransportError`
- Let the Facade queue a mutation for later sync, for operations it explicitly opts in, instead of surfacing an immediate failure
- Test the Client (the only place `HttpTestingController` is used) and the Facade (which fakes the Client) each in isolation, plus an occasional cross-layer integration test via MSW

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Core Principles

- **Facade** is this feature's `data-access` public API: business validation and orchestration, calling the Client. It is the only thing this feature's Signal Store method is allowed to call.
- **Client** is internal, never exported: DTO mapping (via `{feature}.mapper.ts`) plus the actual HTTP call through `libs/shared/http-core`. It is the single point where a raw `HttpErrorResponse` is caught and converted into a typed domain error, checking for a network-level failure (`status === 0`) before any status-code-specific handling.
- DTO ↔ domain model mapping is always a hand-written function in `{feature}.mapper.ts`, including any enrichment from data not present in the DTO itself.
- `OfflineTransportError` is a single, shared error type (defined once in `libs/shared/http-core`, not redefined per feature) so callers across every feature can catch it uniformly.
- `HttpTestingController` is used only inside `{feature}.client.spec.ts` — every other spec fakes the layer directly beneath it.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]

# Structure

## Project Structure

```
/libs/{feature}/data-access
  /src
    /lib
      [{feature}.facade.ts](./classes/class-feature-facade.skill.md)
      {feature}.facade.spec.ts
      [{feature}.client.ts](./classes/class-feature-client.skill.md)
      {feature}.client.spec.ts
      [{feature}.mapper.ts, {feature}.errors.ts](./classes/class-feature-mapper-and-errors.skill.md)
      {feature}.integration.spec.ts        <- only for genuine cross-layer scenarios
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.facade.ts` | Public API: business validation/orchestration, calls the Client. Exported from `index.ts`. | [[classes/class-feature-facade.skill.md\|class-feature-facade.skill]] |
| `{feature}.facade.spec.ts` | Vitest unit test faking the Client. | [[classes/class-feature-facade.skill.md\|class-feature-facade.skill]] |
| `{feature}.client.ts` | Internal: DTO mapping via the Mapper, calls `libs/shared/http-core`, catches `HttpErrorResponse` and throws a typed domain error or the shared `OfflineTransportError`. Never exported. | [[classes/class-feature-client.skill.md\|class-feature-client.skill]] |
| `{feature}.client.spec.ts` | Vitest unit test using `HttpTestingController` — the only place it is used. | [[classes/class-feature-client.skill.md\|class-feature-client.skill]] |
| `{feature}.mapper.ts` / `{feature}.errors.ts` | Internal: hand-written mapping functions and this feature's typed domain error hierarchy. | [[classes/class-feature-mapper-and-errors.skill.md\|class-feature-mapper-and-errors.skill]] |
| `{feature}.integration.spec.ts` | Reserved for the rare case that genuinely needs Store → Facade → Client wired together, using MSW at the network boundary. | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create|Testing/{feature}.integration.spec.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/common (HttpClient, transitively via shared/http-core) | matching the Angular major version in use | Underlying HTTP transport |
| msw | latest compatible | Network-boundary mocking, reserved for genuine cross-layer integration specs |
| vitest | matching workspace configuration | Unit/integration test runner |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{feature}.integration.spec.ts.create|Testing/{feature}.integration.spec.ts.create]]

## What Does NOT Belong Here

- UI components, feature-level state (Signal Store) — belong in this feature's own `libs/{feature}/feature`
- Direct `HttpClient` usage — every HTTP call goes through `libs/shared/http-core`'s base service
- `HttpTestingController` usage anywhere outside `{feature}.client.spec.ts`
- Any durable, persisted queue implementation — belongs in `libs/shared/offline-sync`; this lib only calls `MutationQueueService.enqueue(...)`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

## Allowed Dependencies

- `libs/shared/http-core` (tag: `type:util`, `scope:shared`)
- `libs/shared/offline-sync` (tag: `type:util`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Rules

## MUST
- A feature's `{feature}.client.ts` MUST NOT be exported from that feature's `index.ts` — only the Facade (and domain error types) is part of the public API.
- The Facade MUST be the only class in this project exported from `index.ts`, along with the feature's domain error types.
- Every test in `{feature}.client.spec.ts` MUST use `HttpTestingController` to assert the exact request; `httpTesting.verify()` MUST run in `afterEach`.
- A Facade test MUST fake its Client directly — it MUST NOT use `HttpTestingController` or MSW.
- MSW MUST be used only for `{feature}.integration.spec.ts`-style tests that deliberately span more than one architectural layer.
- Every Client method's error handling MUST check for a network-level failure (`HttpErrorResponse` with `status === 0`) before any status-code-specific handling, and throw the shared `OfflineTransportError` in that case.
- A Facade MUST explicitly opt an operation into queueing by catching `OfflineTransportError` and calling `MutationQueueService.enqueue`.

## MUST NOT
- A `type:data-access` project MUST only be imported by the `type:feature` project that shares its `scope`.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.
- A Facade MUST NOT enqueue an operation whose business validation already failed before the Client was ever called.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Anti-patterns

- **Putting DTO mapping or direct `HttpClient`/`http-core` calls inside the Facade**
  - Consequence: blurs the Facade/Client separation, making business logic and transport concerns hard to test independently
  - Instead: the Facade only calls the Client; all DTO/transport concerns stay inside the Client
- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation the Facade exists to enforce, and skips the queueing decision the Facade owns
  - Instead: the store always goes through the Facade
- **Using `HttpTestingController` inside a Facade spec "to save time faking the Client"**
  - Consequence: the same HTTP call ends up asserted in two different, potentially inconsistent ways
  - Instead: fake the Client directly in the Facade spec
- **Reaching for the integration-spec pattern as the default way to test a Facade**
  - Consequence: slower, more complex tests than necessary, and duplicated mocking risk
  - Instead: default to the narrower unit-test pattern; reserve integration specs for genuine cross-layer scenarios
- **Treating a `status === 0` failure the same as any other server error**
  - Consequence: nothing above the Client can reliably tell "we're offline, retryable later" apart from "the server actively rejected this"
  - Instead: always check for the network-level failure first and throw the shared `OfflineTransportError`
- **Enqueueing an operation whose business validation already failed**
  - Consequence: queues a command that will never succeed, wasting a replay attempt and confusing the user with a "pending" state
  - Instead: business validation always runs and fails before any queueing decision is considered

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Check list

- [ ] `index.ts` exports the Facade and domain error types only, never the Client or Mapper
- [ ] Every HTTP call in this project goes through `libs/shared/http-core`, not raw `HttpClient`
- [ ] `HttpTestingController` appears only in `{feature}.client.spec.ts`
- [ ] Every Facade method has a spec faking the Client
- [ ] Every Client method checks for a network-level failure before any status-code-specific handling
- [ ] Only operations explicitly marked queueable catch `OfflineTransportError` and enqueue

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/DataAccess/{feature}.client.ts.extend|DataAccess/{feature}.client.ts.extend]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]
