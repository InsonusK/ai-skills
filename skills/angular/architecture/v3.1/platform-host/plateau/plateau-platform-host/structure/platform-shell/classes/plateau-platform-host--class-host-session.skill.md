---
name: plateau-platform-host--class-host-session
description: HostSession in apps/platform-shell — implements @platform/contracts' SessionContract as a read-only signal view over libs/shared/state's auth slice, provided once under SESSION_CONTRACT; every mounted remote reads this exact instance — platform-host plateau
domain: skill
type: template
whenToUse: when creating apps/platform-shell/src/app/session/host-session.ts, wiring SESSION_CONTRACT, or reviewing how the shared session stays in lockstep with the auth slice
plateau: platform-host
artifact_type: service
version: 20260903180000
tags:
  - skill/template/class
  - plateau/platform-host
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]]"
---

> `apps/platform-shell/src/app/session/host-session.ts`. Bound once as `{ provide: SESSION_CONTRACT, useExisting: HostSession }` in `app.config.ts`. VP2 `SessionSharing` — requires the monolith's `auth` slice (VP7).

# Goal

- Give every mounted remote a live, read-only view of the current session (`currentUser` / `permissions` / `isAuthenticated`) without any remote implementing authentication — in lockstep with the host's own `auth` slice, no polling

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]

# Core Principles

- Apply ONE plateau template per class
- `SessionContract` is **read-only** from a remote's point of view — no remote can mutate the session through it
- The signals are a thin view over `libs/shared/state`'s `selectCurrentUser` / `selectPermissions` / `selectIsLoggedIn` — never a second copy of session state

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]

# Implementation

```typescript
// Skill: class-host-session
// Plateau: platform-host
import { computed, inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { SessionContract } from '@platform/contracts';
import { selectCurrentUser, selectPermissions, selectIsLoggedIn } from '@org/shared-state';

@Injectable({ providedIn: 'root' })
export class HostSession implements SessionContract {
  private readonly store = inject(Store);
  // read-only views over the auth slice — never a second source of truth
  readonly currentUser = this.store.selectSignal(selectCurrentUser);
  readonly permissions = this.store.selectSignal(selectPermissions);
  readonly isAuthenticated = computed(() => this.store.selectSignal(selectIsLoggedIn)());
}
// app.config.ts:  { provide: SESSION_CONTRACT, useExisting: HostSession }
```

> The `example/` uses a minimal signal-backed stand-in (`setSession` / `clearSession`) instead of an NgRx store, so the federation wiring can be exercised without composing the whole monolith.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]

# Rules

## MUST
- `SESSION_CONTRACT` is provided exactly once, here, at the composition root.
- The implementation is a read-only view over the `auth` slice — it exposes no method to log in/out or change permissions.
- Every field is a `Signal` — a remote reads them reactively; a host logout or session expiry propagates to every remote through this one instance with no message passing.
- Authorization is expressed as permission strings — the same strings the host's own `*hasPermission` uses.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]

# Check list

- [ ] `SESSION_CONTRACT` has exactly one provider
- [ ] The implementation exposes no session-mutation method
- [ ] Its signals derive from `libs/shared/state`'s auth selectors, not a copy
- [ ] Permissions are strings, never role names

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]

# Unittest TestCases

- [ ] WHEN a remote reads `SESSION_CONTRACT.permissions()` THEN it matches the set the host's own UI uses for `*hasPermission`
- [ ] WHEN the host session expires THEN `SESSION_CONTRACT.isAuthenticated()` becomes `false` for every remote, with no action on the remote's part
- [ ] WHEN resolved through `SESSION_CONTRACT` and through `HostSession` THEN it is the same instance

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]
