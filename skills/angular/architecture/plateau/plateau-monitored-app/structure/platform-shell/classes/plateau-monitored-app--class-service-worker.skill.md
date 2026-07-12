---
name: plateau-monitored-app--class-service-worker
description: Workbox-generated service worker for apps/platform-shell — content-type-specific runtime caching strategies for read resilience, now including stale-while-revalidate for federated remote chunks — monitored-app plateau
domain: skill
type: template
plateau: monitored-app
artifact_type: script
version: 20260711220000
tags:
  - skill/template/class
  - plateau/monitored-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]]"
---

> This plateau applies all four caching strategies defined by `solution-offline-first`, plus a fifth rule from `solution-platform-embeddability` for federated remote chunks, now that `RemoteRegistryService` exists.

# Goal

- Implement content-type-specific caching strategies as concrete Workbox routing rules, so the application keeps working for reads while the network is unreliable, including reads from an embedded remote's own chunks

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/ServiceWorker/service-worker.ts.extend|ServiceWorker/service-worker.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The service worker is generated via Workbox's programmatic build API (`workbox-build`), integrated as an Nx build step — not a webpack plugin, and not `@angular/service-worker` (ngsw)
- Route-matching order matters: the network-only rule for auth/mutations MUST take precedence over both read rules
- `KNOWN_REMOTE_ORIGINS` is sourced from the same runtime manifest `RemoteRegistryService` resolves, never hardcoded separately

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/ServiceWorker/service-worker.ts.extend|ServiceWorker/service-worker.ts.extend]]

# Naming convention

| use case | file name pattern | file name |
| -------- | -------------------- | --------- |
| Service worker build script | sw-build.ts | sw-build.ts |
| Service worker runtime routing rules | sw-src.ts | sw-src.ts |

# Implementation

```typescript
// Skill: class-service-worker
// Plateau: monitored-app
// Version: 20260711220000

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
// sw-src.ts — runtime routing rules (all five)
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
//    MUST be registered so it takes precedence over rules 3 and 5 for any non-GET request.
registerRoute(
  ({ url, request }) => url.pathname.startsWith('/auth/') || request.method !== 'GET',
  new NetworkOnly(),
);

// 5. Federated remote chunks — stale-while-revalidate (URLs unknown at build time,
//    sourced from the same manifest RemoteRegistryService resolves)
registerRoute(
  ({ url }) => KNOWN_REMOTE_ORIGINS.some(origin => url.origin === origin),
  new StaleWhileRevalidate({ cacheName: 'federation-remotes' }),
);
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/ServiceWorker/service-worker.ts.extend|ServiceWorker/service-worker.ts.extend]]

# Rules

## MUST
- Rule 4 (network-only for auth/mutations) MUST be registered so it takes precedence over rules 3 and 5 for any non-GET request, even one under `/api/` or a remote's origin.
- The app shell MUST be precached via `injectManifest`'s generated manifest, not hand-listed.
- Static design-system assets (images, fonts) MUST use cache-first with a bounded `ExpirationPlugin`.
- API GET reads MUST use stale-while-revalidate — served from cache immediately, revalidated in the background.
- `KNOWN_REMOTE_ORIGINS` MUST be sourced from the same runtime remote registry configuration `RemoteRegistryService` uses, not hardcoded separately.
- Rule 5 MUST be registered after rule 4, so route-matching order still guarantees auth/mutations never resolve to it.

## MUST NOT
- Auth endpoints and non-GET requests MUST NOT be cached under any strategy other than network-only.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/ServiceWorker/service-worker.ts.extend|ServiceWorker/service-worker.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Registering the API-reads (stale-while-revalidate) route before the auth/mutations (network-only) route**
  - Consequence: depending on Workbox's route-matching order, a mutation request could be incorrectly matched and cached
  - Instead: register the network-only rule first, or make its matcher explicitly exclude what rules 3 and 5 handle
- **Configuring any caching strategy other than network-only for auth or mutation endpoints**
  - Consequence: a cached auth/mutation response is an active correctness and security bug
  - Instead: auth and all non-GET requests are always network-only
- **Hardcoding `KNOWN_REMOTE_ORIGINS` as a separate static list instead of deriving it from `RemoteRegistryService`'s own manifest**
  - Consequence: a newly onboarded embeddable app's origin is never cached, and the two lists silently drift apart over time
  - Instead: derive `KNOWN_REMOTE_ORIGINS` from the same manifest `RemoteRegistryService` fetches

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/ServiceWorker/service-worker.ts.extend|ServiceWorker/service-worker.ts.extend]]

# Check list

- [ ] The five rules are registered in an order that guarantees auth/mutations always resolve to network-only
- [ ] Static assets are bounded by an `ExpirationPlugin`, not left to grow unbounded
- [ ] `KNOWN_REMOTE_ORIGINS` is derived from the same configuration source as `RemoteRegistryService`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/ServiceWorker/service-worker.ts.extend|ServiceWorker/service-worker.ts.extend]]

# Unittest TestCases

- [ ] WHEN a GET request to `/api/orders` is made offline with a cached response available THEN
  - [ ] the cached response is served immediately, and a background revalidation is attempted
- [ ] WHEN a POST request to `/api/orders` is made THEN
  - [ ] it always goes to the network, never served from or written to any cache
- [ ] WHEN a request for an image/font asset is made THEN
  - [ ] it is served cache-first, bounded by the expiration plugin
- [ ] WHEN a request to a known federated remote's origin is made THEN
  - [ ] it is served stale-while-revalidate, not precached

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/ServiceWorker/service-worker.ts.extend|ServiceWorker/service-worker.ts.extend]]
