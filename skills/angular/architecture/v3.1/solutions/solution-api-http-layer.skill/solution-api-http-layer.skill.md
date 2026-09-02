---
name: solution-api-http-layer
description: Facade/Client layering inside each feature's data-access lib, a shared base HTTP service, typed domain errors, and hand-written DTO mapping — collapses the classical NgRx Action/Reducer/Effect chain for feature-level operations in favor of Signal Store calling the Facade directly
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - http
  - data-access
  - framework/angular
  - concern/architecture
  - solution/api-http-layer

whenToUse: when adding a data operation (fetch/create/update/delete) to a feature, deciding how a backend error should surface to a Signal Store or effect, or how a DTO field maps to the domain model
creates:
  - libs/shared/http-core
extends:
  - Repository (formalizes Facade/Client/Mapper structure inside feature data-access libs)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/facade-client-layering.md|Facade Client Layering ADR]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/error-handling-strategy.md|Error Handling Strategy ADR]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/dto-mapping-strategy.md|Dto Mapping Strategy ADR]]"
---

# Goal

- Give every feature a consistent internal structure for data operations: Facade (business logic, public API) → Client (transport/DTO mapping, internal) → shared base HTTP service
- Remove the redundant classical-NgRx orchestration (Action/Reducer/Effect) for feature-level operations, now that Signal Store already owns that role
- Give callers (Signal Store methods, NgRx effects for global state) a single, typed, predictable error shape instead of raw HTTP errors

# Capabilities

- Business validation (Facade) stays testable in isolation from transport/DTO concerns (Client)
- A raw `HttpErrorResponse` never reaches feature or business code — every caller works with typed domain errors
- Common HTTP concerns (base URL, timeout, retry) are defined once in `libs/shared/http-core`, not reimplemented per feature
- No duplicated orchestration mechanism for feature-level state — Signal Store is the only place that manages it

# Core Principles

- **Facade** is a feature's `data-access` public API: business validation and orchestration, calling the Client. It is the only thing a feature's Signal Store method (or, for global state, an NgRx effect) is allowed to call.
- **Client** is internal to a feature's `data-access` lib, never exported: DTO mapping (via that feature's `{feature}.mapper.ts`) plus the actual HTTP call through `libs/shared/http-core`. It is the single point where a raw `HttpErrorResponse` is caught and converted into a typed domain error.
- For feature-level operations, no Action/Reducer/Effect is introduced — the Signal Store method calls the Facade directly and updates its own state from the result. Global/cross-cutting state (auth, notifications, offline-sync) keeps its existing classical NgRx chain (Effect → Facade → Client), unchanged by this solution.
- DTO ↔ domain model mapping is always a hand-written function in `{feature}.mapper.ts`, including any enrichment from data not present in the DTO itself.
- When a feature's `data-access` lib owns several distinct data facets (for example, separate endpoints for orders, payments, and delivery), each facet keeps its own Facade/Client/Mapper trio. The files are grouped by role under `libs/{feature}/data-access/src/lib`:
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
  The `index.ts` exports every facet's Facade and its public error types; no Client or Mapper is exported. The same Facade/Client/Mapper rules apply to every facet.

# Adr

- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/facade-client-layering.md|Signal Store calls Facade directly; Client is an internal transport detail — Action/Reducer/Effect collapsed for feature-level operations]]
  - Selected variant: Signal Store → Facade → Client — chosen since Signal Store already owns the orchestration role, making the classical NgRx chain redundant for feature-level operations
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/error-handling-strategy.md|Client throws typed domain errors; Facade preserves the throw/reject channel]]
  - Selected variant: typed domain errors via throw/reject — chosen for compatibility with both existing call-site styles (`try/catch` and Promise rejection)
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/adr/dto-mapping-strategy.md|Manual mapper functions instead of an automatic mapping library]]
  - Selected variant: manual mappers — chosen because some fields require enrichment from external context that an automatic mapper would not handle cleanly anyway

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]
  - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create|libs/{feature}/data-access]] - internal structure formalized by this solution
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]]
  - Feature-level Signal Store methods call the Facade/Client exactly as already established there; global/cross-cutting effects keep calling into the same Facade/Client layering

