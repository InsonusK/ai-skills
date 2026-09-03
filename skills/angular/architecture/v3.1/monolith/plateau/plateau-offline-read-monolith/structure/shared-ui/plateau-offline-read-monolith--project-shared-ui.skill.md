---
name: plateau-offline-read-monolith--project-shared-ui
description: App-specific reusable UI composed from design-system primitives — plus the shell-mounted OfflineBannerComponent (VP4) — offline-read-monolith plateau
domain: skill
type: template
whenToUse: when adding or editing a presentational component in libs/shared/ui, or checking it stays free of a store/data-access dependency
plateau: offline-read-monolith
project_kind: library
version: 20260903090000
tags:
  - skill/template/project
  - plateau/offline-read-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"

> The bare project is established implicitly by [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/plateau-offline-read-monolith--repo-offline-read-monolith.skill.md|repo-offline-read-monolith]]'s `Repository.create` directory table. `solution-offline-first` (VP4) adds `OfflineBannerComponent` — a presentational component the shell feeds with `isOnline` from the `connectivity` slice.

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
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| /offline-banner | The shared offline indicator. Presentational (`isOnline` input) — the shell owns the store wiring and mounts it once. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/shared-ui/classes/plateau-offline-read-monolith--class-offline-banner-component.skill.md\|class-offline-banner-component]] |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]

## What Does NOT Belong Here

- Feature-specific business logic
- The design-system components themselves (consumed as the `design-system` npm package, not reimplemented here)
- Store wiring inside a component — `OfflineBannerComponent` takes `isOnline` as an input; the shell injects `Store` and passes `selectIsOnline`, keeping this lib free of a `type:store` dependency

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Rules

## MUST
- [[skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-read-monolith/structure/plateau-offline-read-monolith--repo-offline-read-monolith.skill#must|repo-offline-read-monolith]]
- `OfflineBannerComponent` must read connectivity from its `isOnline` input only — never `navigator.onLine`, never the store directly — and must be mounted exactly once, at the shell.
- A feature must never build its own local offline indicator — reuse this component.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]
