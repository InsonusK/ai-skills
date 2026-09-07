---
description: HTTP interceptor that attaches the in-memory access token to outgoing requests and triggers a silent refresh on 401 responses
project_name: shared-state
name: auth
element_kind: interceptor
change_kind: create
tags:
  - solution/authentication
  - element/auth-interceptor-ts
---

# Goals

- Attach the current in-memory access token to every outgoing request without any feature/component needing to do so manually
- Recover transparently from an expired access token via a single silent-refresh-and-retry, without surfacing a spurious error to the user

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------------- | -------------------- | --------- |
| Auth interceptor | authInterceptor | authInterceptor | auth.interceptor.ts | auth.interceptor.ts |

# Implementation changes

```typescript
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
        // future `solution-api-http-layer` defines the retry-after-refresh
        // orchestration in full; this interceptor's contract is to trigger
        // the refresh and surface the 401 if refresh does not resolve it.
      }
      return throwError(() => error);
    }),
  );
};
```

# Rule changes

## MUST
- This interceptor is the only place an outgoing request gets an `Authorization` header.
  - Risk: features setting the header manually will get it wrong (stale token, wrong scheme) and duplicate the logic.
  - Fix: `provideHttpClient(withInterceptors([authInterceptor]))`; feature/data-access code never touches `Authorization`.
- On a 401 it dispatches `Silent Refresh Requested`, not an immediate logout.
  - Risk: logging out on the first 401 kicks the user out on a merely-expired access token that a refresh would fix.
  - Fix: dispatch one silent refresh; only if that also fails does `Session Expired` follow.
- It never intercepts the silent-refresh request itself.
  - Risk: attaching a (missing/expired) bearer to the refresh call, or looping refresh→401→refresh forever.
  - Fix: `if (req.url.includes('/auth/')) return next(req);` first — the refresh relies on the `HttpOnly` cookie only.

## SHOULD
- **Retrying the original request indefinitely on repeated 401s** — Consequence: infinite retry loop if the refresh itself is failing — Instead: attempt exactly one silent refresh per 401; if it also fails, treat the session as expired (dispatch `Session Expired`, handled by the base auth slice from the State management solution)

# Check list

- [ ] The interceptor is registered globally via `provideHttpClient(withInterceptors([authInterceptor]))`
- [ ] The interceptor is excluded from the silent-refresh endpoint's own request

# Unittest TestCases

- [ ] WHEN a request is made with a valid access token in the store THEN
  - [ ] the request carries an `Authorization: Bearer <token>` header
- [ ] WHEN a request receives a 401 THEN
  - [ ] `Silent Refresh Requested` is dispatched exactly once
- [ ] WHEN the silent refresh itself is requested THEN
  - [ ] it is not intercepted with an `Authorization` header
