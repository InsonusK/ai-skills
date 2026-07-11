---
name: project-shared-auth-ui
description: Shared library for authentication-related UI primitives — permission-checking directive and route-guard factory
domain: skill
type: template
plateau: offline-app
project_kind: library
version: 20260711140000
tags:
  - skill/template/project
  - plateau/offline-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

# Goal

- Keep permission-based UI/routing controls in a single reusable library so every feature can import them without duplicating auth logic

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Core Principles

- Every authorization check is expressed as a permission string, never a role name
- Hiding UI with the directive is never a substitute for a server-side authorization check

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Structure

## Project Structure

```
/libs/shared/auth-ui
  /src
    /lib
      [has-permission.directive.ts](./classes/class-has-permission-directive.skill.md)
      [permission.guard.ts](./classes/class-permission-guard.skill.md)
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| has-permission.directive.ts | Structural directive `*hasPermission` that shows/hides UI based on the current user's permissions | [[classes/class-has-permission-directive.skill.md\|class-has-permission-directive.skill]] |
| permission.guard.ts | `requirePermission(...)` functional route-guard factory, attached inside each feature's own routes | [[classes/class-permission-guard.skill.md\|class-permission-guard.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the NgRx major version in use | `Store` injection used to read permissions |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

## What Does NOT Belong Here

- Business features or page components — only reusable auth UI/routing primitives

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

## Allowed Dependencies

- `libs/shared/state` (auth slice, tag: `type:store`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]

# Rules

## MUST
- The library MUST NOT depend on feature libraries — only on `libs/shared/state` (auth slice) and core Angular/NgRx packages.
- Any code here that checks "is the user allowed to do X" MUST express the check as a permission string, never a role name.

## MUST NOT
- The library MUST NOT contain business features or page components.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Inlining the permission check into every component instead of using the directive**
  - Consequence: permission logic spreads across the codebase and becomes inconsistent
  - Instead: use `*hasPermission` from `libs/shared/auth-ui`
- **Centralizing all permission guards in the shell's root routes "for visibility"**
  - Consequence: reintroduces the coupling hierarchical route ownership was designed to prevent
  - Instead: each feature attaches its own guards to its own routes, using `requirePermission`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Check list

- [ ] `libs/shared/auth-ui` is created as a shared library
- [ ] `index.ts` exports `HasPermissionDirective` and `requirePermission`
- [ ] No feature-specific imports exist inside `libs/shared/auth-ui`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]
