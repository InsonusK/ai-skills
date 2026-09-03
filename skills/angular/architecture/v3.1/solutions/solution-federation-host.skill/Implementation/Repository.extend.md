---
description: Extend the base workspace so apps/platform-shell becomes a Native Federation dynamic host, and add the tags/dependency needed for embeddable apps
element_kind: repository
change_kind: extend
tags:
  - solution/federation-host
  - element/monolith-repository
---

# Structure

## Workspace Structure

No new top-level directories are added to the base layout from [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create]]. `apps/platform-shell` is reconfigured, and one new tag is introduced.

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /apps/platform-shell | Gains `type:host` tag in addition to `type:app`, `scope:platform`. Now owns federation host configuration (`federation.config.ts`) and a runtime remote registry (see [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend]]). |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor, matching the Angular major version in use | Native Federation host/remote runtime and build plugin |
| @platform/contracts | semver range, published from a separate repository (see [[../../adr/embedding-mechanism.md|ADR]]) | Shared, versioned contract for cross-app state/events (EventBus interface, shared DTOs). Marked as a `singleton` shared dependency in the federation config so host and every remote resolve to the exact same runtime instance. |

# Rules

## MUST
- `apps/platform-shell` declares the `type:host` tag in addition to `type:app`/`scope:platform`.
  - Risk: without it, lint boundary rules and CI filters that target the host by tag silently skip it.
  - Fix: add `type:host` to the project's tags in `project.json`.
- `apps/platform-shell` marks `@platform/contracts` and Angular as `singleton: true` in its federation shared-dependency config.
  - Risk: non-singleton sharing loads a second Angular / contracts instance, and `Signal`/`InjectionToken` identity breaks across the boundary.
  - Fix: `shared: { '@angular/core': { singleton: true }, '@platform/contracts': { singleton: true }, ... }`.
- The list of remotes and their URLs is resolved at runtime (Dynamic Federation manifest), never hardcoded into the host's build.
  - Risk: a build-time remote list forces a host rebuild+redeploy every time any embeddable app ships, defeating independent deployability.
  - Fix: load `federation.manifest.json` at bootstrap; refresh it independently of host deploys.
- The host never bundles a specific embeddable app's code at build time.
  - Risk: static-importing a remote couples the host's release to that remote's, the exact thing Dynamic Federation was chosen to avoid.
  - Fix: mount remotes only via `loadRemoteModule`; per [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/adr/embedding-mechanism.md|embedding-mechanism]].
- The host never depends on an embeddable app's internal implementation — only on `@platform/contracts` and the `remoteEntry` contract.
  - Risk: importing a remote's own type breaks the moment that team changes it and defeats the shared-contract design.
  - Fix: every cross-boundary type/event/service interface is defined in `@platform/contracts`.
# Unittest TestCases

- [ ] WHEN a new embeddable app is added to the runtime remote registry without a platform rebuild THEN
  - [ ] the platform can load and mount it without redeploying
- [ ] WHEN two remotes both depend on `@platform/contracts` at compatible versions THEN
  - [ ] only one instance of the contracts package (and one Angular instance) is loaded in the browser
- [ ] WHEN a remote is built against an incompatible major version of `@platform/contracts` THEN
  - [ ] the federation runtime surfaces a version-mismatch error instead of silently loading two incompatible instances

## SHOULD
- **Hardcoding a remote's URL or version into the host's source** — Consequence: the platform must be rebuilt and redeployed every time an embeddable app ships a new version, defeating independent deployability — Instead: resolve remotes from a runtime configuration/manifest (Dynamic Federation), refreshed independently of the host's own deploys
- **Letting the host import a type from an embeddable app's own package instead of from `@platform/contracts`** — Consequence: host becomes coupled to one team's internal types, breaking the moment that team changes them, and defeats the point of a shared, independently versioned contract — Instead: any cross-boundary type, event, or shared service interface must be defined in `@platform/contracts`
