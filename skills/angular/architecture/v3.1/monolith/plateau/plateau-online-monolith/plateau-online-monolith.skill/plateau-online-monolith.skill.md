---
name: plateau-online-monolith
description: The base connected Angular application of the monolith catalog — an Nx workspace with hierarchical routing, the state-tiering rule plus a classical NgRx global store, Signal Forms, a Facade/Client HTTP data-access layer with optimistic Signal-Store orchestration, console logging, and enforced Vitest/Playwright coverage across the business layer and the UI layer. One deployable unit, online-only: no offline resilience, no federation, no authentication.
domain: skill
type: template
whenToUse: when scaffolding a new feature (feature + data-access lib), wiring routing/state/forms/data-access, or reviewing whether a change follows the online-monolith conventions
version: 20260902000000
tags:
  - skill/template/plateau
  - plateau/online-monolith
  - stack/typescript
  - framework/angular
  - concern/architecture
parent_plateaus: []
standalone: true
created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill.md|solution-api-http-layer]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"
---

> **First plateau of the `monolith` catalog** — built from scratch, `standalone: true`. It fixes the [monolith Variability Map](skills/angular/architecture/v3.1/monolith/variability-map.md) at **VP2 (GlobalStore) = Yes, VP3 (BackendDataAccess) = Yes**; VP1, VP4–VP8 = No. Next in the chain: `plateau-perf-routing-monolith` (VP1). This is "everything the app needs to run online, end to end, as one deployable unit". No lazy-loading tuning, no offline resilience, no federation, and — deliberately — no authentication (every user implicitly trusted until `plateau-multiuser-monolith`).

# Core Principles

- `apps/` are deployable units, `libs/` are reusable code — nothing else at top level. Every feature is a `feature` lib (components + `{feature}.routes.ts` + a feature Signal Store) plus a `data-access` lib (Facade/Client/Mapper). Every lib exposes only its `index.ts`. Boundaries are enforced by `@nx/enforce-module-boundaries`.
- State lives at the smallest tier that satisfies its real consumers: component `signal()` → feature NgRx Signal Store → the classical NgRx global store in `libs/shared/state`. Promoted upward only when a second unrelated consumer needs it.
- The classical NgRx global store (`libs/shared/state`) exists and is wired (`store.config.ts` root registration), but ships **empty** — no concrete slice yet. Slices arrive with their features later in the chain (`connectivity`, `notifications`, `auth`).
- Routes are owned hierarchically: `apps/platform-shell` mounts first-level feature root segments only (lazy `loadChildren`); a feature knows only paths relative to its own root.
- New forms are built with Signal Forms; `submitForm()`'s callback calls the owning feature's data-access Facade — never `HttpClient` directly.
- Each feature's `data-access` is layered Facade (public API, business validation) → Client (internal transport + DTO mapping) → `libs/shared/http-core`. A raw `HttpErrorResponse` never escapes a Client. For feature-level operations the Signal Store calls the Facade directly — no Action/Reducer/Effect — which is what makes an optimistic `"creating…"` → `"created"` transition a `patchState` around a Facade call.
- Everything logs through `LoggerService` (`libs/shared/logging`), forwarding only to `ConsoleLogSink`; no direct `console.*` anywhere else.
- Every Nx project runs unit tests via Vitest; `HttpTestingController` only inside a Client spec. Every UI component gets a behavioral (Testing Library) + visual (Playwright, against `apps/component-preview`) + accessibility (`@axe-core/playwright`) + style-snapshot spec. Coverage and bundle budgets are enforced as CI errors.

# Capabilities

- `nx affected` runs CI only for impacted projects; module boundaries checked by lint.
- No NgRx boilerplate for purely local UI state; feature state stays encapsulated; a single auditable global store, wired but empty until a feature needs it.
- Any feature can be mounted, remounted, or moved without touching its own code — it never assumes its URL prefix.
- Fine-grained, synchronous field-level form validity/touched/error state with no manual subscriptions.
- A component action reflects immediately as an optimistic status in the UI via the Signal Store, then resolves once the Facade/Client round trip completes.
- Common HTTP concerns (base URL, timeout, retry) live once in `libs/shared/http-core`; every DTO↔model conversion is an explicit, unit-testable mapper.
- A single structured logging entry point, ready for a future backend sink with zero call-site rewrites.
- Every component has a fast DOM-accurate behavioral spec plus a Playwright screenshot-regression spec, an axe accessibility scan, and a computed-style snapshot.

# Structure

See [`structure/`](structure/plateau-online-monolith--repo-online-monolith.skill.md) — one repo skill, the project skills (`apps/platform-shell`, `apps/platform-shell-e2e`, `apps/component-preview`, `libs/shared/{ui,util,state,http-core,logging}`, `libs/{feature}/{feature,data-access}`), and the class/artifact skills under each.

# Example

See [`example/`](plateau-online-monolith.skill/example/) — a runnable Nx workspace with one feature (`orders`) end to end: a routed feature lib with a Signal Forms create form + feature Signal Store, an `orders` data-access lib (Facade/Client/Mapper/errors), `libs/shared/http-core`, `libs/shared/logging`, the empty-but-wired `libs/shared/state`, and the full four-layer test suite. `npm test` (Vitest) and `npm run e2e` (Playwright) green.

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
    User->>Component: submit order form (Signal Forms)
    Component->>Store: addOrder(input)
    Store->>Store: patchState({ status: 'creating' })
    Store->>Facade: addOrder(input)
    Facade->>Facade: validate quantity > 0
    Facade->>Client: addOrder(input)
    Client->>Api: POST /orders
    Api-->>Client: 201 Created
    Client-->>Facade: mapped Order
    Facade-->>Store: mapped Order
    Store->>Store: patchState({ status: 'created', order })
    Store-->>Component: updated signal — UI reflects "created"
```

## Transport failure mapped to a domain error

```mermaid
sequenceDiagram
    autonumber
    participant Client as OrdersClient
    participant Facade as OrdersFacade
    participant Store as OrdersStore
    Client->>Client: catch HttpErrorResponse (409)
    Client-->>Facade: throw OrdersConflictError (typed)
    Facade-->>Store: propagate typed error
    Store->>Store: patchState({ status: 'error', error })
    Note over Client,Store: the UI never sees an HTTP status code
```
