---
description: Extend the base workspace so apps/platform-shell becomes a Native Federation dynamic host, and add the tags/dependency needed for embeddable apps
element_kind: repository
change_kind: extend
tags:
  - solution/platform-embeddability
  - element/repository
---

# Structure

## Workspace Structure

No new top-level directories are added to the base layout from [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create]]. `apps/platform-shell` is reconfigured, and one new tag is introduced.

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /apps/platform-shell | Gains `type:host` tag in addition to `type:app`, `scope:platform`. Now owns federation host configuration (`federation.config.ts`) and a runtime remote registry (see [[skills/angular/architecture/v3.1/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend]]). |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor, matching the Angular major version in use | Native Federation host/remote runtime and build plugin |
| @platform/contracts | semver range, published from a separate repository (see [[../../adr/embedding-mechanism.md|ADR]]) | Shared, versioned contract for cross-app state/events (EventBus interface, shared DTOs). Marked as a `singleton` shared dependency in the federation config so host and every remote resolve to the exact same runtime instance. |

# Rules

## MUST
- `apps/platform-shell` MUST declare the `type:host` tag in addition to its existing `type:app`/`scope:platform` tags.
- `apps/platform-shell` MUST mark `@platform/contracts` (and Angular itself) as `singleton: true` in its federation shared-dependency configuration.
- The list of available remotes (embeddable apps) and their URLs MUST be resolved at runtime (Dynamic Federation manifest), never hardcoded into the host's build output.

- Never bundle a specific embeddable app's code at build time — that would defeat independent deployability, which is the entire reason Dynamic Federation was selected in [[skills/angular/architecture/v3.1/solutions/solution-platform-embeddability.skill/adr/embedding-mechanism.md|embedding-mechanism]].
- Never depend on an embeddable app's internal implementation — only on the `@platform/contracts` package and the federation `remoteEntry` contract.
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
