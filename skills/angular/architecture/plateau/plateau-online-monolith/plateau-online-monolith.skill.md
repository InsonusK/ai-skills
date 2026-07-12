---
name: plateau-online-monolith
description: The base online application — Nx workspace structure, three-tier state management, hierarchical routing, Signal Forms, a Facade/Client HTTP data-access layer with optimistic-update-friendly Signal Store orchestration, console-only logging, and enforced Vitest/Playwright test coverage. Single deployable unit, no offline support, no Module Federation, no authentication yet.
domain: skill
type: template
version: 20260711180000
tags:
  - skill/template/plateau
  - plateau/online-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill.md|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill.md|solution-testing]]"
parent_plateau: "[[skills/angular/architecture/plateau/plateau-design-system/plateau-design-system.skill.md|plateau-design-system.skill]]"
---

> First plateau in the main application's chain (no parent — built from scratch). Next: [[skills/angular/architecture/plateau/plateau-async-monolith/plateau-async-monolith.skill.md|async-monolith]]. This is the **"online-monolith"** milestone: everything the application needs to run online, end to end, as a single deployable unit — structure, state, navigation, forms, data flow with optimistic updates, console logging, and a real test suite. No lazy-loading yet, no offline resilience, no platform/embeddability, no backend log delivery, and — deliberately — no authentication: every user is implicitly trusted until [[skills/angular/architecture/plateau/plateau-multiuser-app/plateau-multiuser-app.skill.md|multiuser-app]], the final plateau in this chain. The [[skills/angular/architecture/plateau/plateau-design-system/plateau-design-system.skill.md|design-system]] npm package is already a real dependency of `apps/platform-shell`.

# Core Principles

- `apps/` are deployable units, `libs/` are reusable code — nothing else lives at top level
- State lives at the smallest tier that satisfies its real consumers: component Signal → feature Signal Store → global NgRx Store — promoted upward only when a second, unrelated consumer genuinely needs it
- Routes are owned hierarchically: the shell only knows first-level root segments; a feature only knows paths relative to its own root
- New forms are built with Signal Forms by default; submission always goes through `submitForm()`, whose callback calls into the owning feature's data-access Facade
- Every feature's `data-access` lib is layered Facade (public API, business validation) → Client (internal transport + DTO mapping) → shared `libs/shared/http-core`; a raw `HttpErrorResponse` never escapes a Client
- For feature-scoped operations, the Signal Store calls the Facade directly — no Action/Reducer/Effect — which is what makes an optimistic "creating…" → "created" status transition a simple `patchState` around a Facade call, not an NgRx round trip
- Everything logs through `LoggerService`, currently forwarding only to the console — no direct `console.*` call anywhere else
- Every Nx project runs unit/component tests via Vitest; `HttpTestingController` is used only inside a feature's own Client spec

# Capabilities


- structure
  - `nx affected` runs CI tasks only for projects impacted by a change; enforced module boundaries via Nx tags
- state management
  - No NgRx boilerplate for purely local UI state; feature state stays encapsulated; a single auditable global store skeleton (`libs/shared/state`)
- routing
  - Any feature can be mounted, remounted, or moved without changing its own code — it never knows its own URL prefix
- forms
  - Fine-grained, synchronous field-level validity/touched/error state with no manual `valueChanges` subscriptions
- data access
  - A component action (e.g. "create order") reflects immediately as a pending/optimistic status in the UI via the Signal Store, then resolves to a final status once the Facade/Client round trip completes
  - Common HTTP concerns (base URL, timeout, retry) live once in `libs/shared/http-core`
  - Every DTO ↔ domain model field conversion is an explicit, unit-testable mapper function
- observability
  - A single, structured logging entry point, ready for a future backend sink to be added with zero call-site rewrites
- testing
  - Every component has a fast, DOM-accurate Testing Library spec, faking its Signal Store — no browser, no HTTP
  - Every feature's Client has exactly one place `HttpTestingController` verifies the real request shape and DTO mapping
  - A small number of critical, cross-cutting user journeys are covered end-to-end via Playwright

# Usecases

## Optimistic create — status flips from "creating" to "created"

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Component as OrderFormComponent
    participant Store as OrdersStore
    participant Facade as OrdersFacade
    participant Client as OrdersClient
    participant Api as Backend

    User->>Component: submit order form
    Component->>Store: addOrder(input)
    Store->>Store: patchState({ status: 'creating' })
    Store->>Facade: addOrder(input)
    Facade->>Facade: validate quantity > 0
    Facade->>Client: addOrder(input)
    Client->>Api: POST /orders
    activate Api
    Api-->>Client: 201 Created
    deactivate Api
    Client-->>Facade: mapped Order
    Facade-->>Store: mapped Order
    Store->>Store: patchState({ status: 'created', order })
    Store-->>Component: updated state (signal) — UI reflects "created"
```

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
    Facade->>Client: call client method
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
