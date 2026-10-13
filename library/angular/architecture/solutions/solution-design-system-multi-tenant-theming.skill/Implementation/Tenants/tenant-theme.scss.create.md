---
description: The shared ds-tenant-theme SCSS mixin — the one place mat.theme() is called for a tenant; emits colour system tokens plus optional --ds-* colour overrides, all through light-dark()
project_name: design-system
name: tenant-theme
element_kind: style
change_kind: create
tags:
  - solution/design-system-multi-tenant-theming
  - element/tenant-theme-scss
---

# How this generic file is used

Create once at `projects/design-system/styles/tenants/_tenant-theme.scss`. Every tenant file `@use`s it; nothing else calls `mat.theme()` for a tenant.

# Goals

- Give every tenant file one entry point so a change to how a tenant palette is emitted is a single edit
- Constrain a tenant to colour only — the mixin exposes a palette and `--ds-*` colour overrides, nothing else
- Preserve `light-dark()` inside every tenant

# Implementation changes

File: `projects/design-system/styles/tenants/_tenant-theme.scss`

```scss
// Skill: class-tenant-theme
// Plateau: multi-tenant-design-system
// Version: <UTC timestamp at generation>
@use '@angular/material' as mat;

/// Emit one tenant's colour theme. Call inside a `:root[data-tenant='<id>']` block.
/// @param {Map} $primary  - a Material palette (e.g. `mat.$cyan-palette`) or `mat.define-palette(...)`
/// @param {Map} $ds-overrides - optional `--ds-*` colour custom properties, each a `light-dark(...)` value
@mixin ds-tenant-theme($primary, $ds-overrides: ()) {
  // colour only — no `typography` / `density` keys, so the base theme.scss stays the
  // sole source of those. `theme-type: color-scheme` keeps Material emitting light-dark().
  @include mat.theme((
    color: (
      theme-type: color-scheme,
      primary: $primary,
    ),
  ));

  @each $name, $value in $ds-overrides {
    #{$name}: $value;
  }
}
```

# Rule changes

## MUST
- The mixin passes only the `color` key to `mat.theme()` — never `typography` or `density`.
  - Risk: exposing those keys lets a tenant change the font scale or component density, which must stay uniform across brands, and re-emits large token sets per tenant.
  - Fix: the signature is `($primary, $ds-overrides)` — there is no parameter for anything but colour.
- `theme-type: color-scheme` is always passed.
  - Risk: without it Material emits fixed light-only or dark-only values and the tenant stops adapting to the OS scheme.
  - Fix: it is hard-coded in the mixin body, not a parameter.
- `$ds-overrides` values are `light-dark()` expressions.
  - Risk: a plain hex in an override renders wrong in one scheme.
  - Fix: the mixin documents it; every tenant file passes `light-dark(<light>, <dark>)`.
- `$primary` is a Material palette map, not a single colour.
  - Risk: a single hex cannot generate the M3 tonal ramp (containers, on-colours, tones).
  - Fix: pass `mat.$<name>-palette` or `mat.define-palette($custom-palette-map)`.

# Check list

- [ ] The mixin body calls `mat.theme((color: (theme-type: color-scheme, primary: $primary)))` and nothing with `typography`/`density`
- [ ] `$ds-overrides` defaults to `()` and is emitted as custom properties
- [ ] The file is a partial (`_tenant-theme.scss`), `@use`d only by tenant files

# Unittest TestCases

- [ ] WHEN a tenant file includes `ds-tenant-theme(mat.$cyan-palette)` under `:root[data-tenant='x']` and the build runs THEN
  - [ ] the compiled CSS scopes `--mat-sys-primary` (a cyan value) to `:root[data-tenant='x']` and leaves `--mat-sys-body-medium` untouched
- [ ] WHEN `$ds-overrides` is `(--ds-color-status-in-progress: light-dark(#00838f, #4dd0e1))` THEN
  - [ ] the compiled CSS contains `--ds-color-status-in-progress: light-dark(#00838f, #4dd0e1)` under the tenant selector
