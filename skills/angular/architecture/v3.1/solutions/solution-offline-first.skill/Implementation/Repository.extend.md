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
- The service worker must be generated via Workbox's programmatic build API, integrated into the Nx build pipeline — not via a webpack-specific plugin, and not via `@angular/service-worker` (ngsw), per [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/adr/service-worker-mechanism.md|service-worker-mechanism]].
- Every feature's `{feature}.client.ts` must distinguish a network-level failure (request never reached the server) from a server-side error response, throwing `OfflineTransportError` for the former — this is the one hook this solution establishes for the future `solution-offline-sync` to build a mutation queue on top of.
- Auth endpoints and every non-GET request must be configured as `network-only` in the service worker's routing rules — never cached, per [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/adr/caching-strategy-per-content-type.md|caching-strategy-per-content-type]].

- Never introduce any durable, persisted queue for failed mutations — that is explicitly out of scope, deferred to the future `solution-offline-sync`. A mutation that fails due to `OfflineTransportError` in this solution still surfaces as a failure to the caller; it is not queued or retried automatically here.
# Unittest TestCases

- [ ] WHEN a feature's Client makes a request while the network is genuinely unreachable THEN
  - [ ] it throws `OfflineTransportError`, not that feature's generic domain error
- [ ] WHEN the service worker's routing configuration is inspected THEN
  - [ ] every auth/mutation endpoint is configured `network-only`

## SHOULD
- **Building even a minimal mutation retry/queue mechanism as part of this solution** — Consequence: partially duplicates the future `solution-offline-sync`'s scope, splitting one coherent problem (durable queueing, retry, conflict handling, pending-state UI) across two solutions with neither fully solving it — Instead: this solution only classifies the error (`OfflineTransportError`); the queue itself is the next solution's job
- **Configuring auth or mutation endpoints with any caching strategy other than network-only** — Consequence: a cached auth/mutation response is an active correctness and security bug, not just staleness — Instead: auth and all non-GET requests are always `network-only`
