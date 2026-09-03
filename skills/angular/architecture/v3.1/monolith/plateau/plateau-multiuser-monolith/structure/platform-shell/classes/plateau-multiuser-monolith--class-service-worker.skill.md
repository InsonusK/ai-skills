---
name: plateau-multiuser-monolith--class-service-worker
description: The Workbox service worker for apps/platform-shell — four content-type routing rules, testable predicates in sw-routes.ts, and the sw-build.mjs Nx build step — multiuser-monolith plateau
domain: skill
type: template
plateau: multiuser-monolith
artifact_type: module
version: 20260903150000
tags:
  - skill/template/class
  - plateau/multiuser-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"

> Three files in `apps/platform-shell/src/`: `sw-src.ts` (Workbox runtime, WebWorker context, compiled with `tsconfig.sw.json`, excluded from the app build), `sw-routes.ts` (pure content-type predicates, unit-tested), `sw-build.mjs` (Nx `build-sw` step — esbuild-bundles `sw-src.ts` then `workbox-build`'s `injectManifest`).

# Goal

- Implement the four content-type caching strategies as concrete Workbox routing rules, keeping the routing *decisions* in a plain module that can be unit-tested without a service worker context

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create.md|ServiceWorker/service-worker.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Four rules: **precache** (app shell + lazy chunks), **cache-first** (images/fonts), **stale-while-revalidate** (API GETs), **network-only** (auth + every non-GET)
- The network-only rule is registered **first** so a mutation can never be matched by the API-reads rule — route-matching order matters
- Generated via Workbox's programmatic API in the Nx pipeline — never ngsw, never a webpack plugin (this workspace uses the esbuild `@angular/build:application` builder)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create.md|ServiceWorker/service-worker.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/adr/caching-strategy-per-content-type.md|Caching Strategy Per Content Type ADR]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| SW runtime | — | — | `sw-src.ts` | `sw-src.ts` |
| Routing predicates | `is{Kind}` | `isApiRead` / `isNetworkOnly` / `isStaticAsset` | `sw-routes.ts` | `sw-routes.ts` |
| Build step | — | — | `sw-build.mjs` | `sw-build.mjs` |

# Implementation

```typescript
// Skill: class-service-worker
// Plateau: multiuser-monolith
// Version: 20260903090000

// sw-routes.ts — pure, DOM-safe, unit-tested
export function isNetworkOnly({ url, method }: RouteInput): boolean {
  return url.pathname.startsWith('/auth/') || method.toUpperCase() !== 'GET';
}
export function isStaticAsset({ destination }: RouteInput): boolean {
  return destination === 'image' || destination === 'font';
}
export function isApiRead(i: RouteInput): boolean {
  return i.url.pathname.startsWith('/api/') && i.method.toUpperCase() === 'GET' && !isNetworkOnly(i);
}
```

```typescript
// sw-src.ts — Workbox wiring (order: network-only FIRST)
precacheAndRoute(self.__WB_MANIFEST);
registerRoute((c) => isNetworkOnly(toInput(c)), new NetworkOnly());
registerRoute((c) => isStaticAsset(toInput(c)), new CacheFirst({ cacheName: 'static-assets', plugins: [new ExpirationPlugin({ maxEntries: 200 })] }));
registerRoute((c) => isApiRead(toInput(c)), new StaleWhileRevalidate({ cacheName: 'api-reads' }));
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create.md|ServiceWorker/service-worker.create]]

# Rules

## MUST
- The network-only rule (auth + every non-GET) must be registered before the API-reads rule so a mutation always resolves to `network-only`, never stale-while-revalidate.
- `sw-src.ts` must be compiled with `tsconfig.sw.json` (`lib: ["webworker"]`) and excluded from `tsconfig.app.json` / the app build.
- The routing *decisions* must live in `sw-routes.ts` as pure predicates so they can be unit-tested; `sw-src.ts` only wires them to Workbox strategies.
- `sw-build.mjs` must run after the production bundle is written and generate `sw.js` via `workbox-build`'s `injectManifest`.
- Never apply several plateau templates per class/artifact.
- Never cache an auth endpoint or any non-GET request with anything other than `network-only`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create.md|ServiceWorker/service-worker.create]]

# Check list

- [ ] Four rules registered; network-only precedes API-reads
- [ ] `sw-routes.ts` predicates are pure and have a Vitest spec
- [ ] `sw-src.ts` is not in `tsconfig.app.json` include / the app build
- [ ] `nx build-sw platform-shell` emits `dist/apps/platform-shell/browser/sw.js`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create.md|ServiceWorker/service-worker.create]]

# Unittest TestCases

- [ ] WHEN a GET `/api/orders` is inspected THEN
  - [ ] `isApiRead` is true and `isNetworkOnly` is false
- [ ] WHEN a POST `/api/orders` is inspected THEN
  - [ ] `isNetworkOnly` is true and `isApiRead` is false
- [ ] WHEN an auth endpoint (`/auth/...`) is inspected THEN
  - [ ] `isNetworkOnly` is true regardless of method

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create.md|ServiceWorker/service-worker.create]]
