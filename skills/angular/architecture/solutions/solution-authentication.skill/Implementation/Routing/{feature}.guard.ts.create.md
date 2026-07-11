---
description: Generic functional route guard pattern checking a required permission against the auth slice's permission set — this is where auth guards, deferred from the App routing solution, are formally defined
project_name: "{Feature}"
name: "{feature}"
element_kind: guard
change_kind: create
---

# How this generic file is used
This is not tied to one concrete feature. Any feature that needs to restrict access to one of its own root-relative routes (see the "App routing (база)" solution's hierarchical route ownership) attaches a guard following this pattern, at the point where that route is defined — inside the feature's own routes, consistent with the feature owning its own root-relative paths.

# Goals

- Restrict navigation into a route based on a required permission, reusing the same permission model as UI-level visibility checks

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------------- | -------------------- | --------- |
| Permission guard factory | requirePermission | requirePermission | permission.guard.ts | permission.guard.ts |

# Implementation changes

```typescript
// permission.guard.ts (shared, in libs/shared/auth-ui, imported by any feature)
export function requirePermission(permission: string): CanActivateFn & CanMatchFn {
  return () => {
    const store = inject(Store);
    const permissions = store.selectSignal(selectPermissions)();
    const router = inject(Router);
    return permissions.includes(permission) || router.createUrlTree(['/forbidden']);
  };
}
```

```typescript
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

# Rule changes

## MUST
- The guard MUST check a permission string against the `shared-state` auth slice's `permissions`, never a role name (see [[skills/angular/architecture/solutions/solution-authentication.skill/adr/authorization-model|authorization-model ADR]]).
- The guard MUST be attached inside the feature's own routes, at the specific path it protects — never centralized in `apps/platform-shell`'s `app.routes.ts`, consistent with the hierarchical route ownership from the "App routing (база)" solution.
- A failed permission check MUST redirect to a forbidden/not-authorized route, not silently fail navigation.

# Anti-patterns

- **Centralizing all permission guards in the shell's root routes "for visibility"**
  - Consequence: reintroduces the coupling the "App routing (база)" solution's hierarchical ownership was designed to prevent — the shell would need to know which specific feature paths require which permissions
  - Instead: each feature attaches its own guards to its own routes, using the shared `requirePermission` factory

# Check list

- [ ] Every route requiring a specific permission uses `requirePermission(...)` at that feature's own route definition
- [ ] No permission guard is defined inside `apps/platform-shell`'s root routes

# Unittest TestCases

- [ ] WHEN a user with the required permission navigates to the guarded route THEN
  - [ ] navigation proceeds
- [ ] WHEN a user without the required permission navigates to the guarded route THEN
  - [ ] navigation redirects to the forbidden route
