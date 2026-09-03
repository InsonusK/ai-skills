---
name: plateau-embeddable-app--class-require-permission
description: The remote's route guard factory — a CanActivateFn reading currentUser/permissions/isAuthenticated from @platform/contracts' SESSION_CONTRACT; no login flow, no local session copy, an unauthenticated session returns false — embeddable-app plateau
domain: skill
type: template
whenToUse: when creating the remote's src/app/session/require-permission.ts, or reviewing why the remote sees isAuthenticated false
plateau: embeddable-app
artifact_type: module
version: 20260903180000
tags:
  - skill/template/class
  - plateau/embeddable-app
  - stack/typescript
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]]"
---

> `src/app/session/require-permission.ts`. Reads the `SESSION_CONTRACT` singleton the host published (`solution-session-sharing`). This is the remote's **own** small guard — it does NOT import `@org/shared-auth-ui` (that is a monolith lib).

# Goal

- Let the remote gate its own routes on the host's session using permission strings, with zero authentication code of its own

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Implementation

```typescript
// Skill: class-require-permission
// Plateau: embeddable-app
import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { SESSION_CONTRACT } from '@platform/contracts';

export function requirePermission(permission: string): CanActivateFn {
  return () => {
    const session = inject(SESSION_CONTRACT);
    if (!session.isAuthenticated()) return false; // the host owns any redirect
    return session.permissions().includes(permission);
  };
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Rules

## MUST
- Read `currentUser` / `permissions` / `isAuthenticated` **only** through `SESSION_CONTRACT` — never a local copy, never a second source.
- When `isAuthenticated()` is `false`, return `false` (or render a not-authenticated state) — never trigger a navigation to a login route. The host owns login.
- Every authorization check is a **permission string**, matching the host's own `*hasPermission` semantics — never a role name.

## SHOULD
- Do not duplicate the host's forbidden/login page — a minimal inline "sign in to continue" placeholder in the guarded component is enough.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Check list

- [ ] `SESSION_CONTRACT` is the only session source in the remote
- [ ] No login route, no local session state
- [ ] `isAuthenticated: false` → `false` / not-authenticated state, no redirect
- [ ] The check is a permission string, not a role

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Unittest TestCases

- [ ] WHEN the host reports no session THEN the guard denies (returns `false`)
- [ ] WHEN an authenticated user lacks the permission THEN the guard denies
- [ ] WHEN an authenticated user has the permission string THEN the guard allows

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]
