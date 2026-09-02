---
description: Workbox-generated service worker for apps/platform-shell, including the build script and the four content-type-specific runtime routing rules
project_name: platform-shell
name: service-worker
element_kind: script
change_kind: create
tags:
  - solution/offline-first
  - element/service-worker
---

# Goals

- Implement the four content-type-specific caching strategies as concrete Workbox routing rules

# Implementation changes

```typescript
// sw-build.ts (invoked as an Nx build step, not a webpack plugin)
import { injectManifest } from 'workbox-build';

await injectManifest({
  swSrc: 'src/sw-src.ts',
  swDest: 'dist/apps/platform-shell/sw.js',
  globDirectory: 'dist/apps/platform-shell',
  globPatterns: ['**/*.{js,css,html}'], // app shell + lazy feature chunks -> precache
});
```

```typescript
// sw-src.ts — the actual runtime routing rules
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

// 4. Auth endpoints and all non-GET requests — network-only, never cached
registerRoute(
  ({ url, request }) => url.pathname.startsWith('/auth/') || request.method !== 'GET',
  new NetworkOnly(),
);
```

# Rule changes

## MUST
- Rule 4 (network-only for auth/mutations) must be registered so it takes precedence over rule 3 for any non-GET request, even one under `/api/` — route matching order matters.

## SHOULD
- **Registering the API-reads (stale-while-revalidate) route before the auth/mutations (network-only) route** — Consequence: depending on Workbox's route-matching order, a mutation request could be incorrectly matched and cached — Instead: register the network-only rule first, or make its matcher function explicitly exclude what rule 3 should handle

# Check list

- [ ] The four rules are registered in an order that guarantees auth/mutations always resolve to `network-only`

# Unittest TestCases

- [ ] WHEN a GET request to `/api/orders` is made offline with a cached response available THEN
  - [ ] the cached response is served immediately, and a background revalidation is attempted
- [ ] WHEN a POST request to `/api/orders` is made THEN
  - [ ] it always goes to the network, never served from or written to any cache
