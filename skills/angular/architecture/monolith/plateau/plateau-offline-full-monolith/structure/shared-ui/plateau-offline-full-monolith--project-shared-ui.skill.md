---
name: plateau-offline-full-monolith--project-shared-ui
description: App-specific reusable UI composed from design-system primitives — plus the shell-mounted OfflineBannerComponent (VP4) — offline-full-monolith plateau
domain: skill
type: template
whenToUse: when adding or editing a presentational component in libs/shared/ui, or checking it stays free of a store/data-access dependency
plateau: offline-full-monolith
project_kind: library
version: 20260903120000
tags:
  - skill/template/project
  - plateau/offline-full-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"

> The bare project is established implicitly by [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/plateau-offline-full-monolith--repo-offline-full-monolith.skill.md|repo-offline-full-monolith]]'s `Repository.create` directory table. VP4 adds `OfflineBannerComponent` (`isOnline` input, shell-mounted); VP5 adds `PendingSyncIndicatorComponent` (`count` input, mounted per feature). Both presentational — the owning host wires the store/queue.

# Goal

- Host reusable, app-specific UI composed out of [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md|design-system]] primitives — not the design system itself, and not feature-specific business logic

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Structure

## Project Structure

```
/libs/shared/ui
  /src
    /lib
      /status-badge
      /offline-banner              <- new (VP4 / solution-offline-first)
        offline-banner.component.ts       <- presentational; input.required<boolean>('isOnline')
        offline-banner.component.spec.ts
      /pending-sync-indicator     <- new (VP5 / solution-offline-sync)
        pending-sync-indicator.component.ts   <- presentational; input.required<number>('count')
        pending-sync-indicator.component.spec.ts
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /offline-banner | The shared offline indicator. Presentational (`isOnline` input) — the shell owns the store wiring and mounts it once. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-ui/classes/plateau-offline-full-monolith--class-offline-banner-component.skill.md\|class-offline-banner-component]] |
| /pending-sync-indicator | The shared "N actions waiting to sync" indicator. Presentational (`count` input) — the owning feature feeds it from `MutationQueueService.pendingForFeature$`. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-ui/classes/plateau-offline-full-monolith--class-pending-sync-indicator-component.skill.md\|class-pending-sync-indicator-component]] |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create.md|UI/pending-sync-indicator.component.ts.create]]

## What Does NOT Belong Here

- Feature-specific business logic
- The design-system components themselves (consumed as the `design-system` npm package, not reimplemented here)
- Store wiring inside a component — `OfflineBannerComponent` takes `isOnline` as an input; the shell injects `Store` and passes `selectIsOnline`, keeping this lib free of a `type:store` dependency

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Rules

## MUST
- [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/plateau-offline-full-monolith--repo-offline-full-monolith.skill#must|repo-offline-full-monolith]]
- `OfflineBannerComponent` must read connectivity from its `isOnline` input only — never `navigator.onLine`, never the store directly — and must be mounted exactly once, at the shell.
- A feature must never build its own local offline indicator — reuse this component.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]
