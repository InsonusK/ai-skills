---
description: Extend the generic Client pattern from `solution-api-http-layer` to distinguish a network-level (offline) failure from a genuine server-side error response
project_name: "{Feature}"
name: "{feature}"
element_kind: service
change_kind: extend
tags:
  - solution/offline-first
  - element/feature-client-ts
---

# How this generic file is used
This extends [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create]] applied to any feature's `{feature}.client.ts`.

# Goals

- Let the Facade (and, eventually, the future `solution-offline-sync`) distinguish "this failed because we're offline" from "the server rejected this request" — a distinction the base `{feature}.client.ts` pattern did not need to make

# Implementation changes

```typescript
// {feature}.client.ts — extended catch block
try {
  const responseDto = await firstValueFrom(this.http.post<OrderDto>('/orders', dto));
  return orderDtoToModel(responseDto);
} catch (error) {
  if (error instanceof HttpErrorResponse && error.status === 0) {
    // status 0 (or ProgressEvent-based error with no response) indicates the request
    // never reached the server at all — a network-level failure, not a server response
    throw new OfflineTransportError('addOrder', { cause: error });
  }
  if (error instanceof HttpErrorResponse && error.status === 409) {
    throw new OrdersConflictError(input.id, { cause: error });
  }
  throw new OrdersAddError('unexpected error adding order', { cause: error });
}
```

# Rule changes

## MUST
- Every Client method checks for a network-level failure (`HttpErrorResponse` with `status === 0`) **before** any server status code, and throws `OfflineTransportError` for it.
  - Risk: a `status === 0` handled by a generic `500`/`4xx` branch surfaces "offline" as "the server rejected this", so a future write queue cannot tell them apart.
  - Fix: `if (e instanceof HttpErrorResponse && e.status === 0) throw new OfflineTransportError(...)` as the first check in `catchError`.
- `OfflineTransportError` is a single shared type, defined once in `libs/shared/http-core`.
  - Risk: a per-feature copy means callers `instanceof`-check the wrong class and miss offline errors from other features.
  - Fix: one export from `libs/shared/http-core`; every feature's Client imports it.

## SHOULD
- **Treating a `status === 0` failure the same as any other server error** — Consequence: the Facade (and the future sync-queue solution) has no reliable way to tell "we're offline, this is retryable later" apart from "the server actively rejected this," which is exactly the distinction this extension exists to provide — Instead: always check for the network-level failure first and throw the shared `OfflineTransportError`

# Check list

- [ ] Every Client method checks for a network-level failure before any status-code-specific handling
- [ ] `OfflineTransportError` is defined once, shared across all features, not redefined per feature

# Unittest TestCases

- [ ] WHEN a Client method's HTTP call fails with no response received (network unreachable) THEN
  - [ ] it throws the shared `OfflineTransportError`, not that feature's own domain error
- [ ] WHEN a Client method's HTTP call fails with an actual server response (e.g. 409, 500) THEN
  - [ ] it throws that feature's own typed domain error, exactly as established in `solution-api-http-layer` — unaffected by this extension
