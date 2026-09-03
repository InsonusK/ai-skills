---
description: Extend the base workspace with a Workbox-generated service worker registered by apps/platform-shell, a new connectivity slice in libs/shared/state, and the offline-error distinction in every feature's Client
element_kind: repository
change_kind: extend
tags:
  - solution/offline-first
  - element/monolith-repository
---

# Structure

## Workspace Structure

No new top-level directories. This extends `apps/platform-shell` (service worker registration + build integration), `libs/shared/state` (new `connectivity` slice, alongside `auth` from the "State management"/`solution-authentication`s), and every feature's `{feature}.client.ts` (from `solution-api-http-layer`).

## Directory and project skills

| Directory/file | Description |
| --------------- | ----------- |
| /apps/platform-shell/src/sw-build.ts | Custom build step invoking `workbox-build`'s programmatic API to generate the service worker from the five routing rules defined in [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create]], run as part of the Nx build target (not a webpack plugin, since this workspace uses the esbuild-based `ApplicationBuilder`). |
| /libs/shared/state/src/lib/connectivity | New slice: `isOnline` (combining `navigator.onLine` events and periodic health-check results), following the same classical-NgRx pattern as the existing `auth` slice. |
| /libs/{feature}/data-access/src/lib/{feature}.client.ts | Extended: catches a network-level failure (no response received at all) and throws `OfflineTransportError` instead of a generic domain error, distinguishing it from a genuine server-side failure (4xx/5xx, which still map to that feature's own domain errors as already established). |

# Rules

## MUST
- The service worker is generated via Workbox's programmatic build API, integrated into the Nx build pipeline — not a webpack plugin, not `@angular/service-worker` (ngsw).
  - Risk: ngsw's manifest model and webpack plugins do not fit Angular's esbuild pipeline — the SW drifts from the actual bundle.
  - Fix: an `nx:run-commands` `build-sw` target runs `workbox-build`'s `injectManifest` after the prod build; per [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/adr/service-worker-mechanism.md|service-worker-mechanism]].
- Every feature's `{feature}.client.ts` distinguishes a network-level failure (request never reached the server) from a server error response, throwing `OfflineTransportError` for the former.
  - Risk: treating "we're offline, retryable" the same as "the server rejected this" makes a future write queue impossible to build correctly.
  - Fix: check `HttpErrorResponse.status === 0` first and throw the shared `OfflineTransportError` — the one hook `solution-offline-sync` builds on.
- Auth endpoints and every non-GET request are `network-only` in the SW routing rules — never cached.
  - Risk: a cached auth response or a replayed mutation from cache is a security and correctness hazard.
  - Fix: register the `network-only` rule first so it wins over the stale-while-revalidate API-reads rule; per [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/adr/caching-strategy-per-content-type.md|caching-strategy-per-content-type]].
- Never introduce a durable, persisted queue for failed mutations here.
  - Risk: a half-built queue in this solution collides with the real one `solution-offline-sync` adds (VP5).
  - Fix: an `OfflineTransportError` surfaces as a failure to the caller; queueing/retry is entirely VP5's concern.
# Unittest TestCases

- [ ] WHEN a feature's Client makes a request while the network is genuinely unreachable THEN
  - [ ] it throws `OfflineTransportError`, not that feature's generic domain error
- [ ] WHEN the service worker's routing configuration is inspected THEN
  - [ ] every auth/mutation endpoint is configured `network-only`

## SHOULD
- **Building even a minimal mutation retry/queue mechanism as part of this solution** — Consequence: partially duplicates the future `solution-offline-sync`'s scope, splitting one coherent problem (durable queueing, retry, conflict handling, pending-state UI) across two solutions with neither fully solving it — Instead: this solution only classifies the error (`OfflineTransportError`); the queue itself is the next solution's job
- **Configuring auth or mutation endpoints with any caching strategy other than network-only** — Consequence: a cached auth/mutation response is an active correctness and security bug, not just staleness — Instead: auth and all non-GET requests are always `network-only`
