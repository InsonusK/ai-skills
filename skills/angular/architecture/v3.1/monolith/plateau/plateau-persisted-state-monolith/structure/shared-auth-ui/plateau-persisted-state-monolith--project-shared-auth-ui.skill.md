---
name: plateau-persisted-state-monolith--project-shared-auth-ui
description: The shared auth primitives lib — the *hasPermission directive, the requirePermission guard factory, and the login form + forbidden page — reads the auth slice, never imports a feature — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing libs/shared/auth-ui — the *hasPermission directive, the requirePermission guard factory, the login / forbidden pages
plateau: persisted-state-monolith
project_kind: library
version: 20260903190000
tags:
  - skill/template/project
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"

> NEW at this plateau (VP7). Tagged `type:store`, `scope:shared` — its directive/guard read the `auth` slice (`selectPermissions`) directly. The V1 `solution-authentication` Repository.extend tags it `type:util`; that cannot hold — a `type:util` lib may not depend on `type:store`. See the [example README](../../plateau-persisted-state-monolith.skill/example/README.md).

# Goal

- Keep permission-based UI controls (`*hasPermission`, `requirePermission`) and shared auth pages in one reusable lib, so every feature imports them without re-implementing auth logic
- Never import a feature lib — this stays a leaf of the auth graph

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create.md|shared-auth-ui.project.create]]

# Structure

## Project Structure

```
/libs/shared/auth-ui
  /src
    /lib
      has-permission.directive.ts     <- *hasPermission (structural; permission string, never a role)
      permission.guard.ts             <- requirePermission(perm) → CanActivateFn & CanMatchFn
      login-form.component.ts         <- Signal Forms login + sign-out
      forbidden-page.component.ts     <- the /forbidden route target
      *.spec.ts
    index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| has-permission.directive.ts | `<x *hasPermission="'orders.delete'">` — shows/hides by permission. UI convenience only. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-auth-ui/classes/plateau-persisted-state-monolith--class-has-permission-directive.skill.md\|class-has-permission-directive]] |
| permission.guard.ts | `requirePermission('orders.archive')` — a functional guard attached at the FEATURE's own route, redirecting to `/forbidden`. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/structure/shared-auth-ui/classes/plateau-persisted-state-monolith--class-permission-guard.skill.md\|class-permission-guard]] |
| login-form.component.ts / forbidden-page.component.ts | The shared auth pages, lazily mounted at `/login` and `/forbidden` by the shell. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create.md|shared-auth-ui.project.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create.md|UI/has-permission.directive.ts.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the Angular major version in use | `Store.selectSignal(selectPermissions)` in the directive/guard |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create.md|shared-auth-ui.project.create]]

## Allowed Dependencies

- `libs/shared/state` (tag: `type:store`, `scope:shared`) — the `auth` slice's `selectPermissions` / `AuthActions`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Repository.extend.md|Repository.extend]]

## What Does NOT Belong Here

- Any `import` from a `type:feature` or `type:data-access` project
- Business features or page components — only reusable auth UI primitives
- The `auth` slice itself — that lives in `libs/shared/state/src/lib/auth/`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create.md|shared-auth-ui.project.create]]

# Rules

## MUST
- Every authorization check in the directive or guard must be a permission **string**, never a role name.
- `requirePermission(...)` must be attached inside the feature's own routes, at the specific path it protects — never in `apps/platform-shell`'s `app.routes.ts`.
- A failed guard check must redirect to `/forbidden`, not silently fail navigation.
- This lib must never `import` a `type:feature` / `type:data-access` project.
- `index.ts` exports only the directive, the guard factory, and the two auth pages.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create.md|shared-auth-ui.project.create]]

# Check list

- [ ] `libs/shared/auth-ui` is a usable shared library, `type:store` / `scope:shared`
- [ ] `index.ts` exports only `HasPermissionDirective`, `requirePermission`, `LoginFormComponent`, `ForbiddenPageComponent`
- [ ] No feature-specific imports
- [ ] Every check is a permission string, never a role

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create.md|shared-auth-ui.project.create]]

# Unittest TestCases

- [ ] WHEN the user has the permission THEN `*hasPermission` renders the template; otherwise it clears the view
- [ ] WHEN a user with the permission hits a `requirePermission`-guarded route THEN navigation proceeds; without it, it redirects to `/forbidden`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|Routing/{feature}.guard.ts.create]]
