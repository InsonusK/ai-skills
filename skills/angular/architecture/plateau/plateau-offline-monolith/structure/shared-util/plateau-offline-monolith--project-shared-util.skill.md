---
name: plateau-offline-monolith--project-shared-util
description: Framework-agnostic pure helpers shared across features — offline-monolith plateau
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
---

> No solution produced a dedicated `shared-util.project.create.md` file — this project is established implicitly by [[skills/angular/architecture/plateau/plateau-offline-monolith/structure/plateau-offline-monolith--repo-offline-monolith.skill|repo-offline-monolith]]'s `Repository.create` entry. Unchanged by every solution applied since.

# Goal

- Host framework-agnostic pure helpers (pure functions, RxJS operators, mapping utilities) shared across features

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Structure

## Project Structure

```
/libs/shared/util
  /src
    /lib
    index.ts
```

## What Does NOT Belong Here

- Angular DI, HTTP calls, state — this lib is pure and framework-agnostic

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST NOT
- MUST NOT add a `type:util` project with any `scope:*` other than `shared`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
