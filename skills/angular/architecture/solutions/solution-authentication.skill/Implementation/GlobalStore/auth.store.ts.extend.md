---
description: Extend the auth slice created by the State management solution with an in-memory access token, granular permissions, and silent-refresh handling
project_name: shared-state
name: auth
element_kind: store
change_kind: extend
tags:
  - solution/authentication
  - element/auth-store-ts
---

# Goals

- Add token lifecycle state (in-memory access token, refresh-in-progress flag) and a granular permission set to the existing auth slice
- Trigger a silent refresh on app bootstrap so an in-memory-only access token survives a page reload from the user's point of view

# Implementation changes

Extends the actions/state already defined in [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create]]:

```typescript
// auth.actions.ts — additional events
export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    // ...existing events from the State management solution...
    'Silent Refresh Requested': emptyProps(),
    'Silent Refresh Succeeded': props<{ accessToken: string; permissions: string[] }>(),
    'Silent Refresh Failed': emptyProps(),
  },
});
```

```typescript
// auth.reducer.ts — additional state shape
interface AuthState {
  accessToken: string | null; // in-memory only — never persisted, see token-storage-strategy ADR
  permissions: string[];      // granular permission strings, see authorization-model ADR
  refreshInProgress: boolean;
  // ...existing fields (currentUser, etc.) from the State management solution...
}
```

```typescript
// auth.effects.ts — bootstrap silent refresh
export class AuthEffects {
  silentRefreshOnInit$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.silentRefreshRequested),
      exhaustMap(() =>
        this.authFacade.silentRefresh().pipe( // sends the HttpOnly refresh cookie automatically
          map(({ accessToken, permissions }) =>
            AuthActions.silentRefreshSucceeded({ accessToken, permissions })),
          catchError(() => of(AuthActions.silentRefreshFailed())),
        ),
      ),
    ),
  );
}
```

# Rule changes

## MUST
- `accessToken` MUST be held only in this slice's in-memory NgRx state — never written to any persistent storage, per [[skills/angular/architecture/solutions/solution-authentication.skill/adr/token-storage-strategy]].
- `Silent Refresh Requested` MUST be dispatched once during application bootstrap (e.g. from an `APP_INITIALIZER`-equivalent or a root route resolver), before any authenticated request is made.
- `permissions` MUST be a flat array of permission strings, never role names, per [[skills/angular/architecture/solutions/solution-authentication.skill/adr/authorization-model]].

# Anti-patterns

- **Persisting `accessToken` to storage "to survive reloads more simply"**
  - Consequence: reintroduces exactly the XSS exposure the token-storage-strategy ADR chose in-memory storage to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow to repopulate the in-memory token after a reload

# Check list

- [ ] `accessToken` exists only as in-memory NgRx state, confirmed absent from any storage write
- [ ] Bootstrap triggers exactly one `Silent Refresh Requested` dispatch before any authenticated call

# Unittest TestCases

- [ ] WHEN the app bootstraps with a valid refresh cookie THEN
  - [ ] `Silent Refresh Succeeded` populates `accessToken` and `permissions`
- [ ] WHEN the app bootstraps with no valid refresh cookie THEN
  - [ ] `Silent Refresh Failed` is dispatched and the user is treated as logged out
- [ ] WHEN `Session Expired` (from the base auth slice) is dispatched THEN
  - [ ] `accessToken` and `permissions` are cleared
