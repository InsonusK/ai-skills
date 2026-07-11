---
name: repo-authenticated
description: Nx workspace layout for the authenticated Angular application — adds libs/shared/auth-ui, extends the auth slice with in-memory token/permission state and silent refresh, a global auth interceptor, and feature-owned permission guards, on top of the data-capable plateau's routing/state/data-access conventions
domain: skill
type: template
plateau: authenticated
version: 20260711150000
tags:
  - skill/template/repo
  - plateau/authenticated
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]]"
---

> **Deferred scope:** `solution-app-routing`'s embeddable-module routing slice remains excluded, deferred to a future "platform" plateau (see [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]]). `solution-authentication` also has one implementation slice scoped to platform-embeddability: `Implementation/EmbeddableApp/platform-contracts.extend.md`, which extends the `@platform/contracts` package (owned by the future platform-embeddability solution) so an embeddable app can read the platform's session (`SessionContract`) without authenticating on its own. Module Federation is not part of this plateau or any plateau up to and including [[skills/angular/architecture/plateau/tested/plateau-tested.skill.md|tested]], so that slice is deliberately excluded here and deferred to the future "platform" plateau, alongside `@platform/contracts` itself. Everything else `solution-authentication` defines — the extended auth slice, the auth interceptor, feature-owned permission guards, and `libs/shared/auth-ui` — works unchanged without federation, since a single-application session model needs no cross-app contract.

# Structure

## Workspace Structure

```
/apps
  /[platform-shell](./platform-shell/project-platform-shell.skill.md)

/libs
  /shared
    /[ui](./shared-ui/project-shared-ui.skill.md)
    /[util](./shared-util/project-shared-util.skill.md)
    /[state](./shared-state/project-shared-state.skill.md)
    /[http-core](./shared-http-core/project-shared-http-core.skill.md)
    /[auth-ui](./shared-auth-ui/project-shared-auth-ui.skill.md)      <- new (solution-authentication)
  /{feature}
    /[feature](./feature-feature/project-feature-feature.skill.md)
    /[data-access](./feature-data-access/project-feature-data-access.skill.md)
```

- `libs/shared/auth-ui` hosts permission-based UI primitives — currently `*hasPermission`, with room for future auth UI components. Tagged `type:util`, `scope:shared`.
- `libs/shared/state`'s `auth` slice is extended (not recreated) with an in-memory access token, a granular permission list, and a silent-refresh-in-progress flag.
- Every routable feature that needs to restrict access to one of its own routes now also defines a permission guard at that route, inside its own `{feature}.routes.ts` — never centralized in the shell.

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[platform-shell/project-platform-shell.skill.md\|project-platform-shell.skill]] | The only deployable unit at this plateau. Composition root: bootstraps the app, owns top-level routing and the selective preloading strategy, registers root providers (including the global auth interceptor). Contains no business logic of its own. |
| /libs/shared/ui | [[shared-ui/project-shared-ui.skill.md\|project-shared-ui.skill]] | Reusable, app-specific UI composed from design-system primitives. No feature-specific business logic. |
| /libs/shared/util | [[shared-util/project-shared-util.skill.md\|project-shared-util.skill]] | Framework-agnostic pure helpers shared across features. |
| /libs/shared/state | [[shared-state/project-shared-state.skill.md\|project-shared-state.skill]] | Classical NgRx Store for global, cross-cutting state. Its `auth` slice now owns the full session lifecycle: in-memory token, permissions, silent refresh. |
| /libs/shared/http-core | [[shared-http-core/project-shared-http-core.skill.md\|project-shared-http-core.skill]] | Base HTTP service shared by every feature's Client — base URL, timeout, retry. Feature-agnostic. |
| /libs/shared/auth-ui | [[shared-auth-ui/project-shared-auth-ui.skill.md\|project-shared-auth-ui.skill]] | Permission-based UI primitives (`*hasPermission`), shared by every feature. |
| /libs/{feature}/feature | [[feature-feature/project-feature-feature.skill.md\|project-feature-feature.skill]] | Generic template: routed, presentational + container components (including forms) for one feature, its feature-level Signal Store, its own root-relative routes (now including any permission guards it needs), and lazy sub-splitting. |
| /libs/{feature}/data-access | [[feature-data-access/project-feature-data-access.skill.md\|project-feature-data-access.skill]] | Generic template: this feature's Facade/Client/Mapper/Errors layering for HTTP data operations. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

## Nx Tag Taxonomy

No new tag values are introduced; `libs/shared/auth-ui` uses the existing `type:util`/`scope:shared` combination.

`@nx/enforce-module-boundaries` allow-list (unchanged from [[skills/angular/architecture/plateau/data-capable/plateau-data-capable.skill.md|data-capable]]):

| type | may depend on |
| ----- | -------------- |
| `app` | any `type:feature` with matching or `scope:platform` |
| `feature` | `type:data-access` with the same `scope`, `type:ui` with `scope:shared`, `type:util` with `scope:shared`, `type:store` with `scope:shared` |
| `data-access` | `type:util` with `scope:shared` |
| `ui` (scope:shared) | `type:util` with `scope:shared` |
| `util` (scope:shared) | nothing (leaf) |
| `store` (scope:shared) | `type:util` with `scope:shared` |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

## Session & authorization conventions (cross-cutting convention)

- Any code that checks "is the user allowed to do X" MUST express the check as a permission string (e.g. `'orders.delete'`), never a role name.
- The access token is only ever held in `shared-state`'s auth slice's in-memory field — it MUST NOT be written to `localStorage`, `sessionStorage`, or any other persistent client storage.
- Route guards live inside the feature they protect — see [[feature-feature/classes/class-feature-guard.skill.md|class-feature-guard.skill]] — never centralized in `apps/platform-shell`.
- The auth interceptor — see [[shared-state/classes/class-auth-interceptor.skill.md|class-auth-interceptor.skill]] — is the only place a request is decorated with the `Authorization` header.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]

