---
name: token-storage-strategy
description: Where the access and refresh tokens are stored on the client
problem: How to store auth tokens in a way that minimizes exposure to XSS while remaining workable for a micro-frontend architecture with third-party federated code running in the same JS runtime
decision: Store the access token only in memory (never in localStorage/sessionStorage); store the refresh token in an HttpOnly, Secure, SameSite cookie set by the backend
tags:
  - solution/authentication
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The platform runs federated embeddable apps (see `solution-platform-embeddability`) built and deployed by separate teams, sharing the same JS runtime and Angular instance as the host. Any token storage mechanism reachable from JavaScript is, by construction, reachable by that third-party code too — and by any XSS payload that manages to execute. We need a token storage strategy that minimizes what an XSS payload (first-party bug or, worst case, a misbehaving/compromised remote) can steal, without breaking the ability of the UI to know "am I logged in" and "what can I do" synchronously.

# Selected variant

**Selected variant:** [[#In-memory access token + HttpOnly refresh cookie]]

The access token is held only in an in-memory Signal inside `libs/shared/state`'s auth slice — never written to `localStorage`/`sessionStorage`/any persistent client storage. The refresh token is set by the backend as an `HttpOnly`, `Secure`, `SameSite` cookie, invisible to JavaScript entirely. On app bootstrap, a silent-refresh call (which the browser sends the refresh cookie with automatically) obtains a fresh access token into memory.

# Searched variants

## In-memory access token + HttpOnly refresh cookie

### Description

Access token lives only as an in-memory Signal (part of the `shared-state` auth slice from the "State management" solution), lost on full page reload by design. Refresh token lives in an `HttpOnly`/`Secure`/`SameSite` cookie, never touched by JS. On bootstrap and on 401 responses, the app calls a refresh endpoint; the browser attaches the refresh cookie automatically, and the response body contains a new access token that is stored back into memory.

### Benefits

- An XSS payload (first-party or from a misbehaving federated remote, given they share one JS runtime per the embeddability solution) cannot read the refresh token at all, and can only ever obtain the access token if it executes while a session is active in memory — nothing persists to steal for a later session
- No CSRF-token machinery needed for regular API calls, since the access token — not a cookie — authorizes them; only the refresh endpoint relies on the cookie, and standard `SameSite=Strict`/`Lax` plus checking the request's origin closes that one endpoint's CSRF exposure
- Matches current OWASP-aligned guidance for SPA token storage
- Composes with the federation architecture from the embeddability solution: the in-memory Signal is exactly the kind of state that can be exposed to embeddable apps through `@platform/contracts`, without ever exposing a stored token value to be copied out

### Costs

- Access token is lost on a full page reload; every reload requires a silent-refresh round-trip before the app is fully usable, adding a brief loading state at bootstrap
- Requires backend cooperation: the refresh endpoint must set/read the `HttpOnly` cookie and issue new access tokens, which is more backend-side plumbing than a token the client simply reads out of storage
- Slightly more moving parts (bootstrap silent-refresh, 401-triggered refresh) than a single storage read

## localStorage/sessionStorage for both tokens

### Description

Both access and refresh tokens are stored in `localStorage` or `sessionStorage`, read directly by an HTTP interceptor.

### Benefits

- Simplest possible implementation — no silent-refresh-on-bootstrap dance, tokens survive page reloads directly
- No backend-side cookie handling required

### Costs

- Any XSS execution — including from a compromised or buggy federated remote sharing the same JS runtime — can read both tokens directly out of storage, with no mitigation
- Directly at odds with the micro-frontend trust model established in the embeddability solution, where the platform explicitly does not fully control every remote's code

## Fully cookie-based (both tokens as HttpOnly cookies, backend-driven authorization)

### Description

Both access and refresh tokens live in `HttpOnly` cookies; the backend authorizes every request based on the cookie automatically, and the client never sees a token value directly.

### Benefits

- Maximum protection against token theft via XSS — no token value is ever present in JS memory or storage at all
- Simplest client-side code — no interceptor needs to attach a bearer token

### Costs

- Every API-authorizing request now needs CSRF protection, not just the refresh endpoint, since cookies are sent automatically by the browser on any request to the domain
- The UI has no synchronous, JS-visible signal of "am I logged in" or "what are my permissions" without an extra dedicated endpoint/response to read that information from — the token itself can no longer carry that information into JS-readable state
- Awkward fit for exposing session/permission state to federated embeddable apps through `@platform/contracts`, since there is no in-memory value to hand them — every embeddable app would need its own way to ask the backend
