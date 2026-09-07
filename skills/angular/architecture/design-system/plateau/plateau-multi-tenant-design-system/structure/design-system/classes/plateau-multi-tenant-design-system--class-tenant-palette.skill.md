---
name: plateau-multi-tenant-design-system--class-tenant-palette
description: Generic pattern for one tenant's palette file in projects/design-system/styles/tenants/ — a :root[data-tenant='<id>'] block that @includes ds-tenant-theme with the tenant's Material palette and any tenant-specific --ds-* colour overrides (VP1) — multi-tenant-design-system plateau
domain: skill
type: template
whenToUse: when adding a new tenant — creating styles/tenants/_{tenant}.scss, adding its @use line to tenants.scss, and its id to DS_TENANTS (VP1)
plateau: multi-tenant-design-system
artifact_type: stylesheet
version: 20260903200000
tags:
  - skill/template/class
  - plateau/multi-tenant-design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]]"
---

> `projects/design-system/styles/tenants/_{tenant}.scss` + one `@use` line in `styles/tenants.scss` (the package asset). Not tied to one tenant.

# Goal

- Define one tenant's brand colour, scoped to its `data-tenant` attribute value
- Keep the file to a single `@include ds-tenant-theme(...)` — no bare `mat.theme()`, no typography, no density

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/{tenant}-palette.scss.create.md|Tenants/{tenant}-palette.scss.create]]

# Core Principles

- Apply ONE plateau template per artifact
- One `:root[data-tenant='<id>']` block, one `@include ds-tenant-theme(...)`
- The selector attribute value equals the tenant's id in `DS_TENANTS` and the file stem
- `--ds-*` overrides are only for colours Material does not model, each a `light-dark()` value

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/adr/tenant-resolution-strategy.md|tenant-resolution-strategy]]

# Naming convention

| use case | file name pattern | file name | selector pattern | selector |
| -------- | ----------------- | --------- | ---------------- | -------- |
| Tenant palette partial | `_{tenant}.scss` | `_globex.scss` | `:root[data-tenant='{tenant}']` | `:root[data-tenant='globex']` |

# Implementation

```scss
// Skill: class-tenant-palette
// Plateau: multi-tenant-design-system
// Version: 20260903200000
@use '@angular/material' as mat;
@use './tenant-theme' as tenant;

:root[data-tenant='globex'] {
  @include tenant.ds-tenant-theme(
    mat.$cyan-palette,
    (
      --ds-color-status-in-progress: light-dark(#00838f, #4dd0e1),
    )
  );
}
```

Then in `styles/tenants.scss`:

```scss
@use 'tenants/acme';
@use 'tenants/globex';
```

And in `src/lib/tenants.ts`: add `'globex'` to `DS_TENANTS`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/{tenant}-palette.scss.create.md|Tenants/{tenant}-palette.scss.create]]

# Rules

## MUST
- The file contains exactly one `:root[data-tenant='<id>']` block and one `@include tenant.ds-tenant-theme(...)`.
- The selector attribute value equals the tenant's `DS_TENANTS` id and the file stem (the same kebab string).
- `--ds-*` overrides are only `--ds-color-*` with `light-dark()` values — never `--ds-spacing-*` / `--ds-radius-*`.
- A new tenant is added by creating the file **and** adding its `@use` line to `tenants.scss` **and** its id to `DS_TENANTS` — all in the same change.
- Never apply several plateau templates per artifact.
- Never put a bare `mat.theme()` or extra rules in a tenant file — everything goes through `ds-tenant-theme`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/{tenant}-palette.scss.create.md|Tenants/{tenant}-palette.scss.create]]

# Check list

- [ ] One `:root[data-tenant='<id>']` block, one `@include`
- [ ] The attribute value matches the `DsTenant` entry and the file stem
- [ ] Overrides are `--ds-color-*` with `light-dark()` values
- [ ] `tenants.scss` `@use`s the file and `DS_TENANTS` lists the id

# Unittest TestCases

- [ ] WHEN `data-tenant='globex'` is on `<html>` and a component using `--mat-sys-primary` renders THEN its resolved colour is the globex palette's primary, in both light and dark
- [ ] WHEN `data-tenant` is removed THEN the same component falls back to the base brand palette

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/{tenant}-palette.scss.create.md|Tenants/{tenant}-palette.scss.create]]
