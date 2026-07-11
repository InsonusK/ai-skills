---
name: project-shared-ui
description: App-specific reusable UI composed from design-system primitives, now including the offline banner
domain: skill
type: template
plateau: async-monolith
project_kind: library
version: 20260711190000
tags:
  - skill/template/project
  - plateau/async-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]]"
---

# Goal

- Host reusable, app-specific UI composed out of [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] primitives, including this plateau's offline UI feedback

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Structure

## Project Structure

```
/libs/shared/ui
  /src
    /lib
      [offline-banner.component.ts](./classes/class-offline-banner-component.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| offline-banner.component.ts | Persistent banner informing the user the app is offline | [[classes/class-offline-banner-component.skill.md\|class-offline-banner-component.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]

## What Does NOT Belong Here

- Feature-specific business logic
- The design-system components themselves (consumed as the `design-system` npm package, not reimplemented here)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[../repo-async-monolith.skill.md#MUST|repo-async-monolith]]

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
