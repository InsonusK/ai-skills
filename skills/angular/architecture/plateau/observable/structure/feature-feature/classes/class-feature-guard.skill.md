---
name: class-feature-guard
description: Generic functional route guard pattern checking a required permission against the auth slice's permission set — attached inside the feature's own routes, at the specific path it protects
domain: skill
type: template
plateau: observable
artifact_type: guard
version: 20260711160000
tags:
  - skill/template/class
  - plateau/observable
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

> Generic pattern, not tied to one concrete feature — any feature that needs to restrict access to one of its own root-relative routes attaches a guard following this pattern, at the point where that route is defined. This is where auth guards, deliberately deferred by `solution-app-routing`, are formally defined.

# Goal

- Restrict navigation into a route based on a required permission, reusing the same permission model as UI-level visibility checks

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The guard is attached inside the feature's own routes, at the specific path it protects — never centralized in `apps/platform-shell`, consistent with hierarchical route ownership

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------------- | -------------------- | --------- |
| Permission guard factory | `requirePermission` | `requirePermission` | `permission.guard.ts` | `permission.guard.ts` |

# Implementation

```typescript
// Skill: class-feature-guard
// Plateau: authenticated
// Version: 20260711160000

// permission.guard.ts (shared, in libs/shared/auth-ui, imported by any feature)
export function requirePermission(permission: string): CanActivateFn & CanMatchFn {
  return () => {
    const store = inject(Store);
    const permissions = store.selectSignal(selectPermissions)();
    const router = inject(Router);
    return permissions.includes(permission) || router.createUrlTree(['/forbidden']);
  };
}

// orders.routes.ts — attaching the guard at the feature's own route
export const ORDERS_ROUTES: Routes = [
  { path: '', component: OrdersListComponent },
  {
    path: ':id/delete-confirm',
    canActivate: [requirePermission('orders.delete')],
    component: DeleteConfirmComponent,
  },
];
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Rules

## MUST
- The guard MUST check a permission string against the `shared-state` auth slice's `permissions`, never a role name.
- The guard MUST be attached inside the feature's own routes, at the specific path it protects — never centralized in `apps/platform-shell`'s `app.routes.ts`.
- A failed permission check MUST redirect to a forbidden/not-authorized route, not silently fail navigation.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Centralizing all permission guards in the shell's root routes "for visibility"**
  - Consequence: reintroduces the coupling hierarchical route ownership was designed to prevent — the shell would need to know which specific feature paths require which permissions
  - Instead: each feature attaches its own guards to its own routes, using the shared `requirePermission` factory

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Check list

- [ ] Every route requiring a specific permission uses `requirePermission(...)` at that feature's own route definition
- [ ] No permission guard is defined inside `apps/platform-shell`'s root routes

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Unittest TestCases

- [ ] WHEN a user with the required permission navigates to the guarded route THEN
  - [ ] navigation proceeds
- [ ] WHEN a user without the required permission navigates to the guarded route THEN
  - [ ] navigation redirects to the forbidden route

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]
