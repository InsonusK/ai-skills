---
name: solution-api-http-layer
description: Facade/Client layering inside each feature's data-access lib, a shared base HTTP service, typed domain errors, and hand-written DTO mapping — collapses the classical NgRx Action/Reducer/Effect chain for feature-level operations in favor of Signal Store calling the Facade directly
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - angular
  - http
  - data-access
triggers:
  - Adding a new data operation (fetch/create/update/delete) to a feature
  - Reviewing how errors from the backend should be surfaced to a Signal Store or NgRx effect
  - Deciding how a DTO field should be mapped to the domain model
creates:
  - libs/shared/http-core
extends:
  - Repository (formalizes Facade/Client/Mapper structure inside feature data-access libs)
depends_on:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|Структура репозитория (база)]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|State management]]"
adr:
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/adr/facade-client-layering|Facade Client Layering ADR]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/adr/error-handling-strategy|Error Handling Strategy ADR]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/adr/dto-mapping-strategy|Dto Mapping Strategy ADR]]"
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

# Adr

- [[adr/facade-client-layering.md|Signal Store calls Facade directly; Client is an internal transport detail — Action/Reducer/Effect collapsed for feature-level operations]]
  - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/adr/facade-client-layering|Signal Store calls Facade directly; Client is an internal transport detail — Action/Reducer/Effect collapsed for feature-level operations]]hrows typed domain errors; Facade preserves the throw/reject channel]]
  - Selected variant: typed domain errors via throw/reject — chosen for compatibility with both existing call-site styles (`try/c[[skills/angular/architecture/solutions/solution-api-http-layer.skill/adr/error-handling-strategy|Client throws typed domain errors; Facade preserves the throw/reject channel]]mapping library]]
  - Selected variant: manual mappers — chosen because some fields require enrichment from external context that an automatic mapper would not handle cleanly anyway

# Requirements

SOLUTION:
- [[../solutio[[skills/angular/architecture/solutions/solution-api-http-layer.skill/adr/dto-mapping-strategy|Manual mapper functions instead of an automatic mapping library]]Repository.create.md|libs/{feature}/data-access]] - internal structure formalized by this solution
- [[../solution-state-management.skill/solution-state-management.skill.md|State management]]
  - Fe[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|Структура репозитория (база)]]that s[[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|libs/{feature}/data-access]] Client exactly as already established there and in t[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|State management]]HttpClient`, already a base Angular dependency

# Template Skill Mutations

REPOSITORY:
- [[./Implementation/Repository.extend.md|Repository]] - extend - add `libs/shared/http-core`, formalize the Facade/Client/Mapper/Errors structure inside every `data-access` project
PROJECT:
- [[./Implementation/HttpCore/shared-http-core.project.create.md|libs/shared/http-core]] - create - base HTTP service (base URL, timeout, retry) shared by every feature's Client

[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository]]ution that creates a `libs/{feature}/data-access` project):
- [[./Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.cre[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|libs/shared/http-core]]
- [[./Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md|{feature}.client.ts]] - create - internal transport/DTO layer
- [[./Implementation/DataAccess/{Feature}.project.create/[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|{feature}.facade.ts]]en DTO mapping and typed domain errors

# Workflow

## Fe[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create|{feature}.client.ts]]nput)`), per the "State management" solution[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create|{feature}.mapper.ts / {feature}.errors.ts]] they fail, it throws immediately without calling the Client.
4. Otherwise, the Facade calls `OrdersClient.addOrder(input)`, which maps the domain model to a DTO (enriching fields as needed via `{feature}.mapper.ts`), calls `libs/shared/http-core`, and maps the response back to a domain model.
5. The store method receives the result and calls `patchState` to update its own signals — no Reducer involved.

![Feature-level data operation](./diagrams/feature-level-data-operation.mmd)

## Transport failure mapped to a domain error (failure path)

1. The backend responds with a 409 conflict.
2. `OrdersClient` catches the `HttpErrorResponse` and throws `OrdersConflictError`, never letting the raw error escape.
3. `OrdersFacade` catches `OrdersConflictError` and re-wraps it into a more specific business error (`OrdersAlreadySubmittedError`) if that adds clarity, or lets it propagate as-is.
4. `OrdersStore`'s `try/catch` catches the typed error and sets its `error` signal — the UI never sees an HTTP status code directly.

## Global/cross-cutting operation — unchanged (steady state)

1. An auth operation (e.g. login) still goes through the classical NgRx chain established in "State management" and "Аутентификация": an Effect calls `AuthFacade.login()`, which calls `AuthClient.login()` internally.
2. This solution does not change that chain — it only formalizes the Facade/Client split inside it and applies the same error-handling and mapping conventions.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository.extend]]
- [[./Implementation/HttpCore/shared-http-core.project.create.md#MUST|HttpCore/shared-http-core.project.create]]
- [[./Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create.md#MUST|{feature}.facade.ts.create]]
- [[./Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md#MUST|{feature}.client.ts.create]]
- [[./Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create.md#MUST|{feature}.mapper-and-errors.ts.create]]

## MUST NOT
- [[./Implementation/Repository.extend.md#MUST NOT|Repository.extend]]

# Anti-patterns

- [[./Implementation/Repository.extend.md|See Repository.extend.md]] — a Signal Store method calling a feature's Client directly, skipping the Facade; a Client letting a raw `HttpErrorResponse` escape.
- [[./Implementation/HttpCore/shared-http-core.project.create.md|See shared-http-core.project.create.md]] — adding feature-specific special cases into the shared base HTTP service.
- [[./Implementation/DataAccess/{Feature}.project.create/{feature}.facade[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend#MUST|Repository]]mapp[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create#MUST|HttpCore/shared-http-core.project.create]]}.cl[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create#MUST|{feature}.facade.ts]]appe[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create#MUST|{feature}.client.ts]]etho[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.mapper-and-errors.ts.create#MUST|{feature}.mapper-and-errors.ts]]Client` directly
-[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend#MUST NOT|Repository]]Client
- [ ] Every DTO ↔[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|See Repository.extend.md]]functions
- [ ] Global/cross-cutting state (auth, notifications, offline-sync) still uses its existing classical NgRx chain, unchanged b[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/HttpCore/shared-http-core.project.create|See shared-http-core.project.create.md]]aAccess/{Feature}.project.create/{feature}.client.ts.create|See {feature}.clien[[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create|See {feature}.facade.ts.create.md]]