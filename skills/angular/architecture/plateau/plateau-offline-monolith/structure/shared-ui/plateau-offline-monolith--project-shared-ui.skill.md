---
name: plateau-offline-monolith--project-shared-ui
description: App-specific reusable UI composed from design-system primitives — the offline banner plus a per-feature pending-sync indicator — offline-monolith plateau
domain: skill
type: template
plateau: offline-monolith
project_kind: library
version: 20260711200000
tags:
  - skill/template/project
  - plateau/offline-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

# Goal

- Host reusable, app-specific UI composed out of [[skills/angular/architecture/plateau/plateau-design-system/plateau-design-system.skill|design-system]] primitives, including offline read and write feedback

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Structure

## Project Structure

```
/libs/shared/ui
  /src
    /lib
      [offline-banner.component.ts](./classes/plateau-offline-monolith--class-offline-banner-component.skill.md)
      [pending-sync-indicator.component.ts](./classes/plateau-offline-monolith--class-pending-sync-indicator-component.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| offline-banner.component.ts | Persistent banner informing the user the app is offline | [[skills/angular/architecture/plateau/plateau-offline-monolith/structure/shared-ui/classes/plateau-offline-monolith--class-offline-banner-component.skill\|class-offline-banner-component]] |
| pending-sync-indicator.component.ts | Reactively shows how many actions are queued and waiting to sync for a given feature | [[skills/angular/architecture/plateau/plateau-offline-monolith/structure/shared-ui/classes/plateau-offline-monolith--class-pending-sync-indicator-component.skill\|class-pending-sync-indicator-component]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]

## What Does NOT Belong Here

- Feature-specific business logic
- The design-system components themselves (consumed as the `design-system` npm package, not reimplemented here)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[skills/angular/architecture/plateau/plateau-offline-monolith/structure/plateau-offline-monolith--repo-offline-monolith.skill#MUST|repo-offline-monolith]]

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
