---
name: plateau-multi-tenant-design-system--class-tenant-theme
description: The shared ds-tenant-theme SCSS mixin in projects/design-system/styles/tenants/ — the one place mat.theme() is called for a tenant; emits colour system tokens plus optional --ds-* colour overrides, all through light-dark() (VP1) — multi-tenant-design-system plateau
domain: skill
type: template
whenToUse: when creating or editing styles/tenants/_tenant-theme.scss, or reviewing what a tenant file is allowed to change (VP1)
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

> `projects/design-system/styles/tenants/_tenant-theme.scss` — a partial, `@use`d only by the per-tenant files. Ships inside the `styles/tenants/` asset directory.

# Goal

- Give every tenant file one entry point so a change to how a tenant palette is emitted is a single edit
- Constrain a tenant to **colour only** — the mixin exposes a palette and `--ds-*` colour overrides, nothing else
- Preserve `light-dark()` inside every tenant

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenant-theme.scss.create.md|Tenants/tenant-theme.scss.create]]

# Core Principles

- Apply ONE plateau template per artifact
- The mixin passes only the `color` key to `mat.theme()` — never `typography` / `density`; those come once from `styles/theme.scss`
- `theme-type: color-scheme` is hard-coded so Material keeps emitting `light-dark()` under the tenant selector
- `$primary` is a Material palette map (`mat.$<name>-palette` or `mat.define-palette(...)`), not a single colour — so the M3 tonal ramp is generated

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/adr/tenant-palette-scope.md|tenant-palette-scope]]

# Naming convention

| use case | mixin name | file name |
| -------- | ---------- | --------- |
| Tenant colour-theme mixin | `ds-tenant-theme` | `_tenant-theme.scss` |

# Implementation

```scss
// Skill: class-tenant-theme
// Plateau: multi-tenant-design-system
// Version: 20260903200000
@use '@angular/material' as mat;

/// Emit one tenant's colour theme. Call inside a `:root[data-tenant='<id>']` block.
/// @param {Map} $primary - a Material palette (e.g. `mat.$cyan-palette`)
/// @param {Map} $ds-overrides - optional `--ds-*` colour custom properties, each a `light-dark(...)` value
@mixin ds-tenant-theme($primary, $ds-overrides: ()) {
  @include mat.theme(
    (
      color: (
        theme-type: color-scheme,
        primary: $primary,
      ),
    )
  );

  @each $name, $value in $ds-overrides {
    #{$name}: $value;
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenant-theme.scss.create.md|Tenants/tenant-theme.scss.create]]

# Rules

## MUST
- The mixin passes only the `color` key to `mat.theme()` — never `typography` or `density`.
- `theme-type: color-scheme` is always passed (hard-coded, not a parameter).
- `$ds-overrides` values are `light-dark()` expressions.
- `$primary` is a Material palette map, not a single colour.
- Never apply several plateau templates per artifact.
- Never add a parameter for typography, density, spacing, or radius — a tenant varies colour only.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenant-theme.scss.create.md|Tenants/tenant-theme.scss.create]]

# Check list

- [ ] The mixin body calls `mat.theme((color: (theme-type: color-scheme, primary: $primary)))` and nothing with `typography`/`density`
- [ ] `$ds-overrides` defaults to `()` and is emitted as custom properties
- [ ] The file is a partial (`_tenant-theme.scss`), `@use`d only by tenant files

# Unittest TestCases

- [ ] WHEN a tenant file includes `ds-tenant-theme(mat.$cyan-palette)` under `:root[data-tenant='x']` and the build runs THEN the compiled CSS scopes `--mat-sys-primary` (a cyan value) to that selector and leaves `--mat-sys-body-medium` untouched
- [ ] WHEN `$ds-overrides` is `(--ds-color-status-in-progress: light-dark(#00838f, #4dd0e1))` THEN the compiled CSS contains that custom property under the tenant selector

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenant-theme.scss.create.md|Tenants/tenant-theme.scss.create]]
