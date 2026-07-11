---
name: plateau-data-capable
description: Navigable app gains real data operations — Signal Forms for input, and a Facade/Client-layered data-access lib per feature on top of a shared HTTP core, with typed domain errors and hand-written DTO mapping.
domain: skill
type: template
version: 20260711140000
tags:
  - skill/template/plateau
  - plateau/data-capable
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
---

> Third plateau in the main application's chain. Previous: [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]]. Next: [[skills/angular/architecture/plateau/authenticated/plateau-authenticated.skill.md|authenticated]].

# Core Principles

- Everything from [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]] carries over unchanged: hierarchical route ownership, selective preloading, and the three-tier state placement rule
- New forms are built with Signal Forms (`form()`/`FieldTree`) by default; existing Reactive Forms are never force-migrated, only touched opportunistically
- A form's submission always goes through `submitForm()`, whose callback calls into the owning feature's data-access Facade — never `HttpClient` directly from a component
- Every feature's `data-access` lib is internally layered: **Facade** (public API, business validation) → **Client** (internal transport + DTO mapping, never exported) → shared `libs/shared/http-core` base service
- A raw `HttpErrorResponse` never escapes a feature's Client — every caller works with that feature's own typed domain errors
- For feature-scoped operations the Signal Store method calls the Facade directly; no Action/Reducer/Effect is introduced. Global/cross-cutting state keeps its existing classical NgRx chain, unchanged

# Capabilities

- structure
  - `nx affected` runs CI tasks only for projects impacted by a change; enforced module boundaries via Nx tags
- state management
  - Component Signal → feature Signal Store → global NgRx Store tiering, unchanged from foundation
- routing
  - Hierarchical, root-relative route ownership; selective preloading; `loadComponent` sub-splitting — unchanged from navigable
- forms
  - Fine-grained, synchronous field-level validity/touched/error state with no manual `valueChanges` subscriptions
  - A single submission pattern (`submitForm()`) reporting success/failure without hand-rolled pre-submit validity checks
  - Non-trivial field schemas are extractable into their own `{form-name}.form.ts` once cross-field logic grows
- data access
  - Business validation (Facade) stays testable in isolation from transport/DTO concerns (Client)
  - Common HTTP concerns (base URL, timeout, retry) live once in `libs/shared/http-core`, not reimplemented per feature
  - Every DTO ↔ domain model field conversion is an explicit, unit-testable mapper function

# Usecases

## Fill in and submit a feature form

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Form as {FormName}FormComponent
    participant Store as {Feature}Store
    participant Facade as {Feature}Facade
    participant Client as {Feature}Client
    participant Http as shared/http-core
    participant Api as Backend

    User->>Form: fill fields
    Form->>Form: form() tracks field-level valid()/touched()/errors() signals
    User->>Form: submit
    Form->>Form: submitForm(form, callback)
    Form->>Store: callback invokes store method with form value
    Store->>Facade: call facade method directly (no Action/Reducer/Effect)
    Facade->>Facade: business-rule validation
    Facade->>Client: call client method
    Client->>Client: modelToDto (mapper)
    Client->>Http: HTTP call via base-http.service
    Http->>Api: HTTP request
    activate Api
    Api-->>Http: response or HttpErrorResponse
    deactivate Api
    Http-->>Client: response
    Client->>Client: dtoToModel, or catch and throw typed domain error
    Client-->>Facade: domain model or typed error
    Facade-->>Store: result or re-wrapped typed error
    Store->>Store: patchState with result or error
    Store-->>Form: updated state (signal)
```

## A transport failure becomes a typed domain error

```mermaid
sequenceDiagram
    autonumber
    participant Store as {Feature}Store
    participant Facade as {Feature}Facade
    participant Client as {Feature}Client
    participant Api as Backend

    Store->>Facade: addOrder(input)
    Facade->>Client: addOrder(input)
    Client->>Api: POST /orders
    activate Api
    Api-->>Client: 409 Conflict (HttpErrorResponse)
    deactivate Api
    Client->>Client: catch HttpErrorResponse, throw OrdersConflictError
    Client-->>Facade: throws OrdersConflictError
    Facade->>Facade: re-wrap as OrdersAlreadySubmittedError (business context)
    Facade-->>Store: throws OrdersAlreadySubmittedError
    Store->>Store: catch, patchState({ error })
    Note over Store,Api: the UI never sees a raw HTTP status code
```
