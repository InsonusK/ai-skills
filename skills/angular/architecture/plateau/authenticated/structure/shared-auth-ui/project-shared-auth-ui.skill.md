---
name: project-shared-auth-ui
description: Shared library for authentication-related UI primitives — currently the has-permission directive, with room for future auth UI components
domain: skill
type: template
plateau: authenticated
project_kind: library
version: 20260711150000
tags:
  - skill/template/project
  - plateau/authenticated
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

# Goal

- Keep permission-based UI controls in a single reusable library so every feature can import them without duplicating auth logic

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Structure

## Project Structure

```
/libs/shared/auth-ui
  /src
    /lib
      [has-permission.directive.ts](./classes/class-has-permission-directive.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| has-permission.directive.ts | Structural directive `*hasPermission` that shows/hides UI based on the current user's permissions. | [[classes/class-has-permission-directive.skill.md\|class-has-permission-directive.skill]] |
| index.ts | Re-exports `HasPermissionDirective` as the public API of this library. | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the NgRx major version in use | `Store` injection used by `has-permission.directive.ts` to read permissions |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

## What Does NOT Belong Here

- Business features or page components — only reusable UI primitives
- The permission model's source of truth — this library only reads `libs/shared/state`'s auth slice, it never owns permission state itself

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

## Allowed Dependencies

- `libs/shared/state` (tag: `type:store`, `scope:shared`) — auth slice only

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Rules

## MUST
- The library MUST NOT depend on feature libraries — only on `libs/shared/state` (auth slice) and core Angular/NgRx packages.
- `HasPermissionDirective` MUST be the only auth UI primitive exported from `libs/shared/auth-ui` until a future solution explicitly adds more.

## MUST NOT
- The library MUST NOT contain business features or page components — only reusable UI primitives.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Anti-patterns

- **Inlining the permission check into every component instead of using the directive**
  - Consequence: permission logic spreads across the codebase and becomes inconsistent
  - Instead: use `*hasPermission` from `libs/shared/auth-ui`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Check list

- [ ] `libs/shared/auth-ui` is created as a publishable/usable shared library
- [ ] `index.ts` exports only `HasPermissionDirective`
- [ ] No feature-specific imports exist inside `libs/shared/auth-ui`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Unittest TestCases

- [ ] WHEN the current user has the required permission THEN `*hasPermission` renders the template
- [ ] WHEN the current user lacks the required permission THEN `*hasPermission` clears the view

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]
