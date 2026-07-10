---
name: solution-platform-embeddability
description: Turns the base workspace's platform-shell into a Native Federation dynamic host, and defines the baseline structure any independently deployed embeddable app must follow to be loaded into it
domain: skill
type: architecture
version: 1.0
tags:
  - skill/architecture/solution
  - angular
  - nx
  - module-federation
  - native-federation
  - platform-embeddability
triggers:
  - Turning the base platform-shell into a host capable of loading independently deployed applications
  - Onboarding a new embeddable app built by a separate team/repository
  - Reviewing whether a platform/embedded-app boundary violates the federation contract
creates:
  - "{embeddable-app-name} (separate repository for an independently deployed embeddable app)"
extends:
  - "apps/platform-shell (turned into a Native Federation dynamic host)"
  - "Repository (type:host tag, shared-dependency rules, @platform/contracts)"
depends_on:
  - "[[../solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]"
adr:
  - "[[adr/embedding-mechanism.md|Embedding Mechanism ADR]]"
---

# Goal

- Let independently built and independently deployed applications be loaded into the platform shell at runtime, without the platform being rebuilt when an embeddable app ships a new version
- Give the platform and every embeddable app a real, low-friction way to exchange state and events at runtime
- Define this as two related but distinct plateaus: **platform** (the host side, extending the base repository from solution #1) and **embeddable application** (the structure any separate, independently deployed app repository must follow to be loadable by the platform)

# Capabilities

- Independent release cycles per embeddable app — no coordination with the platform's own deploy schedule required for routine releases
- Real-time, low-friction state/event exchange between host and embedded app via a shared singleton contract, not serialized message passing
- New embeddable apps can be onboarded by updating a runtime manifest, without a platform code change
- A clear, enforceable contract boundary (`@platform/contracts`) that keeps the platform and every embeddable app decoupled from each other's internals

# Core Principles

- The platform never depends on an embeddable app's internals at build time — only on `@platform/contracts` and the federation `remoteEntry`/exposed-module boundary
- Remote discovery is a runtime concern (Dynamic Federation): which embeddable apps exist and where they are served from is resolved from configuration, not compiled into the host
- Angular itself and `@platform/contracts` are shared as `singleton` dependencies, so host and every embeddable app run in one JS runtime with one instance of the shared contract
- `@platform/contracts` is versioned and published like an external package (the same pattern used for the design system in this architecture) — cross-team compatibility is a semver contract, not a monorepo implementation detail
- An embeddable app repository is not required to adopt Nx or any platform-specific tooling; it only needs to satisfy the federation contract described in this solution

# Adr

- [[adr/embedding-mechanism.md|Native Federation + Dynamic Federation instead of Webpack Module Federation, Web Components, or iframe]]
  - Selected variant: Native Federation + Dynamic Federation — chosen because it matches Angular's current esbuild-based build system, shares one Angular runtime between host and remotes for real singleton state sharing, and lets embeddable apps be discovered and loaded at runtime without a platform rebuild

# Requirements

SOLUTION:
- [[../solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]
  - [[../solution-repository-structure.skill/Implementation/Repository.create.md|apps/platform-shell]]
    - Extended by this solution into a federation host (see [[./Implementation/PlatformHost/platform-shell.project.extend.md]])

NPM:
- @angular-architects/native-federation
  - Federation build plugin and `loadRemoteModule` runtime API, used by both platform-shell and every embeddable app
- @platform/contracts
  - Shared, independently versioned EventBus/state contract package, published from its own repository and consumed as a `singleton` shared dependency by both plateaus

# Template Skill Mutations

REPOSITORY:
- [[./Implementation/PlatformHost/Repository.extend.md|Repository]] - extend - add `type:host` tag, federation shared-dependency requirements, and `@platform/contracts` NPM dependency
- [[./Implementation/EmbeddableApp/Repository.create.md|Repository]] - create - baseline structure any independent embeddable-app repository must follow: federation remote config, `@platform/contracts` dependency, independent CI/deploy

PROJECT:
- [[./Implementation/PlatformHost/platform-shell.project.extend.md|apps/platform-shell]] - extend - turn into a Native Federation dynamic host
  - [[./Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create.md|remote-registry.service.ts]] - create - runtime resolver for available embeddable apps' remoteEntry URLs

This solution intentionally does not prescribe an internal `feature`/`data-access` structure for the embeddable app's own repository — that repository is free to apply solution #1's structure internally if it also chooses Nx, but it is only required to satisfy the federation contract described in [[./Implementation/EmbeddableApp/Repository.create.md]].

# Workflow

## Onboard a new embeddable app (happy path)

1. A separate team scaffolds their own repository following [[./Implementation/EmbeddableApp/Repository.create.md]] — federation remote config, `@platform/contracts` dependency, own CI/deploy.
2. They deploy their app and publish its `remoteEntry` URL and exposed module path to the platform's runtime manifest.
3. The platform's `RemoteRegistryService` picks up the new entry on its next manifest refresh — no platform rebuild required.
4. Platform shell loads the remote's exposed component via `loadRemoteModule` and mounts it.
5. Host and embedded app exchange events/state through the shared `@platform/contracts` EventBus, both resolving to the same singleton instance.

![Onboard a new embeddable app (happy path)](./diagrams/onboard-a-new-embeddable-app-happy-path.mmd)

## Independent redeploy of an embeddable app (happy path)

1. The embeddable app's team ships a new version to their own deploy target, at the same `remoteEntry` URL already registered with the platform.
2. Next time a user loads (or the shell refreshes) the remote, the new version is served — no platform code change, rebuild, or redeploy involved.

## Remote fails to load (failure path)

1. `RemoteRegistryService` cannot resolve or load a given remote (unreachable URL, incompatible `@platform/contracts` version, etc.).
2. The failure is caught at the point of loading, not allowed to propagate to a shell-wide crash.
3. The shell renders a fallback in place of that remote's slot; the rest of the platform, and any other already-loaded remotes, keep working.

## Version-incompatible shared contract (cross-cutting failure path)

1. An embeddable app is built and deployed against a major version of `@platform/contracts` incompatible with the one the platform host expects.
2. Depending on `strictVersion` configuration, the federation runtime either loads a duplicate, non-shared instance of the contracts package (silently breaking state sharing) or refuses to load the remote with a version-mismatch error.
3. This must surface as an explicit, visible failure (see Rules in [[./Implementation/PlatformHost/Repository.extend.md]]) rather than a silently duplicated runtime.

# Rules

## MUST
- [[./Implementation/PlatformHost/Repository.extend.md#MUST|PlatformHost/Repository.extend]]
- [[./Implementation/PlatformHost/platform-shell.project.extend.md#MUST|PlatformHost/platform-shell.project.extend]]
  - [[./Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create.md#MUST|remote-registry.service.ts.create]]
- [[./Implementation/EmbeddableApp/Repository.create.md#MUST|EmbeddableApp/Repository.create]]

## SHOULD
- [[./Implementation/PlatformHost/platform-shell.project.extend.md#SHOULD|PlatformHost/platform-shell.project.extend]]
- [[./Implementation/EmbeddableApp/Repository.create.md#SHOULD|EmbeddableApp/Repository.create]]

## MUST NOT
- [[./Implementation/PlatformHost/Repository.extend.md#MUST NOT|PlatformHost/Repository.extend]]
- [[./Implementation/EmbeddableApp/Repository.create.md#MUST NOT|EmbeddableApp/Repository.create (import restriction)]]

# Anti-patterns

- [[./Implementation/PlatformHost/Repository.extend.md|See PlatformHost/Repository.extend.md]] — hardcoding a remote's URL/version into the host, or bypassing `@platform/contracts` to import a remote's internals directly.
- [[./Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create.md|See remote-registry.service.ts.create.md]] — caching the remotes manifest for the whole tab lifetime with no refresh path.
- [[./Implementation/EmbeddableApp/Repository.create.md|See EmbeddableApp/Repository.create.md]] — bumping `@platform/contracts` to an incompatible major version without cross-team coordination, or depending on platform-shell internals instead of the shared contract.

# Check list

- [ ] `apps/platform-shell` is tagged `type:host` and resolves remotes at runtime, not build time
- [ ] `@platform/contracts` (and Angular) are declared as `singleton: true` on both the platform host and every embeddable app
- [ ] A new embeddable app can be onboarded by updating the runtime remotes manifest alone, with no platform code change
- [ ] Every embeddable app repository builds, tests, and deploys independently of the platform's own CI/CD pipeline
- [ ] A failure to load one remote does not take down the rest of the platform shell
- [ ] No import exists, in either direction, between platform-shell internals and an embeddable app's internals outside of `@platform/contracts` and the federation `remoteEntry` boundary