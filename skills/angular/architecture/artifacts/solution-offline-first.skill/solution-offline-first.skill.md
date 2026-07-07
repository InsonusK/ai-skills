---
name: offline-first
description: Workbox-based service worker with five content-type-specific caching strategies, an accurate connectivity signal (navigator.onLine + health-check), and an offline-error distinction in the Client layer — scoped to read resilience only (Сценарий A); write queueing is deferred to the future "Синхронизация offline-данных" solution
domain: skill
type: architecture
version: 1.0
tags:
  - skill/architecture/solution
  - angular
  - pwa
  - offline
  - service-worker
triggers:
  - Setting up or reviewing the service worker's caching configuration
  - Deciding how a new kind of request should be cached
  - Reviewing how the application detects and displays offline state
creates:
  - "apps/platform-shell/src/sw-src.ts (service worker)"
extends:
  - "apps/platform-shell (service worker registration, build)"
  - "libs/shared/state (new connectivity slice)"
  - "libs/{feature}/data-access (Client offline-error distinction)"
depends_on:
  - "[[../solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]"
  - "[[../solution-state-management.skill/solution-state-management.skill.md|State management]]"
  - "[[../solution-api-http-layer.skill/solution-api-http-layer.skill.md|API/HTTP-слой]]"
  - "[[../solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|Встраиваемость платформы]]"
adr:
  - "[[adr/service-worker-mechanism.md]]"
  - "[[adr/caching-strategy-per-content-type.md]]"
  - "[[adr/connectivity-detection.md]]"
---

# Goal

- Make the application shell load and show last-known data even when offline (Сценарий A: read resilience), without attempting to solve offline mutation handling in this solution
- Apply the right caching strategy per content type, rather than one compromise applied everywhere
- Give the application an accurate, single source of truth for connectivity, and a clean hook (`OfflineTransportError`) for the future write-queueing solution to build on

# Capabilities

- The app shell always loads, even with no network, via an atomically-updated precached bundle
- Feature data screens show last-known-good data instantly (stale-while-revalidate) instead of a blank/error state when offline
- Federated embeddable modules (per the "Встраиваемость платформы" solution) keep working from their last-cached version if their independent deployment is temporarily unreachable
- Auth and mutation requests are never served from or written to any cache, protecting the token-handling guarantees from the "Аутентификация" solution
- A clear, accurate offline indicator, backed by more than just `navigator.onLine`

# Core Principles

- This solution's scope is read resilience only ("Сценарий A"): the app shell always loads, and API reads fall back to last-known-good cached data when offline. It deliberately does not implement write queueing, retry, or conflict handling for mutations attempted while offline — that is the explicit scope of the future "Синхронизация offline-данных" solution, which this solution only prepares a hook for
- Five distinct caching strategies apply by content type: precache (app shell), cache-first (static assets), stale-while-revalidate (API reads and federated remote chunks), network-only (auth and all non-GET requests)
- Connectivity is judged by combining `navigator.onLine` events with a periodic backend health-check — neither alone is trustworthy on its own
- Every feature's Client distinguishes a network-level failure (`OfflineTransportError`) from a genuine server-side error — this is the one architectural hook this solution adds toward the future write-queueing solution

# Adr

