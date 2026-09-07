---
name: plateau-online-monolith--project-shared-util
description: Framework-agnostic pure helpers shared across features — online-monolith plateau
domain: skill
type: template
whenToUse: when adding a framework-agnostic helper to libs/shared/util, or checking it has no Angular/RxJS import
plateau: online-monolith
project_kind: library
version: 20260902000000
tags:
  - skill/template/project
  - plateau/online-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"

> No solution produced a dedicated `shared-util.project.create.md` file — this project is established implicitly by [[skills/angular/architecture/v3.1/monolith/plateau/plateau-online-monolith/structure/plateau-online-monolith--repo-online-monolith.skill.md|repo-online-monolith]]'s `Repository.create` directory table.

# Goal

- Host framework-agnostic pure helpers (pure functions, RxJS operators, mapping utilities) shared across features

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]

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
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Rules

- MUST NOT add a `type:util` project with any `scope:*` other than `shared` — a feature-specific helper belongs inside that feature's own lib.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
