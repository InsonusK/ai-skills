---
name: solution-federation-host
description: Turns the monolith's platform-shell into a Native Federation dynamic host that discovers and mounts independently deployed remote apps at runtime, with a runtime remote manifest, a failed-remote fallback, and (when offline-first is present) a stale-while-revalidate rule for remote chunks
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - nx
  - module-federation
  - framework/native-federation
  - framework/angular
  - concern/architecture
  - solution/federation-host

whenToUse: when turning the shell into a federation host, wiring runtime remote discovery, or reviewing whether a host/remote boundary violates the federation contract
creates:
  - apps/platform-shell/src/app/remote-registry.service.ts
extends:
  - apps/platform-shell (Native Federation dynamic host)
  - Repository (type:host tag, federation shared-dependency rules)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/adr/embedding-mechanism.md|embedding-mechanism]]"
---

# Goal
- Let independently built, independently deployed remote apps be loaded into `apps/platform-shell` at runtime, with no platform rebuild when a remote ships a new version.
- Keep the host decoupled from every remote's internals — the only contract is `@platform/contracts` (from `solution-platform-contracts`) plus the federation `remoteEntry`/exposed-module boundary.
- When `solution-offline-first` is also composed, extend its service worker with a fifth rule so a temporarily unreachable remote still mounts from cache — this is the `FederatedReadResilience` feature.

# Capabilities
- Runtime remote discovery via a manifest (`RemoteRegistryService`) — onboard a remote by updating config, no host code change.
- A failed remote load degrades to a fallback slot in that route, never a shell-wide crash.
- `apps/platform-shell` mounts a remote exactly like a local feature — one root-segment `loadChildren`-shaped entry, resolved through the manifest.

# Core Principle
- The host never depends on a remote's internals at build time — only on `@platform/contracts` and the federation boundary.
- Remote discovery is a **runtime** concern (Dynamic Federation) — which remotes exist and where they are served from is resolved from configuration, never compiled in.
- Angular and `@platform/contracts` are shared `singleton: true` so host and every remote run in one JS runtime with one contract instance.
- A version-incompatible `@platform/contracts` on a remote surfaces as an explicit, visible failure — never a silently duplicated runtime.
- The fifth service-worker rule (federated remote chunks, stale-while-revalidate) applies **only when `solution-offline-first` is also present**; `RuntimeRemoteFederation` itself has no offline dependency.

# Boundaries
- Assumes a `monolith` baseline (`solution-repository-structure` + `solution-app-routing`). The host is a monolith plateau + this solution; state, data-access, auth, offline are all monolith concerns composed via `parent_plateaus`, not re-declared here.
- Does **not** `depends_on solution-offline-first` (the V1 `depends_on` was over-strong). `FederatedReadResilience`'s `service-worker.ts.extend` applies conditionally when both solutions co-occur — a plateau-level ordering, recorded in that plateau's `registry/`.
- Does not model the remote side — see [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]]. Does not model the contracts package — see [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]].
- Does not implement `SessionContract` publication — that is [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]].

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/adr/embedding-mechanism.md|embedding-mechanism]] — Native Federation + Dynamic Federation, over Webpack Module Federation / Web Components / iframe: matches Angular's esbuild build, one shared Angular runtime for real singleton state, runtime discovery without a host rebuild.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]
  - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|apps/platform-shell]] - extended into a federation host
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]
  - the hierarchical route-ownership pattern is what a remote reuses one level down
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]
  - provides the `@platform/contracts` package this host shares as a strict singleton

NPM:
- `@angular-architects/native-federation` — federation build plugin + `loadRemoteModule` runtime API.

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/Repository.extend.md|Repository]] - extend - `type:host` tag, federation shared-dependency rules, explicit-failure requirement on a version mismatch

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend.md|apps/platform-shell]] - extend - turn into a Native Federation dynamic host
  - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create.md|remote-registry.service.ts]] - create - runtime resolver for remotes' `remoteEntry` URLs
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/ServiceWorker/service-worker.ts.extend.md|service-worker (extend)]] - extend - the fifth caching rule (federated remote chunks); applies only when `solution-offline-first` is also composed

# Workflow

## Onboard a new remote (happy path)

1. A separate team scaffolds their repo per [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] and publishes its `remoteEntry` URL + exposed module path to the platform's runtime manifest.
2. `RemoteRegistryService` picks up the new entry on its next manifest refresh — no host rebuild.
3. The shell loads the remote's exposed module via `loadRemoteModule` and mounts it at one root segment.
4. Host and remote exchange events/state through the shared `@platform/contracts` — the same singleton instance.

## Remote fails to load (failure path)

1. `RemoteRegistryService` cannot resolve or load a remote (unreachable URL, incompatible contract version).
2. The failure is caught at the load point, not propagated.
3. The shell renders a fallback in that route's slot; the rest of the platform keeps working.

## Federated remote unreachable, offline-first present (failure path)

1. A remote's `remoteEntry` is temporarily unreachable (that team's outage).
2. The fifth stale-while-revalidate rule serves the last-cached version.
3. The next load once reachable revalidates and updates the cache.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/Repository.extend.md#MUST|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend.md#MUST|platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create.md#MUST|remote-registry.service.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/ServiceWorker/service-worker.ts.extend.md#MUST|service-worker.ts.extend]]
- Never hardcode a remote's URL or version into the host — always resolve through `RemoteRegistryService`'s manifest.
  - Risk: a remote redeploy or re-host forces a platform code change, defeating the whole point.
  - Fix: the manifest is the single source of remote locations.
- Never import a remote's internals into the host, or the host's internals into a remote.
  - Risk: build-time coupling that breaks independent deployability.
  - Fix: the only contract is `@platform/contracts` + the federation boundary.

## SHOULD
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend.md#SHOULD|platform-shell.project.extend]]
- Avoid caching the remotes manifest for the whole tab lifetime with no refresh path.
- Avoid hardcoding `KNOWN_REMOTE_ORIGINS` for the SW rule instead of deriving it from `RemoteRegistryService`.

# Check list
- [ ] `apps/platform-shell` is tagged `type:host` and resolves remotes at runtime, not build time.
- [ ] `@platform/contracts` and Angular are `singleton: true` on the host.
- [ ] A new remote is onboarded by updating the manifest alone.
- [ ] A failed remote load does not take down the shell.
- [ ] No import between host internals and any remote's internals outside `@platform/contracts` + the federation boundary.
- [ ] If `solution-offline-first` is present, remote chunks are runtime-cached, sourced from the manifest.