## Three-tier state placement, hierarchical routing, selective preloading, Signal Forms, data-access layering (cross-cutting conventions)

Unchanged from [[skills/angular/architecture/plateau/data-capable/plateau-data-capable.skill.md|data-capable]].

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|LocalState/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- Every Nx project MUST declare exactly one `type:*` tag and exactly one `scope:*` tag.
- Every lib MUST expose its public API through a single `index.ts` barrel.
- A `type:feature` project MUST NOT import another `type:feature` project directly.
- A `type:data-access` project MUST only be imported by the `type:feature` project that shares its `scope`.
- Business logic MUST NOT live in `apps/platform-shell`.
- `libs/shared/state` MUST NOT depend on any `type:feature` or `type:data-access` project.
- Every routable `type:feature` project MUST export its `Routes` array from `index.ts`, relative to its own root only.
- The mounting project MUST assign the root segment.
- Every `type:app` and routable `type:feature` project MUST declare an enforced bundle budget.
- New forms MUST use Signal Forms by default; submission MUST go through `submitForm()`.
- A feature's `{feature}.client.ts` MUST NOT be exported from that feature's `index.ts`.
- A Client MUST catch every `HttpErrorResponse` and rethrow a typed domain error.
- Any code that checks "is the user allowed to do X" MUST express the check as a permission string, never a role name.
- The access token MUST only ever be held in the `shared-state` auth slice's in-memory field — MUST NOT be written to any persistent client storage.
- `accessToken` MUST be held only in the auth slice's in-memory NgRx state.
- `Silent Refresh Requested` MUST be dispatched once during application bootstrap, before any authenticated request is made.
- `permissions` MUST be a flat array of permission strings, never role names.
- The auth interceptor MUST be the only place an outgoing request is decorated with the `Authorization` header.
- On a 401, the auth interceptor MUST dispatch `Silent Refresh Requested` rather than immediately logging the user out.
- The auth interceptor MUST NOT be applied to the silent-refresh request itself.
- A permission guard MUST be attached inside the feature's own routes, at the specific path it protects — never centralized in `apps/platform-shell`.
- A failed permission check MUST redirect to a forbidden/not-authorized route, not silently fail navigation.

## SHOULD
- New business features SHOULD be scaffolded as a `{feature}/feature` + `{feature}/data-access` pair from the start.
- Bundle budget thresholds SHOULD be reviewed deliberately when a feature's legitimate size grows.

## SHOULD NOT
- Existing Reactive Forms code SHOULD NOT be migrated to Signal Forms purely for consistency.

## MUST NOT
- MUST NOT place a routed business feature directly under `/apps`.
- MUST NOT add a `type:util` project with any `scope:*` other than `shared`.
- A `type:feature` project MUST NOT reach into another feature's Signal Store directly.
- A component or Signal Store method MUST NOT import a feature's Client directly, bypassing the Facade.
- A feature MUST NOT set `preload: true` on its own routes.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-api-http-layer.skill/solution-api-http-layer.skill|solution-api-http-layer]] - [[skills/angular/architecture/solutions/solution-api-http-layer.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]

# Anti-patterns

- **Single flat lib per feature instead of `feature` + `data-access` split**
  - Consequence: UI and HTTP/data concerns become entangled
  - Instead: always split into at least `feature` and `data-access` from the start
- **A feature checking `currentUser.role === 'admin'` instead of a permission**
  - Consequence: couples feature code to the platform's specific role taxonomy, and makes the check impossible to reuse from a future embeddable app that doesn't know the platform's role names
  - Instead: check a permission string (e.g. `'orders.delete'`) delivered by the backend as part of the session
- **Persisting `accessToken` to storage "to survive reloads more simply"**
  - Consequence: reintroduces exactly the XSS exposure the token-storage-strategy ADR chose in-memory storage to avoid
  - Instead: rely on the silent-refresh-on-bootstrap flow to repopulate the in-memory token after a reload
- **Retrying the original request indefinitely on repeated 401s**
  - Consequence: infinite retry loop if the refresh itself is failing
  - Instead: attempt exactly one silent refresh per 401; if it also fails, treat the session as expired
- **Centralizing all permission guards in the shell's root routes "for visibility"**
  - Consequence: reintroduces the coupling hierarchical route ownership was designed to prevent — the shell would need to know which specific feature paths require which permissions
  - Instead: each feature attaches its own guards to its own routes, using the shared `requirePermission` factory
- **Relying on `*hasPermission` alone to protect a destructive action, with no server-side check**
  - Consequence: a user could still trigger the action by calling the API directly, bypassing the UI entirely
  - Instead: treat this directive purely as UI polish; the backend remains the actual authorization boundary

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create|Routing/{feature}.guard.ts.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/UI/has-permission.directive.ts.create|UI/has-permission.directive.ts.create]]

# Unittest TestCases

- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN the codebase is searched for `localStorage`/`sessionStorage` writes of a token value THEN
  - [ ] none are found
- [ ] WHEN any guard/directive is inspected THEN
  - [ ] its authorization check references a permission string, not a role name
- [ ] WHEN the app bootstraps with a valid refresh cookie THEN
  - [ ] `Silent Refresh Succeeded` populates `accessToken` and `permissions`
- [ ] WHEN a request receives a 401 THEN
  - [ ] `Silent Refresh Requested` is dispatched exactly once

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.extend|GlobalStore/auth.store.ts.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/HttpLayer/auth.interceptor.ts.create|HttpLayer/auth.interceptor.ts.create]]
