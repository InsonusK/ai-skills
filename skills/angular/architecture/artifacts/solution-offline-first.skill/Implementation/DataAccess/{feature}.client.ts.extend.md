---
description: Extend the generic Client pattern from the API/HTTP-слой solution to distinguish a network-level (offline) failure from a genuine server-side error response
project_name: "{Feature}"
name: "{feature}"
element_kind: service
change_kind: extend
---

# How this generic file is used
This extends [[../../../solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.client.ts.create.md]], applied to any feature's `{feature}.client.ts`.

# Goals

- Let the Facade (and, eventually, the future "Синхронизация offline-данных" solution) distinguish "this failed because we're offline" from "the server rejected this request" — a distinction the base `{feature}.client.ts` pattern did not need to make

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
- Every Client method's error handling MUST check for a network-level failure (an `HttpErrorResponse` with `status === 0`, indicating no response was ever received) before checking for any specific server status code, and throw `OfflineTransportError` in that case instead of a feature-specific domain error.
- `OfflineTransportError` MUST be a single, shared error type (defined once in `libs/shared/http-core`, not redefined per feature) so callers across every feature can catch it uniformly.

# Anti-patterns

- **Treating a `status === 0` failure the same as any other server error**
  - Consequence: the Facade (and the future sync-queue solution) has no reliable way to tell "we're offline, this is retryable later" apart from "the server actively rejected this," which is exactly the distinction this extension exists to provide
  - Instead: always check for the network-level failure first and throw the shared `OfflineTransportError`

# Check list

- [ ] Every Client method checks for a network-level failure before any status-code-specific handling
- [ ] `OfflineTransportError` is defined once, shared across all features, not redefined per feature

# Unittest TestCases

- [ ] WHEN a Client method's HTTP call fails with no response received (network unreachable) THEN
  - [ ] it throws the shared `OfflineTransportError`, not that feature's own domain error
- [ ] WHEN a Client method's HTTP call fails with an actual server response (e.g. 409, 500) THEN
  - [ ] it throws that feature's own typed domain error, exactly as established in the "API/HTTP-слой" solution — unaffected by this extension
