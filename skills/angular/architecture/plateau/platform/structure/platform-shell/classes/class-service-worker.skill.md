---
name: class-service-worker
description: Workbox-generated service worker for apps/platform-shell — all five content-type-specific caching strategies, including stale-while-revalidate for federated remote chunks now that RemoteRegistryService exists
domain: skill
type: template
plateau: platform
artifact_type: script
version: 20260711150000
tags:
  - skill/template/class
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]]"
---

> This plateau applies all five caching strategies defined by `solution-offline-first` — the fifth (stale-while-revalidate for federated remote chunks), deferred by [[skills/angular/architecture/plateau/offline-app/structure/platform-shell/classes/class-service-worker.skill.md|offline-app's version of this file]], is re-included here now that `RemoteRegistryService` (from `solution-platform-embeddability`) exists to source known remote origins from.

# Goal

- Implement content-type-specific caching strategies as concrete Workbox routing rules, so the application — including federated embeddable content — keeps working while the network is unreliable

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The service worker is generated via Workbox's programmatic build API (`workbox-build`), integrated as an Nx build step
- Route-matching order matters: the network-only rule for auth/mutations MUST take precedence over the API-reads rule
- Federated remote chunks are runtime-cached using the same origin list `RemoteRegistryService` resolves — read resilience for embeddable apps piggybacks on the same registry that loads them

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]

# Naming convention

| use case | file name pattern | file name |
| -------- | -------------------- | --------- |
| Service worker build script | sw-build.ts | sw-build.ts |
| Service worker runtime routing rules | sw-src.ts | sw-src.ts |

# Implementation

```typescript
// Skill: class-service-worker
// Plateau: platform
// Version: 20260711150000

// sw-build.ts (invoked as an Nx build step)
import { injectManifest } from 'workbox-build';

await injectManifest({
  swSrc: 'src/sw-src.ts',
  swDest: 'dist/apps/platform-shell/sw.js',
  globDirectory: 'dist/apps/platform-shell',
  globPatterns: ['**/*.{js,css,html}'],
});
```

```typescript
// sw-src.ts — all 5 rules
import { precacheAndRoute } from 'workbox-precaching';
import { registerRoute } from 'workbox-routing';
import { CacheFirst, StaleWhileRevalidate, NetworkOnly } from 'workbox-strategies';
import { ExpirationPlugin } from 'workbox-expiration';

// 1. App shell — precache, injected by injectManifest above
precacheAndRoute(self.__WB_MANIFEST);

// 2. Static design-system assets — cache-first
registerRoute(
  ({ request }) => ['image', 'font'].includes(request.destination),
  new CacheFirst({ cacheName: 'static-assets', plugins: [new ExpirationPlugin({ maxEntries: 200 })] }),
);

// 3. API GET reads — stale-while-revalidate
registerRoute(
  ({ url, request }) => url.pathname.startsWith('/api/') && request.method === 'GET',
  new StaleWhileRevalidate({ cacheName: 'api-reads' }),
);

// 4. Auth endpoints and all non-GET requests — network-only, never cached.
//    Registered before rule 5 so it takes precedence for any non-GET request.
registerRoute(
  ({ url, request }) => url.pathname.startsWith('/auth/') || request.method !== 'GET',
  new NetworkOnly(),
);

// 5. Federated remote chunks — stale-while-revalidate, sourced from RemoteRegistryService's
//    own runtime manifest (KNOWN_REMOTE_ORIGINS is populated from the same manifest the
//    RemoteRegistryService fetches, not hardcoded).
registerRoute(
  ({ url }) => KNOWN_REMOTE_ORIGINS.some(origin => url.origin === origin),
  new StaleWhileRevalidate({ cacheName: 'federated-remote-chunks' }),
);
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]

# Rules

## MUST
- Rule 4 (network-only for auth/mutations) MUST be registered so it takes precedence over rules 3 and 5 for any non-GET request.
- The app shell MUST be precached via `injectManifest`'s generated manifest.
- Static design-system assets MUST use cache-first with a bounded `ExpirationPlugin`.
- API GET reads MUST use stale-while-revalidate.
- Federated remote chunks MUST use stale-while-revalidate, with `KNOWN_REMOTE_ORIGINS` sourced from `RemoteRegistryService`'s manifest, never hardcoded.

## MUST NOT
- Auth endpoints and non-GET requests MUST NOT be cached under any strategy other than network-only.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Registering the API-reads or federated-remote rule before the auth/mutations (network-only) route**
  - Consequence: a mutation request could be incorrectly matched and cached
  - Instead: register the network-only rule before rules 3 and 5, or make their matchers explicitly exclude what rule 4 handles
- **Hardcoding `KNOWN_REMOTE_ORIGINS` instead of sourcing it from `RemoteRegistryService`'s manifest**
  - Consequence: a newly onboarded embeddable app's origin is never cached, defeating the "no platform rebuild needed" benefit of Dynamic Federation
  - Instead: derive the origin list from the same manifest the registry itself fetches
- **Configuring any caching strategy other than network-only for auth or mutation endpoints**
  - Consequence: a cached auth/mutation response is an active correctness and security bug
  - Instead: auth and all non-GET requests are always network-only

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Check list

- [ ] All five rules are registered in an order that guarantees auth/mutations always resolve to network-only
- [ ] Static assets are bounded by an `ExpirationPlugin`
- [ ] `KNOWN_REMOTE_ORIGINS` is derived from `RemoteRegistryService`'s manifest, not hardcoded

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Unittest TestCases

- [ ] WHEN a GET request to `/api/orders` is made offline with a cached response available THEN
  - [ ] the cached response is served immediately, and a background revalidation is attempted
- [ ] WHEN a POST request to `/api/orders` is made THEN
  - [ ] it always goes to the network, never served from or written to any cache
- [ ] WHEN a request for an image/font asset is made THEN
  - [ ] it is served cache-first, bounded by the expiration plugin
- [ ] WHEN a request to a known federated remote's origin is made offline with a cached chunk available THEN
  - [ ] the cached chunk is served immediately, and a background revalidation is attempted

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
