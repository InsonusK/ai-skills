---
name: class-auth-interceptor
description: HTTP interceptor that attaches the in-memory access token to outgoing requests and triggers a silent refresh on 401 responses
domain: skill
type: template
plateau: authenticated
artifact_type: interceptor
version: 20260711150000
tags:
  - skill/template/class
  - plateau/authenticated
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

# Goal

- Attach the current in-memory access token to every outgoing request without any feature/component needing to do so manually
- Recover transparently from an expired access token via a single silent-refresh-and-retry, without surfacing a spurious error to the user

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- This is the only place a request is decorated with the `Authorization` header — no feature or `data-access` Client sets it directly

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------- | -------------------- | --------- |
| Auth interceptor | `authInterceptor` | `authInterceptor` | `auth.interceptor.ts` | `auth.interceptor.ts` |

# Implementation

```typescript
// Skill: class-auth-interceptor
// Plateau: authenticated
// Version: 20260711150000

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(Store);
  const accessToken = store.selectSignal(selectAccessToken)();

  const authorized = accessToken
    ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : req;

  return next(authorized).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        store.dispatch(AuthActions.silentRefreshRequested());
        // a future solution may define full retry-after-refresh orchestration;
        // this interceptor's contract is to trigger the refresh and surface
        // the 401 if refresh does not resolve it.
      }
      return throwError(() => error);
    }),
  );
};
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

# Rules

## MUST
- This interceptor MUST be the only place an outgoing request is decorated with the `Authorization` header — feature/data-access code MUST NOT set that header manually.
- On a 401 response, this interceptor MUST dispatch `Silent Refresh Requested` rather than immediately logging the user out — a single transparent refresh attempt comes before treating the session as expired.
- This interceptor MUST NOT be applied to the silent-refresh request itself (to avoid an infinite loop) — the refresh call relies solely on the `HttpOnly` cookie, not a bearer header.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Retrying the original request indefinitely on repeated 401s**
  - Consequence: infinite retry loop if the refresh itself is failing
  - Instead: attempt exactly one silent refresh per 401; if it also fails, treat the session as expired (dispatch `Session Expired`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

# Check list

- [ ] The interceptor is registered globally via `provideHttpClient(withInterceptors([authInterceptor]))`
- [ ] The interceptor is excluded from the silent-refresh endpoint's own request

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]

# Unittest TestCases

- [ ] WHEN a request is made with a valid access token in the store THEN
  - [ ] the request carries an `Authorization: Bearer <token>` header
- [ ] WHEN a request receives a 401 THEN
  - [ ] `Silent Refresh Requested` is dispatched exactly once
- [ ] WHEN the silent refresh itself is requested THEN
  - [ ] it is not intercepted with an `Authorization` header

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]
