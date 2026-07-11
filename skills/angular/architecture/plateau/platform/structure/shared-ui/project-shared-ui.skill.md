---
name: project-shared-ui
description: App-specific reusable UI composed from design-system primitives, including the offline banner and pending-sync indicator
domain: skill
type: template
plateau: platform
project_kind: library
version: 20260711150000
tags:
  - skill/template/project
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

# Goal

- Host reusable, app-specific UI composed out of [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] primitives (now consumed as a version-negotiated federation singleton — see [[../platform-shell/project-platform-shell.skill.md|project-platform-shell]]), including connectivity/offline-sync UI feedback

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Structure

## Project Structure

```
/libs/shared/ui
  /src
    /lib
      [offline-banner.component.ts](./classes/class-offline-banner-component.skill.md)
      [pending-sync-indicator.component.ts](./classes/class-pending-sync-indicator-component.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| offline-banner.component.ts | Persistent banner informing the user the app is offline | [[classes/class-offline-banner-component.skill.md\|class-offline-banner-component.skill]] |
| pending-sync-indicator.component.ts | Per-feature count of mutations queued and waiting to sync | [[classes/class-pending-sync-indicator-component.skill.md\|class-pending-sync-indicator-component.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]

## What Does NOT Belong Here

- Feature-specific business logic
- The design-system components themselves (consumed as the `design-system` npm package/federation singleton, not reimplemented here)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[../repo-platform.skill.md#MUST|repo-platform]]

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
