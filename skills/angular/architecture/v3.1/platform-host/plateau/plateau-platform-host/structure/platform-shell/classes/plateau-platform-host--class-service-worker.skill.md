---
name: plateau-platform-host--class-service-worker
description: The 5th Workbox rule added to the monolith's service worker — stale-while-revalidate for federated remote chunks, origins derived from the same manifest RemoteRegistryService uses; present ONLY because the parent monolith has offline-first (VP3 FederatedReadResilience) — platform-host plateau
domain: skill
type: template
whenToUse: when adding the federated-remote-chunk caching rule to sw-src.ts, or checking that KNOWN_REMOTE_ORIGINS is derived from the manifest and registered after the network-only rule
plateau: platform-host
artifact_type: script
version: 20260903180000
tags:
  - skill/template/class
  - plateau/platform-host
  - stack/typescript
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]]"
---

> Extends [`plateau-multiuser-monolith`'s `class-service-worker`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/platform-shell/classes/plateau-multiuser-monolith--class-service-worker.skill.md) (four content-type rules) with a fifth. **Conditional** — applies only when the composed monolith plateau answers `OfflineReadResilience` (VP4) = Yes. `RuntimeRemoteFederation` itself has no offline dependency. Not part of the `example/` (a smoke test with no SW).

# Goal

- Let a federated remote's chunks keep working from their last-cached version when that team's independent deployment is temporarily unreachable, using the runtime-caching mechanism the base service worker already established

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/ServiceWorker/service-worker.ts.extend.md|ServiceWorker/service-worker.ts.extend]]

# Implementation

```typescript
// sw-src.ts — appended AFTER the base four rules (esp. after the network-only auth/mutations rule)
import { StaleWhileRevalidate } from 'workbox-strategies';
import { registerRoute } from 'workbox-routing';

// 5. Federated remote chunks — stale-while-revalidate. Origins are unknown at build
//    time; derived from the same manifest RemoteRegistryService resolves.
registerRoute(
  ({ url }) => KNOWN_REMOTE_ORIGINS.some((origin) => url.origin === origin),
  new StaleWhileRevalidate({ cacheName: 'federation-remotes' }),
);
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/ServiceWorker/service-worker.ts.extend.md|ServiceWorker/service-worker.ts.extend]]

# Rules

## MUST
- `KNOWN_REMOTE_ORIGINS` is derived from the same runtime manifest `RemoteRegistryService` uses — never a separate hardcoded list that can drift.
- This rule is registered **after** the base network-only (auth/mutations) rule, so route-matching order still guarantees auth/mutations never resolve to it.
- The rule is added only in a plateau whose composed monolith has `solution-offline-first` — never in a host composing a monolith plateau with offline = No.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/ServiceWorker/service-worker.ts.extend.md|ServiceWorker/service-worker.ts.extend]]

# Check list

- [ ] `KNOWN_REMOTE_ORIGINS` comes from the manifest, not a static list
- [ ] The 5th rule is registered after the network-only rule
- [ ] The rule is absent when the composed monolith has no offline-first

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/ServiceWorker/service-worker.ts.extend.md|ServiceWorker/service-worker.ts.extend]]

# Unittest TestCases

- [ ] WHEN a request to a known federated remote's origin is made THEN it is served stale-while-revalidate, not precache
- [ ] WHEN an auth or non-GET request is made to a remote origin THEN it still resolves to the base network-only rule, never this one

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/ServiceWorker/service-worker.ts.extend.md|ServiceWorker/service-worker.ts.extend]]
