---
name: plateau-monitored-app--project-shared-util
description: Framework-agnostic pure helpers shared across features — monitored-app plateau
domain: skill
type: template
plateau: monitored-app
project_kind: library
version: 20260711220000
tags:
  - skill/template/project
  - plateau/monitored-app
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
---

> No solution produced a dedicated `shared-util.project.create.md` file — this project is established implicitly by [[skills/angular/architecture/plateau/plateau-monitored-app/structure/plateau-monitored-app--repo-monitored-app.skill.md|repo-monitored-app]]'s `Repository.create` entry. Unchanged by every solution applied since.

# Goal

- Host framework-agnostic pure helpers (pure functions, RxJS operators, mapping utilities) shared across features

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

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
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST NOT
- MUST NOT add a `type:util` project with any `scope:*` other than `shared`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
