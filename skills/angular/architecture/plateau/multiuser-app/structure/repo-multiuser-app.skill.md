---
name: repo-multiuser-app
description: Nx workspace layout for the multiuser-app plateau — monitored-app plus real authentication — session lifecycle, in-memory access token, permission-based route guards and UI directives, and the platform-host-side SessionContract shared with every embeddable app
domain: skill
type: template
plateau: multiuser-app
version: 20260711230000
tags:
  - skill/template/repo
  - plateau/multiuser-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

> Sixth and final plateau in the main application's chain. Parent: [[skills/angular/architecture/plateau/monitored-app/plateau-monitored-app.skill.md|monitored-app]]. This is the **"multiuser-app"** milestone: the application scales to many users — every user is authenticated, session state lives in a single auditable NgRx slice with an in-memory-only access token, authorization is expressed as permission strings enforced both at the route level (guards) and the UI level (`*hasPermission`), and the platform host now shares a real `SessionContract` with every mounted embeddable app. All 17 solutions in the main chain are now applied.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)
  /[platform-shell-e2e](./platform-shell-e2e/project-platform-shell-e2e.skill.md)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)
    /[http-core](./shared-http-core/project-shared-http-core.skill.md)
    /[auth-ui](./shared-auth-ui/project-shared-auth-ui.skill.md)      <- new (solution-authentication)
    /[logging](./shared-logging/project-shared-logging.skill.md)
    /[offline-sync](./shared-offline-sync/project-shared-offline-sync.skill.md)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)
```

- `libs/shared/auth-ui` is the only new top-level project — the permission-checking directive and route-guard factory shared by every feature. `libs/shared/state`'s `auth` slice, a skeleton since `online-monolith`, becomes a real session lifecycle here.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | Composition root — unchanged in structure; now serves `SessionContract` to every mounted embeddable app. |
| /apps/platform-shell-e2e | [[platform-shell-e2e/project-platform-shell-e2e.skill.md\|project-platform-shell-e2e.skill]] | Playwright end-to-end scenario specs, now including login/logout and a permission-denied scenario. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI — unchanged. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store; `auth` slice is now a real session lifecycle (login, silent refresh, logout), plus `authInterceptor`. |
| /libs/shared/http-core | [[shared-http-core/project-shared-http-core.skill.md\|project-shared-http-core.skill]] | Base HTTP service and `OfflineTransportError` — unchanged. |
| /libs/shared/auth-ui | [[shared-auth-ui/project-shared-auth-ui.skill.md\|project-shared-auth-ui.skill]] | Permission-checking directive (`*hasPermission`) and route-guard factory (`requirePermission`) shared by every feature. |
| /libs/shared/logging | [[shared-logging/project-shared-logging.skill.md\|project-shared-logging.skill]] | `LoggerService`, `BackendLogSink`, `LogRetryQueue` — unchanged. |
| /libs/shared/offline-sync | [[shared-offline-sync/project-shared-offline-sync.skill.md\|project-shared-offline-sync.skill]] | Mutation queue and replay orchestrator — unchanged. |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template, now optionally attaching a `requirePermission` guard to its own routes. |
| /libs/{feature}/data-access | [[feature-data-access/project-feature-data-access.skill.md\|project-feature-data-access.skill]] | Generic template — unchanged. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/shared-auth-ui.project.create|shared-auth-ui.project.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]

## Nx Tag Taxonomy

Unchanged axes from `platform-monolith`: `type` ∈ {`app`, `host`, `e2e`, `feature`, `data-access`, `ui`, `util`, `store`}, `scope` ∈ {`platform`, `shared`, `{feature-name}`}. `libs/shared/auth-ui` is tagged `type:ui`, `scope:shared`.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

## Cross-cutting conventions

These rules apply inside every project in the workspace and have no single project of their own to live in:

- **Three-tier state placement**, **hierarchical route ownership**, **Facade/Client/Mapper layering**, **single logging seam**, **selective preloading**, **bundle budgets**, **offline-aware reads/mutations**, **federation boundary** — unchanged from `monitored-app`.
- **Authorization model**: authorization is always expressed as permission strings, never role names, checked identically at the route level (`requirePermission` guard) and the UI level (`*hasPermission` directive).
- **In-memory-only access token**: the access token lives only in the `auth` slice's in-memory NgRx state — never `localStorage`/`sessionStorage`, and never exposed through `SessionContract`.
- **SessionContract**: the platform host is the sole source of `SessionContract` (`currentUser`, `permissions`, `isAuthenticated`); every embeddable app reads it read-only, never implementing its own login flow.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- The access token MUST only ever live in `shared-state`'s in-memory auth slice — never in `localStorage`/`sessionStorage`.
- `authInterceptor` MUST be the only place an outgoing request is decorated with the `Authorization` header.
- Every permission check (route guard or UI directive) MUST check a permission string, never a role name.
- `SessionContract` MUST be read-only from an embeddable app's point of view.
- All other rules from [[skills/angular/architecture/plateau/monitored-app/plateau-monitored-app.skill.md|monitored-app]] continue to apply unchanged.

## MUST NOT
- No feature MUST maintain its own copy of "is logged in"/"current user"/permissions state — every read goes through `auth.selectors.ts` or `libs/shared/auth-ui`.
- Permission guards MUST NOT be centralized in `apps/platform-shell`'s root routes — each feature attaches its own.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Persisting the access token to storage "to survive reloads more simply"**
  - Consequence: reintroduces the XSS exposure the in-memory token strategy exists to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow
- **Centralizing all permission guards in the shell's root routes "for visibility"**
  - Consequence: reintroduces the coupling hierarchical route ownership was designed to prevent
  - Instead: each feature attaches its own guards to its own routes
- **An embeddable app implementing its own login screen "just in case" the platform session is missing**
  - Consequence: duplicates authentication logic across teams, creates two different ways a user could end up authenticated
  - Instead: the embeddable app only ever reads `SessionContract`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN a user without the required permission navigates to a guarded route THEN
  - [ ] navigation redirects to the forbidden route
- [ ] WHEN the platform's session expires THEN
  - [ ] `SessionContract.isAuthenticated` becomes `false` for every embeddable app reading it
- [ ] WHEN the codebase is searched for `localStorage`/`sessionStorage` writes of the access token THEN
  - [ ] none are found

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]
