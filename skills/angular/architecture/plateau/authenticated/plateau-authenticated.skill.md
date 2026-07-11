---
name: plateau-authenticated
description: Data-capable app gains a full session lifecycle — in-memory access token with transparent silent refresh, a single permission-string authorization model shared by route guards and a UI visibility directive, and a global auth interceptor.
domain: skill
type: template
version: 20260711150000
tags:
  - skill/template/plateau
  - plateau/authenticated
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"
parent_plateau: "[[skills/angular/architecture/plateau/data-capable/plateau-data-capable.skill.md|data-capable]]"
---

> Fourth plateau in the main application's chain. Previous: [[skills/angular/architecture/plateau/data-capable/plateau-data-capable.skill.md|data-capable]]. Next: [[skills/angular/architecture/plateau/observable/plateau-observable.skill.md|observable]].

# Core Principles

- Everything from [[skills/angular/architecture/plateau/data-capable/plateau-data-capable.skill.md|data-capable]] carries over unchanged: hierarchical routing, selective preloading, Signal Forms, and the Facade/Client-layered data-access pattern
- The access token lives only in memory, inside `shared-state`'s auth slice; the refresh token lives only in an `HttpOnly`/`Secure`/`SameSite` cookie the client never reads — no token value ever touches `localStorage`/`sessionStorage`
- Every authorization check — guards, the UI directive — is expressed as a permission string, never a role name
- Route guards live inside the feature whose route they protect, consistent with hierarchical route ownership — never centralized in the shell
- Hiding UI with a permission check is a convenience, not a security boundary; the backend remains the actual authorization boundary
- A single global interceptor is the only place a request is decorated with the `Authorization` header, and the only place a 401 triggers a silent refresh

# Capabilities

- structure, state management, routing, forms, data access
  - Unchanged from [[skills/angular/architecture/plateau/data-capable/plateau-data-capable.skill.md|data-capable]]
- authentication & session
  - No token value ever persists in `localStorage`/`sessionStorage`, closing off the most common XSS-driven token theft vector
  - Silent, transparent session recovery after a page reload or a transient 401, without forcing the user to re-authenticate
  - A single permission model reused by route guards and a structural UI directive — no parallel authorization logic to keep in sync
- authorization
  - Feature routes protect themselves with `requirePermission(...)` guards attached at their own route definitions
  - `*hasPermission` conditionally renders UI based on the current session's permission set

# Usecases

## App bootstrap recovers a session after reload

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant App as apps/platform-shell
    participant Store as shared-state (auth slice)
    participant Api as Backend

    User->>App: reloads the page
    App->>Store: dispatch Silent Refresh Requested (bootstrap)
    Store->>Api: silent refresh call (HttpOnly refresh cookie sent automatically)
    activate Api
    Api-->>Store: new access token + permissions, or failure
    deactivate Api
    alt refresh succeeded
        Store->>Store: Silent Refresh Succeeded — accessToken, permissions populated
        Store-->>App: user appears still logged in
    else refresh failed
        Store->>Store: Silent Refresh Failed — treated as logged out
        Store-->>App: user sees a logged-out state
    end
```

## A request transparently recovers from an expired access token

```mermaid
sequenceDiagram
    autonumber
    participant Feature as Feature Facade/Client
    participant Interceptor as authInterceptor
    participant Store as shared-state (auth slice)
    participant Api as Backend

    Feature->>Interceptor: outgoing HTTP request
    Interceptor->>Interceptor: attach Authorization: Bearer <accessToken>
    Interceptor->>Api: request
    activate Api
    Api-->>Interceptor: 401 Unauthorized
    deactivate Api
    Interceptor->>Store: dispatch Silent Refresh Requested
    Store->>Api: silent refresh call
    activate Api
    Api-->>Store: new access token
    deactivate Api
    Store-->>Interceptor: Silent Refresh Succeeded
    Interceptor-->>Feature: surfaces the original 401 (single-refresh contract)
    Note over Feature,Api: the feature's typed domain error handling (from data-capable)<br/>surfaces this as a normal failure if the caller does not retry
```

## Restricting a destructive action to a permission

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant Directive as *hasPermission
    participant Guard as requirePermission
    participant Store as shared-state (auth slice)
    participant Api as Backend

    Note over Directive,Store: UI convenience layer
    Directive->>Store: read selectPermissions()
    Store-->>Directive: permissions[]
    Directive-->>User: renders "Delete" button only if permitted

    User->>Guard: navigates to :id/delete-confirm
    Guard->>Store: read selectPermissions()
    alt has permission
        Guard-->>User: navigation proceeds
        User->>Api: DELETE /orders/:id
        Note over Api: backend re-checks authorization — the real boundary
    else missing permission
        Guard-->>User: redirected to /forbidden
    end
```
