---
name: project-shared-util
description: Framework-agnostic pure helpers shared across features
domain: skill
type: template
plateau: online-monolith
project_kind: library
version: 20260711180000
tags:
  - skill/template/project
  - plateau/online-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
---

> No solution produced a dedicated `shared-util.project.create.md` file — this project is established implicitly by [[../repo-online-monolith.skill.md|repo-online-monolith]]'s `Repository.create` directory table.

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
- MUST NOT add a `type:util` project with any `scope:*` other than `shared` — a feature-specific helper belongs inside that feature's own lib.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
