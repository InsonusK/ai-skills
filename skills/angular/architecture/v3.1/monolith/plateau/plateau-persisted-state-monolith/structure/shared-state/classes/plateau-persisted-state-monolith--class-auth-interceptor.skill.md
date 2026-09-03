---
name: plateau-persisted-state-monolith--class-auth-interceptor
description: The functional HTTP interceptor that attaches the in-memory access token and, on a 401, dispatches a single silent refresh — never intercepting the refresh call itself — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing authInterceptor (VP7) — the bearer attach, the 401 -> single silent refresh, skipping the refresh call itself
plateau: persisted-state-monolith
artifact_type: interceptor
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"

> `libs/shared/state/src/lib/auth/auth.interceptor.ts`. Registered once at the shell via `provideHttpClient(withFetch(), withInterceptors([authInterceptor]))`.

# Goal

- Attach the current in-memory access token to every outgoing request, so no feature/component sets `Authorization` manually
- Recover transparently from an expired access token via a single silent-refresh, without surfacing a spurious error

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md|HttpLayer/auth.interceptor.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- A functional `HttpInterceptorFn` — reads `selectAccessToken` via `inject(Store).selectSignal(...)`
- The silent-refresh request (`/auth/…`) is never intercepted — no bearer, no 401→refresh loop
- On a 401 it dispatches `Silent Refresh Requested` (one attempt) and re-throws — it does not immediately log the user out

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md|HttpLayer/auth.interceptor.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Auth interceptor | `authInterceptor` | `authInterceptor` | `auth.interceptor.ts` | `auth.interceptor.ts` |

# Implementation

```typescript
// Skill: class-auth-interceptor
// Plateau: persisted-state-monolith
// Version: 20260903190000

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(Store);
  if (req.url.includes('/auth/')) return next(req); // login / refresh / logout — cookie-only

  const token = store.selectSignal(selectAccessToken)();
  const authorized = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authorized).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) store.dispatch(AuthActions.silentRefreshRequested());
      return throwError(() => error);
    }),
  );
};
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md|HttpLayer/auth.interceptor.ts.create]]

# Rules

## MUST
- This interceptor must be the only place an outgoing request gets an `Authorization` header.
- It must dispatch a single `Silent Refresh Requested` on a 401 — never an immediate logout.
- It must never be applied to the silent-refresh request itself (skip `/auth/…`).
- It must be registered once, at the shell, via `withInterceptors([authInterceptor])`.
- Never apply several plateau templates per class/artifact.
- Never retry the original request indefinitely on repeated 401s — one refresh attempt, then treat as logged out.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md|HttpLayer/auth.interceptor.ts.create]]

# Check list

- [ ] Registered globally via `withInterceptors([authInterceptor])`
- [ ] Excluded from the silent-refresh endpoint's own request
- [ ] A valid token → `Authorization: Bearer <token>`
- [ ] A 401 → exactly one `Silent Refresh Requested`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md|HttpLayer/auth.interceptor.ts.create]]

# Unittest TestCases

- [ ] WHEN a request is made with a token in the store THEN it carries `Authorization: Bearer <token>`
- [ ] WHEN a request receives a 401 THEN `Silent Refresh Requested` is dispatched exactly once
- [ ] WHEN the request is to `/auth/refresh` THEN it is not decorated with a bearer

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create.md|HttpLayer/auth.interceptor.ts.create]]
