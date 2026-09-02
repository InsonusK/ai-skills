---
description: Extend the offline-first service worker with a fifth caching rule — stale-while-revalidate runtime caching for federated remote chunks, sourced from RemoteRegistryService
project_name: platform-shell
name: service-worker
element_kind: script
change_kind: extend
tags:
  - solution/federation-host
  - element/service-worker-ts
---

# How this file is used
This applies only once both [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] and this solution are both present — it extends [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create|the base service worker's four content-type rules]] with a fifth rule for federated remote chunks, whose origins are only known once this solution's `RemoteRegistryService` exists. Not applicable to a plateau that has offline-first without platform-embeddability.

# Goals

- Let a federated embeddable app's remote chunks keep working from their last-cached version if that team's independent deployment is temporarily unreachable, using the same runtime-caching mechanism the base service worker already established for other content types

# Implementation changes

```typescript
// sw-src.ts — appended after the base solution's four rules
import { StaleWhileRevalidate } from 'workbox-strategies';

// 5. Federated remote chunks — stale-while-revalidate runtime caching (URLs unknown at build time,
//    sourced from the same manifest RemoteRegistryService resolves)
registerRoute(
  ({ url }) => KNOWN_REMOTE_ORIGINS.some(origin => url.origin === origin),
  new StaleWhileRevalidate({ cacheName: 'federation-remotes' }),
);
```

# Rule changes

## MUST
- `KNOWN_REMOTE_ORIGINS` MUST be sourced from the same runtime remote registry configuration [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create|`RemoteRegistryService`]] uses, not hardcoded separately, to avoid the two falling out of sync.
- This rule MUST be registered after the base solution's network-only rule (auth/mutations), so route-matching order still guarantees auth/mutations never resolve to this rule.

## SHOULD
- **Hardcoding `KNOWN_REMOTE_ORIGINS` as a separate static list instead of deriving it from `RemoteRegistryService`'s own manifest** — Consequence: a newly onboarded embeddable app's origin is never cached, and the two lists silently drift apart over time — Instead: derive `KNOWN_REMOTE_ORIGINS` from the same manifest `RemoteRegistryService` fetches

# Check list

- [ ] `KNOWN_REMOTE_ORIGINS` is derived from the same configuration source as `RemoteRegistryService`
- [ ] This rule is registered after the base auth/mutations network-only rule

# Unittest TestCases

- [ ] WHEN a request to a known federated remote's origin is made THEN
  - [ ] it is served stale-while-revalidate, not precached
