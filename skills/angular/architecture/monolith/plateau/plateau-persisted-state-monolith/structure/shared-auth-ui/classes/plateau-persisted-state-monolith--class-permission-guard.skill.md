---
name: plateau-persisted-state-monolith--class-permission-guard
description: The requirePermission functional guard factory in libs/shared/auth-ui — checks a permission string against the auth slice, attached at a feature's own route, redirecting to /forbidden — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing the requirePermission guard factory (VP7), or attaching it at a feature route
plateau: persisted-state-monolith
artifact_type: guard
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"

> `libs/shared/auth-ui/src/lib/permission.guard.ts`. The factory is shared; the **attachment** is per-feature — inside `{feature}.routes.ts`, never in the shell. This is where the auth guards `solution-app-routing` deferred are formally defined.

# Goal

- Restrict navigation into a route by a required permission, reusing the same permission model as `*hasPermission`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|Routing/{feature}.guard.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- `requirePermission(perm)` returns `CanActivateFn & CanMatchFn` — a factory, evaluated in an injection context
- Checks a permission **string**, never a role name
- On failure returns `router.createUrlTree(['/forbidden'])` — a redirect, not a silent nav failure
- Attached at the feature's own route (hierarchical ownership) — never in `apps/platform-shell`'s root routes

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|Routing/{feature}.guard.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Permission guard factory | `requirePermission` | `requirePermission` | `permission.guard.ts` | `permission.guard.ts` |

# Implementation

```typescript
// Skill: class-permission-guard
// Plateau: persisted-state-monolith
// Version: 20260903190000

export function requirePermission(permission: string): CanActivateFn & CanMatchFn {
  return () => {
    const permissions = inject(Store).selectSignal(selectPermissions)();
    const router = inject(Router);
    return permissions.includes(permission) || router.createUrlTree(['/forbidden']);
  };
}

// {feature}.routes.ts — attached at the feature's OWN route
{ path: 'archive', canActivate: [requirePermission('orders.archive')], loadComponent: () => import('./…') }
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|Routing/{feature}.guard.ts.create]]

# Rules

## MUST
- The guard must check a permission string against the `auth` slice's `permissions`, never a role name.
- The guard must be attached inside the feature's own routes, at the specific path it protects — never centralized in `apps/platform-shell`'s `app.routes.ts`.
- A failed check must redirect to `/forbidden`, not silently fail navigation.
- Never apply several plateau templates per class/artifact.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|Routing/{feature}.guard.ts.create]]

# Check list

- [ ] Every route needing a permission uses `requirePermission(...)` at that feature's own route
- [ ] No permission guard is defined in `apps/platform-shell`'s root routes

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|Routing/{feature}.guard.ts.create]]

# Unittest TestCases

- [ ] WHEN a user with the required permission navigates to the guarded route THEN navigation proceeds (returns `true`)
- [ ] WHEN a user without it navigates THEN it returns a `UrlTree` for `/forbidden`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create.md|Routing/{feature}.guard.ts.create]]
