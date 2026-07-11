---
name: project-shared-ui
description: App-specific reusable UI composed from design-system primitives
domain: skill
type: template
plateau: tested
project_kind: library
version: 20260711170000
tags:
  - skill/template/project
  - plateau/tested
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
---

> No solution produced a dedicated `shared-ui.project.create.md` file — this project is established implicitly by [[../repo-tested.skill.md|repo-tested]]'s `Repository.create` directory table. Unchanged since [[skills/angular/architecture/plateau/foundation/plateau-foundation.skill.md|foundation]].

# Goal

- Host reusable, app-specific UI composed out of [[skills/angular/architecture/plateau/design-system/plateau-design-system.skill.md|design-system]] primitives — not the design system itself, and not feature-specific business logic

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Structure

## Project Structure

```
/libs/shared/ui
  /src
    /lib
    index.ts
```

## What Does NOT Belong Here

- Feature-specific business logic
- The design-system components themselves (consumed as the `design-system` npm package, not reimplemented here)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[../repo-tested.skill.md#MUST|repo-tested]]

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
