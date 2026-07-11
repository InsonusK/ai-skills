---
name: class-service-worker
description: Workbox-generated service worker for apps/platform-shell — content-type-specific runtime caching strategies for read resilience
domain: skill
type: template
plateau: offline-monolith
artifact_type: script
version: 20260711200000
tags:
  - skill/template/class
  - plateau/offline-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
---

> This plateau applies all four caching strategies defined by `solution-offline-first`. A fifth rule — stale-while-revalidate for federated remote chunks — is added later, as its own extension, by [[skills/angular/architecture/plateau/plateau-platform-monolith.skill/plateau-platform-monolith.skill|platform-monolith]], where `solution-platform-embeddability` is applied.

# Goal

- Implement content-type-specific caching strategies as concrete Workbox routing rules, so the application keeps working for reads while the network is unreliable

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The service worker is generated via Workbox's programmatic build API (`workbox-build`), integrated as an Nx build step — not a webpack plugin, and not `@angular/service-worker` (ngsw)
- Route-matching order matters: the network-only rule for auth/mutations MUST take precedence over the API-reads rule

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Naming convention

| use case | file name pattern | file name |
| -------- | -------------------- | --------- |
| Service worker build script | sw-build.ts | sw-build.ts |
| Service worker runtime routing rules | sw-src.ts | sw-src.ts |

# Implementation

```typescript
// Skill: class-service-worker
// Plateau: offline-monolith
// Version: 20260711200000

// sw-build.ts (invoked as an Nx build step)
import { injectManifest } from 'workbox-build';

await injectManifest({
  swSrc: 'src/sw-src.ts',
  swDest: 'dist/apps/platform-shell/sw.js',
  globDirectory: 'dist/apps/platform-shell',
  globPatterns: ['**/*.{js,css,html}'], // app shell + lazy feature chunks -> precache
});
```

```typescript
// sw-src.ts — runtime routing rules (4 of the 5 defined by solution-offline-first)
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
//    MUST be registered so it takes precedence over rule 3 for any non-GET request.
registerRoute(
  ({ url, request }) => url.pathname.startsWith('/auth/') || request.method !== 'GET',
  new NetworkOnly(),
);

// Rule 5 (stale-while-revalidate for federated remote chunks) is NOT registered at this
// plateau — see "Deferred capability" below.
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Rules

## MUST
- Rule 4 (network-only for auth/mutations) MUST be registered so it takes precedence over rule 3 for any non-GET request, even one under `/api/`.
- The app shell MUST be precached via `injectManifest`'s generated manifest, not hand-listed.
- Static design-system assets (images, fonts) MUST use cache-first with a bounded `ExpirationPlugin`.
- API GET reads MUST use stale-while-revalidate — served from cache immediately, revalidated in the background.

## MUST NOT
- Auth endpoints and non-GET requests MUST NOT be cached under any strategy other than network-only.
- This plateau MUST NOT register a caching rule for federated remote origins yet — see the deferred capability below.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Registering the API-reads (stale-while-revalidate) route before the auth/mutations (network-only) route**
  - Consequence: depending on Workbox's route-matching order, a mutation request could be incorrectly matched and cached
  - Instead: register the network-only rule first, or make its matcher explicitly exclude what rule 3 handles
- **Configuring any caching strategy other than network-only for auth or mutation endpoints**
  - Consequence: a cached auth/mutation response is an active correctness and security bug
  - Instead: auth and all non-GET requests are always network-only

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Check list

- [ ] The four rules applied at this plateau are registered in an order that guarantees auth/mutations always resolve to network-only
- [ ] Static assets are bounded by an `ExpirationPlugin`, not left to grow unbounded

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Unittest TestCases

- [ ] WHEN a GET request to `/api/orders` is made offline with a cached response available THEN
  - [ ] the cached response is served immediately, and a background revalidation is attempted
- [ ] WHEN a POST request to `/api/orders` is made THEN
  - [ ] it always goes to the network, never served from or written to any cache
- [ ] WHEN a request for an image/font asset is made THEN
  - [ ] it is served cache-first, bounded by the expiration plugin

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]

# Extended later

A fifth caching rule — stale-while-revalidate for requests to a federated remote's origin — is not part of `solution-offline-first` and does not exist at this plateau. There are no embeddable remotes to cache yet. It is added as its own extension by `solution-platform-embeddability`, applied at [[skills/angular/architecture/plateau/plateau-platform-monolith.skill/plateau-platform-monolith.skill|platform-monolith]], sourcing `KNOWN_REMOTE_ORIGINS` from `RemoteRegistryService`'s own runtime configuration.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
