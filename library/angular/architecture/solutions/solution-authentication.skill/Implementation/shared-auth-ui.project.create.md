---
description: Shared library for authentication-related UI primitives — currently the has-permission directive, with room for future auth UI components
name: shared-auth-ui
project_kind: library
element_kind: project
change_kind: create
tags:
  - solution/authentication
  - element/shared-auth-ui-project
---

# Goals

- Keep permission-based UI controls in a single reusable library so every feature can import them without duplicating auth logic

# Structure

## Project Structure

```
/libs/shared/auth-ui
  /src
    /lib
      has-permission.directive.ts
    index.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| has-permission.directive.ts | Structural directive `*hasPermission` that shows/hides UI based on the current user's permissions, per [[./UI/has-permission.directive.ts.create.md]]. |
| index.ts | Re-exports `HasPermissionDirective` as the public API of this library. |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @ngrx/store | matching the NgRx major version in use | `Store` injection used by `has-permission.directive.ts` to read permissions |

# Rules

## MUST
- The library never depends on feature libraries — only `libs/shared/state` (auth slice) and core Angular/NgRx.
  - Risk: a dependency on a feature makes `libs/shared/auth-ui` un-shareable and can create an import cycle.
  - Fix: the `scope:shared` boundary; the directive/guard read `selectPermissions` from `libs/shared/state` only.
- `HasPermissionDirective` (and, in v3.1, `requirePermission` + the login/forbidden pages) are the only things exported.
  - Risk: exporting internals invites consumers to couple to them, and blurs what the "auth UI primitives" contract is.
  - Fix: `index.ts` exports exactly the directive, the guard factory, and the two pages.
- Never contain business features or page components — only reusable auth UI primitives.
  - Risk: a feature dropped into `shared/auth-ui` becomes importable by everything, bypassing the feature boundary.
  - Fix: business features live under `libs/{feature}`; this lib holds only cross-cutting auth UI.
## SHOULD
- **Inlining the permission check into every component instead of using the directive** — Consequence: permission logic spreads across the codebase and becomes inconsistent — Instead: use `*hasPermission` from `libs/shared/auth-ui`

# Check list

- [ ] `libs/shared/auth-ui` is created as a publishable/usable shared library
- [ ] `index.ts` exports only `HasPermissionDirective`
- [ ] No feature-specific imports exist inside `libs/shared/auth-ui`

# Unittest TestCases

- [ ] WHEN the current user has the required permission THEN `*hasPermission` renders the template
- [ ] WHEN the current user lacks the required permission THEN `*hasPermission` clears the view
