---
description: Extend projects/design-system with the styles/tenants/ folder, the ng-package.json asset entry for tenants.scss, and the DsTenant union in public-api
name: design-system
project_kind: library
element_kind: project
change_kind: extend
tags:
  - solution/design-system-multi-tenant-theming
  - element/design-system-project
---

# Goals

- Add the `styles/tenants/` layer to the publishable library without touching the existing `theme.scss` / `custom-tokens.scss` or any component
- Ship `tenants.scss` as a package asset so a consuming app can `@use 'design-system/styles/tenants'`
- Export a typed `DsTenant` contract from `public-api` so consumers set `data-tenant` against a known set

# Structure

## Workspace place

```
/projects/design-system
```

## Project Structure

```
/projects/design-system
  /styles
    tenants/
      _tenant-theme.scss
      _acme.scss
      _globex.scss
      tenants.scss
  /src/lib
    tenants.ts
  /src/public-api.ts        <- + export { DS_TENANTS } / export type { DsTenant }
  ng-package.json           <- assets += "./styles/tenants/tenants.scss"
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| styles/tenants/_tenant-theme.scss | The shared `ds-tenant-theme` mixin — colour only. | [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenant-theme.scss.create.md\|tenant-theme.scss]] |
| styles/tenants/_{tenant}.scss | One per tenant. | [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/{tenant}-palette.scss.create.md\|{tenant}-palette.scss]] |
| styles/tenants/tenants.scss | Aggregator; the package asset. | — |
| src/lib/tenants.ts | `DS_TENANTS` + `DsTenant`. | [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenants.ts.create.md\|tenants.ts]] |

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/material | already present | `mat.theme()` / `mat.$*-palette` for tenant palettes |

No new package.

## What Does NOT Belong Here

- Tenant *selection* logic (which tenant a user belongs to, reading it from a claim or an API) — a consuming-app concern.
- A runtime `applyTenant()` function — the mechanism is the CSS attribute, not JS.

## Allowed Dependencies

- unchanged — the library depends on `@angular/core`, `@angular/material`, and nothing in the workspace.

# Rules

## MUST
- `theme.scss` and `custom-tokens.scss` are not modified by this solution.
  - Risk: folding tenant logic into the base files makes the single-tenant plateau and the multi-tenant plateau diverge on the same file.
  - Fix: everything new lives under `styles/tenants/`.
- `ng-package.json` `assets` gains `./styles/tenants/tenants.scss` (the aggregator only, not the partials).
  - Risk: without the asset entry, `@use 'design-system/styles/tenants'` fails to resolve in a consumer; shipping the `_*.scss` partials individually invites consumers to import one tenant directly.
  - Fix: ship `tenants.scss`; the `_`-prefixed partials are `@use`d internally by it.
- `public-api.ts` exports `DS_TENANTS` and `DsTenant` and nothing else new.
  - Risk: exporting the SCSS or a helper widens the public TS surface with things that are not TS API.
  - Fix: the only TS the consumer needs is the tenant-id union.

# Check list

- [ ] `styles/tenants/` contains `_tenant-theme.scss`, one `_{tenant}.scss` per tenant, and `tenants.scss`
- [ ] `ng-package.json` `assets` lists `./styles/tenants/tenants.scss`
- [ ] `public-api.ts` exports `DS_TENANTS` + `DsTenant`
- [ ] `ng build design-system` places `styles/tenants/tenants.scss` in `dist/design-system/styles/tenants/`
- [ ] No component file references a tenant

# Unittest TestCases

- [ ] WHEN `ng build design-system` runs THEN
  - [ ] `dist/design-system/styles/tenants/tenants.scss` exists and `@use`s the tenant partials
- [ ] WHEN `public-api.ts` is imported THEN
  - [ ] `DS_TENANTS` is a readonly tuple and `DsTenant` is its element union
