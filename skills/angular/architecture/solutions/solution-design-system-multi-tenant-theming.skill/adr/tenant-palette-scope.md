---
name: tenant-palette-scope
description: What a tenant file is allowed to change — only the Material colour system tokens (and tenant-specific --ds-* colours), via a shared ds-tenant-theme mixin, never typography, density, or base component styles
problem: A tenant file could re-emit a full mat.theme() (color + typography + density + base styles) scoped to its selector, or override only a subset. Re-emitting everything per tenant multiplies CSS weight and lets tenants drift on non-colour concerns that should stay uniform.
decision: A tenant is a colour palette and nothing else. Each tenant file @includes one shared ds-tenant-theme mixin that emits only mat.theme((color: ...)) scoped to :root[data-tenant='<id>'], plus optional --ds-* colour overrides. Typography, density, and Material's base styles are emitted once by the base theme.scss and never repeated. light-dark() is preserved inside every tenant.
tags:
  - solution/design-system-multi-tenant-theming
  - stack/typescript
  - framework/angular-material
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Given the `[data-tenant]` selector mechanism (see [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/adr/tenant-resolution-strategy.md|tenant-resolution-strategy]]), a tenant file still has latitude in *what* it emits under that selector:

- a full `mat.theme((color, typography, density))` — re-declaring every system token and Material's base component styles per tenant;
- only the colour keys of `mat.theme()`;
- a hand-authored set of `--mat-sys-*` values;
- a token map compiled to CSS by a custom script.

The wider the tenant's surface, the more CSS ships per tenant and the more a tenant can diverge on things (font scale, spacing rhythm, component density) that the design system wants to keep identical across all brands.

# Selected variant

**Selected variant:** [[#Colour-only, via a shared ds-tenant-theme mixin (selected)]]

# Searched variants

## Colour-only, via a shared ds-tenant-theme mixin (selected)

### Description

`styles/tenants/_tenant-theme.scss` defines `@mixin ds-tenant-theme($primary, $ds-overrides: ())`. The mixin body calls `mat.theme((color: (theme-type: color-scheme, primary: $primary)))` and then emits each entry of `$ds-overrides` as a custom property. Each tenant file (`_acme.scss`, `_globex.scss`, …) is:

```scss
@use '@angular/material' as mat;
@use './tenant-theme' as tenant;

:root[data-tenant='globex'] {
  @include tenant.ds-tenant-theme(
    mat.$cyan-palette,
    ( --ds-color-status-in-progress: light-dark(#00838f, #4dd0e1) )
  );
}
```

`styles/theme.scss` still emits the full `mat.theme((color, typography, density))` once at bare `:root` — that is the default brand and the sole source of typography, density, and Material base styles.

### Benefits

- **Minimal CSS per tenant** — only the colour system tokens are re-declared under each tenant selector; typography and density (large outputs) and Material's base component rules are emitted exactly once.
- **Tenants cannot drift on non-colour concerns** — a tenant file physically has no way to change the font scale or density because the mixin does not expose those keys. Brand consistency on everything except colour is structural, not a review rule.
- **One place calls `mat.theme()` for tenants** — the mixin. A change to how a tenant palette is emitted (a new Material option, a tertiary palette) is a one-line mixin edit, not N tenant-file edits.
- **`light-dark()` preserved** — the mixin passes `theme-type: color-scheme`, so Material emits `light-dark()` colour values under the tenant selector exactly as at `:root`; `--ds-*` overrides are authored with `light-dark()` by convention.
- **Palette input is a Material palette** — `mat.$cyan-palette` or a `mat.define-palette(...)` — so the tonal ramp is generated correctly, not hand-authored.

### Costs

- A tenant that genuinely needs a different type scale cannot express it here — it would need a deliberate extension to the mixin (and an ADR revision). Accepted: that is the point; if such a requirement is real it deserves the scrutiny.
- Re-calling `mat.theme((color: ...))` under a second selector still emits the full colour token set (not a diff against `:root`). Accepted: the colour set is the small part of `mat.theme()`'s output, and a partial override would require enumerating exactly which tokens derive from the palette — the same fragility the resolution-strategy ADR rejected for the JS approach.

## Full mat.theme() per tenant

### Description

Each tenant file emits `mat.theme((color, typography, density))` in full under its selector.

### Benefits

- A tenant is completely self-contained; no dependency on what `:root` emitted.
- A tenant could, if ever needed, vary typography or density.

### Costs

- Multiplies the largest parts of `mat.theme()`'s output (typography scale, density rules, base component styles) by the tenant count, for values that are identical across tenants.
- Removes the structural guarantee that tenants differ only by colour — now a review rule, routinely missed.

## Hand-authored --mat-sys-* values per tenant

### Description

Each tenant file sets the ~30 colour custom properties directly, without `mat.theme()`.

### Benefits

- Absolute control over every token; smallest possible output.

### Costs

- The M3 tonal palette (primary, secondary, tertiary, neutral, their containers, on-colours, at multiple tones) is dozens of interrelated values — authoring them by hand per tenant is error-prone and loses Material's palette generation.
- Breaks whenever Material adds or renames a colour token — the exact drift the resolution-strategy ADR rejected for the JS approach, moved into SCSS.

## A token JSON per tenant compiled by a custom script

### Description

Each tenant is a `tenant.tokens.json`; a build script turns it into scoped CSS.

### Benefits

- Tenant definitions are data, potentially editable by non-engineers or a tenant-admin UI.

### Costs

- A bespoke build step in a repo whose whole `solution-design-system-structure` premise is a plain Angular CLI workspace with ng-packagr and nothing exotic.
- Still has to solve palette generation (either embed Material's algorithm in the script or ship raw tone values), reintroducing the hand-authored-palette problem.
