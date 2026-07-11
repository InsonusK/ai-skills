---
name: plateau-offline-app
description: The online-monolith application extended with read resilience (service worker, connectivity awareness) and a real mutation write-queue with retry/conflict handling — the app keeps working, both reading and writing, while the network is unreliable
domain: skill
type: template
version: 20260711140000
tags:
  - skill/template/plateau
  - plateau/offline-app
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
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

> Previous plateau in the main application's chain: [[skills/angular/architecture/plateau/tested/plateau-tested.skill.md|tested]] (the "online-monolith" milestone: routing, forms, HTTP layer, auth, logging, testing — 10 solutions). This plateau adds `solution-offline-first` and `solution-offline-sync` on top, unchanged otherwise. Next: [[skills/angular/architecture/plateau/platform/plateau-platform.skill.md|platform]] (the final plateau, which re-includes the two Module-Federation-specific slices deferred here — see "Deferred to the platform plateau" in [[skills/angular/architecture/plateau/offline-app/structure/repo-offline-app.skill.md|repo-offline-app]] — and adds embeddability/design-system-application in full).

# Core Principles

- Every read stays available while the network is unreliable: a Workbox service worker precaches the app shell, serves static assets cache-first, and serves API GET reads stale-while-revalidate — auth and every mutation remain strictly network-only, never cached
- Connectivity is a first-class, accurate signal (`isOnline`, combining `navigator.onLine` with a periodic health check), not a per-feature guess
- A feature's Client distinguishes "we're offline" (`OfflineTransportError`) from "the server rejected this" — a network-level failure is never conflated with a domain error
- A Facade explicitly, per operation, opts a mutation into a durable, per-feature-partitioned write-queue instead of failing outright when offline
- Replay is automatic on connectivity restoration, FIFO within a feature, concurrent across features, with a server-wins conflict default and one clean seam for future custom resolution
- The user is never left guessing: an offline banner and a per-feature pending-sync count make both read-staleness and write-queueing visible

# Capabilities

- read resilience
  - app shell precached; static design-system assets cache-first; API GET reads stale-while-revalidate; auth/mutations always network-only
  - an accurate `isOnline` signal in `libs/shared/state`, available to every feature
- write resilience
  - a Dexie-backed, per-feature-partitioned mutation queue with a stable idempotency key per queued command
  - automatic replay on connectivity restoration; a stuck feature's replay never blocks another feature's
  - server-wins conflict handling by default, behind a single overridable seam
- UX feedback
  - a shell-level offline banner, backed by the shared `connectivity` slice
  - a per-feature pending-sync indicator reactively backed by the mutation queue
- everything the `tested` plateau already provides — Nx module boundaries, three-tier state, hierarchical routing with selective preloading, Signal Forms, Facade/Client/Mapper HTTP layering, in-memory-token auth with permission-based guards/directives, structured logging with a backend sink, and a layered Vitest/Playwright test strategy — unchanged

# Usecases

## Read a feature while offline

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant SW as Service Worker
    participant Component
    participant Store as {Feature}Store
    participant Facade as {Feature}Facade
    participant Client as {Feature}Client

    User->>Component: opens a previously visited feature page
    Component->>Store: load()
    Store->>Facade: fetch{Feature}()
    Facade->>Client: GET /api/{feature}
    Client->>SW: HTTP GET
    SW-->>Client: cached response (stale-while-revalidate), background revalidation attempted
    Client-->>Facade: mapped domain model
    Facade-->>Store: result
    Store-->>Component: updated state (signal), offline banner still visible
```

## Attempt a mutation while offline, then sync automatically

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Component
    participant Store as {Feature}Store
    participant Facade as {Feature}Facade
    participant Client as {Feature}Client
    participant Queue as MutationQueueService
    participant Orchestrator as ReplayOrchestrator
    participant Api as Backend

    User->>Component: submit create form (offline)
    Component->>Store: addOrder(payload)
    Store->>Facade: addOrder(payload)
    Facade->>Client: POST /orders
    Client-->>Facade: throws OfflineTransportError (status 0)
    Facade->>Queue: enqueue({ feature: 'orders', operationName: 'addOrder', payload, idempotencyKey })
    Facade-->>Store: { queued: true }
    Store-->>Component: pending state — pending-sync indicator shows 1

    Note over Orchestrator: connectivity restored — isOnline transitions to true
    Orchestrator->>Queue: pendingForFeatureOnce('orders')
    Orchestrator->>Facade: addOrder(payload, { idempotencyKey })
    Facade->>Client: POST /orders (same idempotency key)
    Client->>Api: HTTP request
    Api-->>Client: 201 Created
    Client-->>Facade: created Order
    Orchestrator->>Queue: markSynced(entry.id)
    Note over Component: pending-sync indicator returns to 0
```
