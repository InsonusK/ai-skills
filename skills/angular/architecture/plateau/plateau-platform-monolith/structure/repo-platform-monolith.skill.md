---
name: repo-platform-monolith
description: Nx workspace layout for the platform-monolith plateau — offline-monolith turned into a Native Federation dynamic host, able to load independently deployed embeddable apps at runtime and share the design system as a version-negotiated singleton
domain: skill
type: template
plateau: platform-monolith
version: 20260711210000
tags:
  - skill/template/repo
  - plateau/platform-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]]"
---

> Fourth plateau in the main application's chain. Parent: [[skills/angular/architecture/plateau/plateau-offline-monolith/plateau-offline-monolith|offline-monolith]]. Next: [[skills/angular/architecture/plateau/plateau-monitored-app/plateau-monitored-app|monitored-app]]. This is the **"platform-monolith"** milestone: the monolith becomes a platform — `apps/platform-shell` is a Native Federation dynamic host that discovers and mounts independently built and deployed embeddable apps at runtime, sharing a single Angular runtime, `@platform/contracts` instance, and (when version-compatible) the design system. See the sibling [[skills/angular/architecture/plateau/plateau-embeddable-app/plateau-embeddable-app|embeddable-app]] plateau for what an embeddable app repository itself must look like. Still no authentication (that arrives at [[skills/angular/architecture/plateau/plateau-multiuser-app/plateau-multiuser-app|multiuser-app]], the last plateau — the platform host has nothing to share via `SessionContract` until then), no backend log delivery.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)
  /[platform-shell-e2e](./platform-shell-e2e/project-platform-shell-e2e.skill.md)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)
    /[http-core](./shared-http-core/project-shared-http-core.skill.md)
    /[logging](./shared-logging/project-shared-logging.skill.md)
    /[offline-sync](./shared-offline-sync/project-shared-offline-sync.skill.md)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)
```

- No new top-level project inside this monorepo — federation host capability is added entirely inside `apps/platform-shell`. What's new is external to this repo: any number of independently repositoried, independently deployed embeddable apps, each conforming to the [[skills/angular/architecture/plateau/plateau-embeddable-app/plateau-embeddable-app|embeddable-app]] plateau.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/platform-shell/project-platform-shell.skill\|project-platform-shell]] | Composition root, now also a Native Federation dynamic host: runtime remote registry, `loadRemoteModule` mounting, service-worker caching extended to federated remote chunks. |
| /apps/platform-shell-e2e | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/platform-shell-e2e/project-platform-shell-e2e.skill\|project-platform-shell-e2e]] | Playwright end-to-end scenario specs — unchanged from `offline-monolith`. |
| /libs/shared/ui | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/shared-ui/project-shared-ui.skill\|project-shared-ui]] | Reusable, app-specific UI — unchanged from `offline-monolith`. |
| /libs/shared/util | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/shared-util/project-shared-util.skill\|project-shared-util]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/shared-state/project-shared-state.skill\|project-shared-state]] | Classical NgRx Store — unchanged. |
| /libs/shared/http-core | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/shared-http-core/project-shared-http-core.skill\|project-shared-http-core]] | Base HTTP service and `OfflineTransportError` — unchanged. |
| /libs/shared/logging | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/shared-logging/project-shared-logging.skill\|project-shared-logging]] | `LoggerService`, console-only — unchanged. |
| /libs/shared/offline-sync | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/shared-offline-sync/project-shared-offline-sync.skill\|project-shared-offline-sync]] | Mutation queue and replay orchestrator — unchanged. |
| /libs/{feature}/feature | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/feature-feature/project-feature-feature.skill\|project-feature-feature]] | Generic template — unchanged. |
| /libs/{feature}/data-access | [[skills/angular/architecture/plateau/plateau-platform-monolith/structure/feature-data-access/project-feature-data-access.skill\|project-feature-data-access]] | Generic template — unchanged. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## Nx Tag Taxonomy

Unchanged axes from `online-monolith`: `type` ∈ {`app`, `e2e`, `feature`, `data-access`, `ui`, `util`, `store`}, `scope` ∈ {`platform`, `shared`, `{feature-name}`}, plus one addition:

| Axis | Values | Meaning |
| ----- | ------- | ------- |
| `type` (addition) | `host` | Marks `apps/platform-shell` as a Native Federation dynamic host, in addition to its existing `type:app` tag |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]

## Cross-cutting conventions

These rules apply inside every project in the workspace and have no single project of their own to live in:

- **Three-tier state placement**, **hierarchical route ownership**, **Facade/Client/Mapper layering**, **single logging seam**, **selective preloading**, **bundle budgets**, **offline-aware reads/mutations** — unchanged from `offline-monolith`.
- **Federation boundary**: the host never bundles a specific embeddable app's code at build time, and never imports an embeddable app's internal implementation — only `@platform/contracts` and the federation `remoteEntry` contract. Remote discovery is a runtime concern (Dynamic Federation), resolved from configuration, never compiled in.
- **Design-system version negotiation**: the platform host and every embeddable app declare `requiredVersion` ranges for the design system as a version-negotiated singleton (`singleton: true`, `strictVersion: false`) — sharing the platform's instance when ranges are compatible, isolating otherwise.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]

# Rules

## MUST
- `apps/platform-shell` MUST declare the `type:host` tag in addition to its existing `type:app`/`scope:platform` tags.
- `apps/platform-shell` MUST mark `@platform/contracts` (and Angular) as `singleton: true` shared dependencies.
- The list of available remotes (embeddable apps) and their URLs MUST be resolved at runtime (Dynamic Federation manifest), never hardcoded into the host's build output.
- The design system MUST be shared as a version-negotiated singleton between the host and every embeddable app, per each side's declared `requiredVersion` range.
- All other rules from [[skills/angular/architecture/plateau/plateau-offline-monolith/plateau-offline-monolith|offline-monolith]] continue to apply unchanged.

## MUST NOT
- The host MUST NOT bundle a specific embeddable app's code at build time.
- The host MUST NOT depend on an embeddable app's internal implementation — only on `@platform/contracts` and the federation `remoteEntry` contract.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]

# Anti-patterns

- **Hardcoding a remote's URL or version into the host's source**
  - Consequence: the platform must be rebuilt and redeployed every time an embeddable app ships a new version, defeating independent deployability
  - Instead: resolve remotes from a runtime configuration/manifest, refreshed independently of the host's own deploys
- **Letting the host import a type from an embeddable app's own package instead of from `@platform/contracts`**
  - Consequence: host becomes coupled to one team's internal types, breaking the moment that team changes them
  - Instead: any cross-boundary type, event, or shared service interface must be defined in `@platform/contracts`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]

# Unittest TestCases

- [ ] WHEN a new embeddable app is added to the runtime remote registry without a platform rebuild THEN
  - [ ] the platform can load and mount it without redeploying
- [ ] WHEN two remotes both depend on `@platform/contracts` at compatible versions THEN
  - [ ] only one instance of the contracts package (and one Angular instance) is loaded in the browser
- [ ] WHEN an embeddable app's `requiredVersion` for the design system matches the platform's loaded version THEN
  - [ ] no separate design-system bundle is fetched — the shared instance is used

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/Repository.extend|PlatformHost/Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/PlatformHost/platform-shell.federation.extend|PlatformHost/platform-shell.federation.extend]]
