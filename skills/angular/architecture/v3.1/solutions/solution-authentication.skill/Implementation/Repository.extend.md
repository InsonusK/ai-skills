---
description: Extend the base workspace with conventions for auth guards, the token-attaching interceptor, and permission-checking directives
element_kind: repository
change_kind: extend
tags:
  - solution/authentication
  - element/monolith-repository
---

# Structure

No new top-level directories. This extension adds artifact-placement conventions on top of [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create]] and extends the `auth` slice already created by this solution's [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md|auth.store.ts]].

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /libs/shared/state/src/lib/auth | Extended (not recreated) with: in-memory access token field, permission list field, silent-refresh trigger. See [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/GlobalStore/auth.store.ts.create.md]]. |
| /libs/shared/auth-ui | New lib, tagged `type:util`, `scope:shared`: hosts the permission-checking structural directive and any shared "not authorized" presentational pieces. |
| /libs/{feature}/feature/src/lib/**/*.guard.ts | Route guards live inside the feature they protect, following the functional guard pattern in [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/Implementation/Routing/{feature}.guard.ts.create]]. |

# Rules

## MUST
- Any code that checks "is the user allowed to do X" MUST express the check in terms of a permission string (see [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/authorization-model.md|authorization-model]]), never a role name.
- The access token MUST only ever be held in the `shared-state` auth slice's in-memory field — it MUST NOT be written to `localStorage`, `sessionStorage`, or any other persistent client storage (see [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/token-storage-strategy.md|token-storage-strategy]]).

# Unittest TestCases

- [ ] WHEN the codebase is searched for `localStorage`/`sessionStorage` writes of a token value THEN
  - [ ] none are found
- [ ] WHEN any guard/directive is inspected THEN
  - [ ] its authorization check references a permission string, not a role name

## SHOULD
- **A feature checking `currentUser.role === 'admin'` instead of a permission** — Consequence: couples feature code to the platform's specific role taxonomy, breaking the decoupling this solution's authorization-model ADR exists to provide, and makes the check impossible to reuse from an embeddable app that doesn't know the platform's role names — Instead: check a permission string (e.g. `'orders.delete'`) delivered by the backend as part of the session
