---
name: plateau-multiuser-app--class-permission-guard
description: Shared functional route guard factory checking a required permission against the auth slice's permission set — attached inside each feature's own routes — multiuser-app plateau
domain: skill
type: template
plateau: multiuser-app
artifact_type: guard
version: 20260711230000
tags:
  - skill/template/class
  - plateau/multiuser-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"
---

> The reusable `requirePermission` factory is defined once here. Each feature imports it and attaches it inside its own `{feature}.routes.ts`.

# Goal

- Restrict navigation into a route based on a required permission, reusing the same permission model as UI-level visibility checks

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Checks a permission string, never a role name
- Attached at the feature's own route, never centralized in the shell's root routes

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------------- | -------------------- | --------- |
| Permission guard factory | `requirePermission` | `requirePermission` | `permission.guard.ts` | `permission.guard.ts` |

# Implementation

```typescript
// Skill: class-permission-guard
// Plateau: multiuser-app
// Version: 20260711230000

export function requirePermission(permission: string): CanActivateFn & CanMatchFn {
  return () => {
    const store = inject(Store);
    const permissions = store.selectSignal(selectPermissions)();
    const router = inject(Router);
    return permissions.includes(permission) || router.createUrlTree(['/forbidden']);
  };
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Rules

## MUST
- The guard MUST check a permission string against `shared-state`'s `permissions`.
- The guard MUST be attached inside the feature's own routes, never centralized in `apps/platform-shell`.
- A failed permission check MUST redirect to a forbidden/not-authorized route.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Centralizing all permission guards in the shell's root routes "for visibility"**
  - Consequence: reintroduces the coupling hierarchical route ownership was designed to prevent
  - Instead: each feature attaches its own guards using the shared `requirePermission` factory

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Check list

- [ ] Every route requiring a specific permission uses `requirePermission(...)` at that feature's own route definition
- [ ] No permission guard is defined inside `apps/platform-shell`'s root routes

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Unittest TestCases

- [ ] WHEN a user with the required permission navigates to the guarded route THEN
  - [ ] navigation proceeds
- [ ] WHEN a user without the required permission navigates to the guarded route THEN
  - [ ] navigation redirects to the forbidden route

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]
