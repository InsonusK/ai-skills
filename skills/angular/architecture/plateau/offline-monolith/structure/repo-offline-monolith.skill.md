---
name: repo-offline-monolith
description: Nx workspace layout for the offline-monolith plateau — async-monolith plus a durable, per-feature-partitioned mutation queue that replays automatically once connectivity is restored, making the application a full offline-capable PWA for both reads and writes
domain: skill
type: template
plateau: offline-monolith
version: 20260711200000
tags:
  - skill/template/repo
  - plateau/offline-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

> Third plateau in the main application's chain. Parent: [[skills/angular/architecture/plateau/async-monolith/plateau-async-monolith.skill.md|async-monolith]]. Next: [[skills/angular/architecture/plateau/platform-monolith/plateau-platform-monolith.skill.md|platform-monolith]]. This is the **"offline-monolith"** milestone: a mutation attempted while genuinely offline is no longer an immediate failure — it is durably queued, per feature, and replayed automatically once connectivity returns. This is the full offline-capable PWA. Still no authentication, no Module Federation, no backend log delivery.

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
    /[offline-sync](./shared-offline-sync/project-shared-offline-sync.skill.md)      <- new (solution-offline-sync)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)
```

- `libs/shared/offline-sync` is the only new top-level project at this plateau — everything else already existed at `async-monolith` and is extended here: each feature's Facade gains the queueing decision, `shared/ui` gains a pending-sync indicator.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | Composition root — unchanged from `async-monolith`. |
| /apps/platform-shell-e2e | [[platform-shell-e2e/project-platform-shell-e2e.skill.md\|project-platform-shell-e2e.skill]] | Playwright end-to-end scenario specs, now including an offline-write-then-sync scenario. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI, now including the pending-sync indicator alongside the offline banner. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store — unchanged from `async-monolith`. |
| /libs/shared/http-core | [[shared-http-core/project-shared-http-core.skill.md\|project-shared-http-core.skill]] | Base HTTP service and the shared `OfflineTransportError` type — unchanged from `async-monolith`. |
| /libs/shared/logging | [[shared-logging/project-shared-logging.skill.md\|project-shared-logging.skill]] | `LoggerService`, console-only — unchanged. |
| /libs/shared/offline-sync | [[shared-offline-sync/project-shared-offline-sync.skill.md\|project-shared-offline-sync.skill]] | Durable, per-feature-partitioned mutation queue and replay orchestrator. |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template — unchanged from `async-monolith`. |
| /libs/{feature}/data-access | [[feature-data-access/project-feature-data-access.skill.md\|project-feature-data-access.skill]] | Generic template: Facade/Client/Mapper/Errors, the Facade now able to queue a mutation for later sync. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

## Nx Tag Taxonomy

Unchanged axes from `online-monolith`: `type` ∈ {`app`, `e2e`, `feature`, `data-access`, `ui`, `util`, `store`}, `scope` ∈ {`platform`, `shared`, `{feature-name}`}.

`@nx/enforce-module-boundaries` allow-list addition:

| type | may depend on |
| ----- | -------------- |
| `util` (scope:shared) | nothing (leaf) — except `libs/shared/offline-sync`, which is tagged `type:util` but may additionally depend on `libs/shared/state` (to read the `connectivity` slice) |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

## Cross-cutting conventions

These rules apply inside every project in the workspace and have no single project of their own to live in:

- **Three-tier state placement**, **hierarchical route ownership**, **Facade/Client/Mapper layering**, **single logging seam**, **selective preloading**, **bundle budgets**, **offline-aware reads**, **offline-aware transport errors** — unchanged from `async-monolith`.
- **Offline-aware mutations**: a Facade explicitly opts a mutation into the offline queue by catching `OfflineTransportError` and calling `MutationQueueService.enqueue(...)`; queueing is never automatic for every method.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/DataAccess/{feature}.facade.ts.extend|DataAccess/{feature}.facade.ts.extend]]

# Rules

## MUST
- Every queued mutation MUST carry a client-generated idempotency key, reused unchanged across every replay attempt, and MUST be partitioned by the feature that created it.
- Replay MUST process all feature partitions concurrently, and entries within one partition strictly FIFO.
- All other rules from [[skills/angular/architecture/plateau/async-monolith/plateau-async-monolith.skill.md|async-monolith]] continue to apply unchanged.

## MUST NOT
- A Facade MUST NOT enqueue an operation whose business validation already failed before the Client was ever called.
- This project MUST NOT implement per-operation or per-field custom conflict logic beyond the shared server-wins default.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **A Signal Store method calling the feature's Client directly, skipping the Facade**
  - Consequence: bypasses business-rule validation and the queueing decision the Facade owns
  - Instead: the store always goes through the Facade
- **Enqueueing every `OfflineTransportError` unconditionally**
  - Consequence: some operations (one-time, time-sensitive) queued and replayed later can produce a confusing or wrong result
  - Instead: each Facade explicitly decides which of its operations are queueable

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN two features both have pending mutations and one feature's replay fails repeatedly THEN
  - [ ] the other feature's partition still completes its replay successfully
- [ ] WHEN a queued mutation is replayed after connectivity is restored THEN
  - [ ] it is sent with the same idempotency key it was enqueued with

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/Repository.extend|Repository.extend]]