NPM:
- `HttpClient`, already a base Angular dependency

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository]] - extend - add `libs/shared/http-core`, formalize the Facade/Client/Mapper/Errors structure inside every `data-access` project

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|libs/shared/http-core]] - create - base HTTP service (base URL, timeout, retry) shared by every feature's Client

Artifact-level (generic pattern, applied by any solution that creates a `libs/{feature}/data-access` project):
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|{feature}.facade.ts]] - create - public API, business validation/orchestration
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|{feature}.client.ts]] - create - internal transport/DTO layer
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|{feature}.mapper.ts / {feature}.errors.ts]] - create - hand-written DTO mapping and typed domain errors

# Workflow

## Feature-level create operation (happy path)

1. A component calls a Signal Store method (e.g. `OrdersStore.addOrder(input)`).
2. The store method calls `OrdersFacade.addOrder(input)` directly — no Action is dispatched, per the "State management" solution's tiering.
3. The Facade validates business-level input (e.g. quantity > 0); if they fail, it throws immediately without calling the Client.
4. Otherwise, the Facade calls `OrdersClient.addOrder(input)`, which maps the domain model to a DTO (enriching fields as needed via `{feature}.mapper.ts`), calls `libs/shared/http-core`, and maps the response back to a domain model.
5. The store method receives the result and calls `patchState` to update its own signals — no Reducer involved.

![Feature-level data operation](./diagrams/feature-level-data-operation.mmd)

## Transport failure mapped to a domain error (failure path)

1. The backend responds with a 409 conflict.
2. `OrdersClient` catches the `HttpErrorResponse` and throws `OrdersConflictError`, never letting the raw error escape.
3. `OrdersFacade` catches `OrdersConflictError` and re-wraps it into a more specific business error (`OrdersAlreadySubmittedError`) if that adds clarity, or lets it propagate as-is.
4. `OrdersStore`'s `try/catch` catches the typed error and sets its `error` signal — the UI never sees an HTTP status code directly.

## Global/cross-cutting operation — unchanged (steady state)

1. An auth operation (e.g. login) still goes through the classical NgRx chain established in "State management" and `solution-authentication`: an Effect calls `AuthFacade.login()`, which calls `AuthClient.login()` internally.
2. This solution does not change that chain — it only formalizes the Facade/Client split inside it and applies the same error-handling and mapping conventions.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend#MUST|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create#MUST|HttpCore/shared-http-core.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create#MUST|{feature}.facade.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create#MUST|{feature}.client.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create#MUST|{feature}.mapper-and-errors.ts.create]]

- [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend#MUST|Repository.extend]]
## SHOULD
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|See Repository.extend.md]] — a Signal Store method calling a feature's Client directly, skipping the Facade; a Client letting a raw `HttpErrorResponse` escape.
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|See shared-http-core.project.create.md]] — adding feature-specific special cases into the shared base HTTP service.
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|See {feature}.facade.ts.create.md]] — putting DTO mapping or direct HTTP calls inside the Facade instead of delegating to the Client.
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|See {feature}.client.ts.create.md]] — a component or Signal Store method importing the feature's Client directly instead of going through the Facade.
- Avoid — [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|See {feature}.mapper-and-errors.ts.create.md]] — a mapper silently dropping a DTO field with no domain equivalent and no explanatory comment.

# Check list

- [ ] Every DTO ↔ model conversion has a corresponding function in `{feature}.mapper.ts`
- [ ] Global/cross-cutting state (auth, notifications, offline-sync) still uses its existing classical NgRx chain, unchanged by this solution
- [ ] Every feature's `index.ts` exports the Facade and domain error types only, never the Client or Mapper
- [ ] When a feature has multiple data facets, each facet's files are grouped under `facade/`, `client/`, and `mapper/` with names `{feature}_N.{kind}.ts`
- [ ] No component or Signal Store method imports a feature's Client directly, bypassing the Facade