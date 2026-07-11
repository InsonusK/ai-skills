---
name: plateau-tested
description: The "online-monolith" milestone — a single deployable Nx application with structured state management, hierarchical routing with lazy loading, Signal Forms, a Facade/Client data-access layer, session-based authentication and permissions, structured logging with backend reporting, and enforced Vitest/Playwright test coverage. No offline support, no Module Federation host/embeddable-app split yet.
domain: skill
type: template
version: 20260711170000
tags:
  - skill/template/plateau
  - plateau/tested
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
  - "[[skills/angular/architecture/solutions/solution-logging-base.skill/solution-logging-base.skill|solution-logging-base]]"
  - "[[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill|solution-logging-global]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

> Sixth plateau in the main application's chain, and the **"online-monolith" milestone**. Previous: [[skills/angular/architecture/plateau/observable/plateau-observable.skill.md|observable]]. Next: [[skills/angular/architecture/plateau/offline-app/plateau-offline-app.skill.md|offline-app]]. This is also where the [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] plateau's npm package becomes a real, plain dependency of `apps/platform-shell` (see `platform-shell`'s own project skill) — federation-aware consumption is deferred to the future "platform" plateau, same as Module Federation itself.

# Core Principles

- Everything from [[skills/angular/architecture/plateau/observable/plateau-observable.skill.md|observable]] carries over unchanged: structured logging, session lifecycle, permission-based authorization, hierarchical routing, Signal Forms, and the Facade/Client-layered data-access pattern
- Every Nx project runs its unit/component tests via Vitest; end-to-end tests are Playwright specs in a dedicated `apps/platform-shell-e2e` project
- Every test fakes exactly the layer directly beneath the unit under test — `HttpTestingController` is used only inside a feature's Client spec, never above it
- CI enforces a minimum code-coverage threshold per project as a hard failure, not a warning

# Capabilities

- structure, state management, routing, forms, data access, authentication & authorization, logging & observability
  - Unchanged from [[skills/angular/architecture/plateau/observable/plateau-observable.skill.md|observable]]
- testing
  - Every component has a fast, DOM-accurate Testing Library spec, faking its Signal Store — no browser, no HTTP
  - Every feature's Client has exactly one place `HttpTestingController` verifies the real request shape and DTO mapping
  - Every feature's Facade and Signal Store are tested by faking the layer directly below them, never HTTP
  - A small number of critical, cross-cutting user journeys are covered end-to-end via Playwright against the real built application
  - Coverage regressions fail CI outright, surfacing a genuine drop in tested code immediately

# Usecases

## Optimistic create with full test coverage across every layer

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
    Store->>Facade: addOrder(input)
    Facade->>Facade: validate quantity > 0
    Facade->>Client: addOrder(input)
    Client->>Api: POST /orders
    activate Api
    Api-->>Client: 201 Created
    deactivate Api
    Client-->>Facade: mapped Order
    Facade-->>Store: mapped Order
    Store->>Store: append to orders(), loading=false
    Store-->>Component: updated state (signal)

    Note over Component,Client: Each arrow above has its own isolated spec:<br/>Component fakes Store, Store fakes Facade, Facade fakes Client,<br/>Client is the only place HttpTestingController is used.
```

## CI gate on a pull request

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant CI as CI pipeline
    participant Vitest as Vitest (unit/component)
    participant PW as Playwright (e2e)
    participant Cov as Coverage gate

    Dev->>CI: open PR
    CI->>Vitest: nx affected -t test
    Vitest-->>CI: pass/fail
    CI->>PW: run apps/platform-shell-e2e against a production-like build
    PW-->>CI: pass/fail
    CI->>Cov: check coverage threshold per affected project
    alt below threshold
        Cov-->>CI: fail build
    else at or above threshold
        Cov-->>CI: pass
    end
```
