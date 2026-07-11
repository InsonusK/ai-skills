---
name: project-platform-shell
description: The single deployable Angular application — composition root, top-level routing, root providers
domain: skill
type: template
plateau: foundation
project_kind: application
version: 20260711120000
tags:
  - skill/template/project
  - plateau/foundation
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
---

> No solution produced a dedicated `platform-shell.project.create.md` file at this plateau — this project is established implicitly by [[../repo-foundation.skill.md|repo-foundation]]'s `Repository.create` directory table. This skill exists so the project has a consistent home for its own content as later plateaus extend it.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      app.config.ts
      app.routes.ts
    main.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| app.config.ts | Root provider registration | — |
| app.routes.ts | Top-level route table (populated once routing exists) | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[../repo-foundation.skill.md#MUST|repo-foundation]]

## MUST NOT
- [[../repo-foundation.skill.md#MUST NOT|repo-foundation]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
