---
name: service-worker-mechanism
description: Which service worker tooling to use for offline resilience
problem: Angular's own built-in Service Worker (ngsw) is now feature-frozen and the Angular team itself recommends alternatives for advanced caching needs; we need a mechanism capable of per-content-type caching strategies and runtime caching of dynamically-resolved federated remote chunks
decision: Use Workbox instead of Angular's built-in Service Worker (ngsw) or a fully hand-written service worker
tags:
  - solution/offline-first
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The application needs a service worker to make the app shell load offline and to apply different caching strategies to different kinds of requests (app shell, static assets, API reads, federated remote chunks, and auth/mutation endpoints that must never be cached). Angular ships its own `@angular/service-worker` (ngsw), but Angular's own documentation now describes it as a basic utility with a limited feature set for simple offline support, explicitly stating that only security fixes will be accepted going forward and recommending native browser APIs for more advanced caching and offline capabilities. Given this workspace's needs — multiple distinct caching strategies, and runtime caching of remote chunks whose URLs are only known at runtime (per the "Встраиваемость платформы" solution's Dynamic Federation) — we need to decide what replaces or supplements ngsw.

# Selected variant

**Selected variant:** [[#Workbox]]

Workbox is adopted as the service worker tooling, integrated into the Nx/esbuild-based build pipeline via its programmatic build API rather than a webpack-specific plugin (since this workspace uses Angular's esbuild-based `ApplicationBuilder`, not the webpack builder).

# Searched variants

## Workbox

### Description

A set of libraries and build tools (`workbox-build`, `workbox-routing`, `workbox-strategies`, etc.) for authoring and generating a service worker with fine-grained, per-route caching strategies (cache-first, network-first, stale-while-revalidate, network-only), plus precaching for build-time-known assets and runtime caching for URLs only known at request time.

### Benefits

- Actively developed, with per-build-tool integrations (Vite, webpack, Next.js) rather than being tied to one bundler — usable with Nx's esbuild-based Angular build via its programmatic `workbox-build` API in a custom build step
- Supports distinct caching strategies per route/content-type out of the box, matching this solution's need for five different strategies (app shell, static assets, API reads, auth/mutations, federated remote chunks)
- Runtime caching (matching requests by URL pattern as they occur, rather than only precaching a fixed manifest) directly supports caching federated remote chunks whose URLs are resolved dynamically at runtime, per the platform-embeddability solution's Dynamic Federation
- A service worker registered by `apps/platform-shell` intercepts `fetch` events for any request initiated by a document under its control — including cross-origin requests to independently-deployed embeddable apps' `remoteEntry` URLs — so a single service worker at the shell level can still cache federation chunks served from other origins

### Costs

- Requires custom build integration effort, since this workspace's esbuild-based Angular build has no first-party Workbox plugin the way webpack does — the manifest/service-worker generation step needs to be wired in manually via `workbox-build`'s programmatic API
- Loses `@angular/service-worker`'s Angular-specific integration conveniences (e.g. the `SwUpdate` service's Angular-idiomatic update-checking API) — an equivalent update-checking flow needs to be built using the service worker's own `postMessage`/`skipWaiting` primitives directly

## Angular Service Worker (ngsw)

### Description

Angular's own built-in service worker, configured via `ngsw-config.json`, integrated directly into the Angular CLI build.

### Benefits

- Zero custom build integration — works out of the box with `ng add @angular/pwa`
- Angular-idiomatic update-checking via the `SwUpdate` service

### Costs

- Explicitly described by Angular's own documentation as feature-frozen, accepting only security fixes, with the framework's own team recommending alternatives for anything beyond simple offline support
- Its caching configuration model does not naturally express "cache this pattern of runtime-resolved URLs" for federated remote chunks whose origins aren't known until Dynamic Federation resolves them
- Building on a tool the framework itself says will not receive new features creates a dead end for any caching need beyond what it already supports today

## Fully hand-written service worker (no Workbox)

### Description

Write the service worker's `fetch`/`install`/`activate` event handlers directly, without any library.

### Benefits

- No third-party dependency at all
- Full control over every line of caching logic

### Costs

- Reimplements, by hand, exactly what Workbox already provides (routing, strategy implementations, precache manifest injection, cache expiration) — significant effort for no architectural benefit over adopting Workbox
- More surface area for subtle service-worker lifecycle bugs (a notoriously easy area to get wrong — stale caches, missed `skipWaiting`, incorrect scope) without a well-tested library absorbing that complexity
