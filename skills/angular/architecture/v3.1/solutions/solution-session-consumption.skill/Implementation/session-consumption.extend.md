---
description: Extend a federation remote's exposed module to read SessionContract from @platform/contracts, render a not-authenticated state, and express authorization as permission strings
element_kind: project
change_kind: extend
tags:
  - solution/session-consumption
  - element/remote-session-consumption
---

# Structure

No new project. The remote's exposed module (or a small `session/` folder inside it) injects `SessionContract` and derives a guard/directive from it.

```
src/
  session/
    require-permission.ts        (route guard factory — permission string in, redirect-or-proceed)
    has-permission.directive.ts  (structural directive — *hasPermission="'orders.delete'")
```

# Implementation changes

```typescript
// reads the singleton contract published by the host (solution-session-sharing)
import { SESSION_CONTRACT } from '@platform/contracts';

export function requirePermission(permission: string): CanActivateFn {
  return () => {
    const session = inject(SESSION_CONTRACT);
    if (!session.isAuthenticated()) return false;          // host renders / owns the redirect
    return session.permissions().includes(permission);
  };
}
```

# Rules

## MUST
- Read `currentUser` / `permissions` / `isAuthenticated` only through `SessionContract` — never a local copy, never a second source.
  - Risk: session state that drifts from the host (still "logged in" after a host logout).
  - Fix: inject the contract; derive everything from its signals.
- When `isAuthenticated()` is false, render a not-authenticated state — never trigger a navigation to a login route.
  - Risk: the remote competes with the host for the authentication flow.
  - Fix: the host owns login; the remote shows an inert state and waits.
- Every authorization check is a permission string, matching the host's own `*hasPermission` semantics.

## SHOULD
- Avoid duplicating the host's forbidden/login UI in the remote — a minimal inline "sign in to continue" placeholder is enough.

# Check list
- [ ] `SessionContract` is the only session source in the remote.
- [ ] No login route, no local session state.
- [ ] `isAuthenticated: false` → not-authenticated state, no redirect.
