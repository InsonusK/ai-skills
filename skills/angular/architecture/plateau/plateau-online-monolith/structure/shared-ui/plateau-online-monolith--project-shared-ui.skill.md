---
name: plateau-online-monolith--project-shared-ui
description: App-specific reusable UI composed from design-system primitives — online-monolith plateau
domain: skill
type: template
plateau: online-monolith
project_kind: library
version: 20260711180000
tags:
  - skill/template/project
  - plateau/online-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
---

> No solution produced a dedicated `shared-ui.project.create.md` file — this project is established implicitly by [[skills/angular/architecture/plateau/plateau-online-monolith/structure/plateau-online-monolith--repo-online-monolith.skill.md|repo-online-monolith]]'s `Repository.create` directory table.

# Goal

- Host reusable, app-specific UI composed out of [[skills/angular/architecture/plateau/plateau-design-system/plateau-design-system.skill.md|design-system]] primitives — not the design system itself, and not feature-specific business logic

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

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
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[skills/angular/architecture/plateau/plateau-online-monolith/structure/plateau-online-monolith--repo-online-monolith.skill#MUST|repo-online-monolith]]

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