- [[adr/service-worker-mechanism.md|Workbox instead of Angular's built-in (and now feature-frozen) Service Worker]]
  - Selected variant: Workbox — chosen because ngsw is explicitly feature-frozen by Angular's own team, and Workbox supports the per-content-type strategies and runtime caching of dynamically-resolved federation chunks this solution needs
- [[adr/caching-strategy-per-content-type.md|Five strategies by content type instead of one uniform strategy]]
  - Selected variant: five strategies — chosen because auth/mutations, the app shell, static assets, API reads, and federated remote chunks each have genuinely different freshness/availability requirements
- [[adr/connectivity-detection.md|navigator.onLine events + periodic health-check instead of navigator.onLine alone]]
  - Selected variant: combined signal — chosen because `navigator.onLine` alone materially misrepresents real backend reachability in common failure modes (captive portals, backend outages)

# Requirements

SOLUTION:
- [[../solution-state-management.skill/solution-state-management.skill.md|State management]]
  - New `connectivity` slice added to `libs/shared/state`, following the same classical-NgRx pattern as the existing `auth` slice
- [[../solution-api-http-layer.skill/solution-api-http-layer.skill.md|API/HTTP-слой]]
  - Every feature's `{feature}.client.ts` extended to throw `OfflineTransportError` on network-level failures
- [[../solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|Встраиваемость платформы]]
  - Federated remote chunks are runtime-cached using the same origin list as `RemoteRegistryService`

NPM:
- workbox-build, workbox-routing, workbox-strategies, workbox-precaching, workbox-expiration
  - Service worker generation and routing strategies, per [[adr/service-worker-mechanism.md]]

# Template Skill Mutations

REPOSITORY:
- [[./Implementation/Repository.extend.md|Repository]] - extend - Workbox build integration in `apps/platform-shell`, new `connectivity` slice, `OfflineTransportError` distinction in every feature's Client

Artifact-level:
- [[./Implementation/ServiceWorker/workbox-routes.create.md|workbox-routes (sw-src.ts)]] - create - the five content-type routing rules
- [[./Implementation/GlobalStore/connectivity.store.ts.create.md|connectivity.store.ts]] - create - `isOnline` signal combining browser events and health-check
- [[./Implementation/DataAccess/{feature}.client.ts.extend.md|{feature}.client.ts (extend)]] - extend - throws `OfflineTransportError` on network-level failure, generic pattern applied to any feature's Client
- [[./Implementation/UI/offline-banner.component.ts.create.md|offline-banner.component.ts]] - create - shared offline indicator, mounted once at the shell level

# Workflow

## Opening the app offline (happy path — Сценарий A)

1. The user opens the application with no network connection.
2. The precached app shell loads regardless — the browser serves it entirely from the service worker's cache.
3. A feature screen requests its data; the stale-while-revalidate strategy immediately serves the last-cached response, so the user sees last-known data rather than a blank screen.
4. `OfflineBannerComponent`, reading the `connectivity` slice, shows the offline indicator.

## Network restored while viewing cached data (happy path)

1. The health-check succeeds again; `isOnline` becomes `true`; the offline banner disappears.
2. The next time a stale-while-revalidate-cached endpoint is requested, the background revalidation fetches fresh data and updates the cache for next time.

## Attempting a mutation while offline (documented limitation of this solution)

1. The user attempts to create an order while offline.
2. The Client's HTTP call fails at the network level; per this solution's extension, it throws `OfflineTransportError` instead of a generic domain error.
3. The Facade/Signal Store surfaces this as a failure to the user (e.g. "you're offline, try again once reconnected") — no queueing or automatic retry happens in this solution.
4. The future "Синхронизация offline-данных" solution will catch this same `OfflineTransportError` to decide whether to enqueue the mutation instead of surfacing it as a failure.

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Client as OrdersClient
    participant SW as Service Worker
    User->>Client: addOrder() while offline
    activate Client
    Client->>SW: POST /orders (network-only route)
    activate SW
    SW-->>Client: network failure (no response)
    deactivate SW
    Client-->>User: throws OfflineTransportError
    deactivate Client
    Note over Client,User: This solution stops here — surfaced as a failure.<br/>The future sync solution will catch this error and queue instead.
```

## Federated remote temporarily unreachable (failure path)

1. An embeddable app's independently-deployed `remoteEntry` is temporarily unreachable (that team's own outage, unrelated to the user's connectivity).
2. The stale-while-revalidate runtime-caching rule serves the last-cached version of that remote instead of failing to load it entirely.
3. Once that team's deployment is reachable again, the next load revalidates and updates the cached version.

# Rules

## MUST
- [[./Implementation/Repository.extend.md#MUST|Repository.extend]]
- [[./Implementation/ServiceWorker/workbox-routes.create.md#MUST|ServiceWorker/workbox-routes.create]]
- [[./Implementation/GlobalStore/connectivity.store.ts.create.md#MUST|GlobalStore/connectivity.store.ts.create]]
- [[./Implementation/DataAccess/{feature}.client.ts.extend.md#MUST|DataAccess/{feature}.client.ts.extend]]
- [[./Implementation/UI/offline-banner.component.ts.create.md#MUST|UI/offline-banner.component.ts.create]]

## MUST NOT
- [[./Implementation/Repository.extend.md#MUST NOT|Repository.extend]]

# Anti-patterns

- [[./Implementation/Repository.extend.md|See Repository.extend.md]] — building even a minimal mutation queue as part of this solution; caching auth/mutation endpoints with anything other than network-only.
- [[./Implementation/ServiceWorker/workbox-routes.create.md|See workbox-routes.create.md]] — registering routes in an order that lets a mutation be matched by the API-reads rule.
- [[./Implementation/GlobalStore/connectivity.store.ts.create.md|See connectivity.store.ts.create.md]] — relying on the browser's online signal alone without the health-check.
- [[./Implementation/DataAccess/{feature}.client.ts.extend.md|See {feature}.client.ts.extend.md]] — treating a network-level failure the same as a server error.
- [[./Implementation/UI/offline-banner.component.ts.create.md|See offline-banner.component.ts.create.md]] — a feature building its own local offline indicator instead of the shared one.

# Check list

- [ ] The app shell loads fully with no network connection
- [ ] API GET requests show last-known cached data when offline, via stale-while-revalidate
- [ ] Auth and every non-GET request are always `network-only`, never cached
- [ ] Federated remote chunks are runtime-cached (stale-while-revalidate), not precached
- [ ] `isOnline` reflects both `navigator.onLine` and the health-check, not either alone
- [ ] Every feature's Client throws `OfflineTransportError` for network-level failures, distinct from server-side domain errors
- [ ] No mutation queueing, retry, or persistence exists in this solution — mutations attempted offline simply fail, deferred entirely to the future "Синхронизация offline-данных" solution
