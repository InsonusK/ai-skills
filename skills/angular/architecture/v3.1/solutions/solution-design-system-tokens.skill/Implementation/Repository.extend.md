---
description: Extend the design system repository with the M3 theme definition and the custom --ds-* token layer
element_kind: repository
change_kind: extend
tags:
  - solution/design-system-tokens
  - element/design-system-repository
---

# Structure

## Workspace Structure

```
/projects/design-system
  /src
    /styles
      theme.scss              <- mat.theme() definition, light/dark via light-dark()
      custom-tokens.scss       <- --ds-* tokens for gaps: semantic status colors, spacing, radius
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| /src/styles/theme.scss | The single `mat.theme()` definition (one fixed brand palette, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/adr/brand-theming-scope.md|brand-theming-scope]]), applied at the root selector. Colors use `light-dark()` internally, per Material's own M3 implementation. |
| /src/styles/custom-tokens.scss | `--ds-*` custom properties for concepts Material's token set doesn't cover: semantic status/priority colors (high/medium/low, stop/start/in-progress, etc.), spacing scale, radius scale — each defined with `light-dark()` per [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/adr/light-dark-mode-strategy.md|light-dark-mode-strategy]]. |

# Rules

## MUST
- Every custom component consumes `--mat-sys-*` tokens directly for colors/typography/elevation — never a `--ds-color-primary`-style alias for something Material already models.
  - Risk: an alias layer adds indirection with no naming gain and leaves two names for one concept that can drift.
  - Fix: reference `--mat-sys-primary` directly; per [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/adr/token-consumption-strategy.md|token-consumption-strategy]].
- `--ds-*` tokens are introduced only for concepts with no Material equivalent — never as a passthrough alias for an existing `--mat-sys-*` token.
  - Risk: passthrough `--ds-*` tokens balloon the token surface and hide which values are truly design-system-owned.
  - Fix: `--ds-*` only for domain semantic colors, spacing scale, radius scale.
- Every color token — Material's `--mat-sys-*` and this library's `--ds-*` — uses `light-dark()`.
  - Risk: a token with a single fixed value renders wrong in one scheme, and a JS class-toggle theme switch creeps back in.
  - Fix: `light-dark(<light>, <dark>)` for every color value; per [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/adr/light-dark-mode-strategy.md|light-dark-mode-strategy]].
- Token values are overridden only via Material's Sass override mixins — never by hand-setting `--mat-*` custom properties in raw CSS.
  - Risk: hand-set `--mat-*` bypasses the API's spelling/forward-compat validation and is upgrade-hostile when Material renames a token.
  - Fix: `mat.theme-overrides(...)` or the component's own override mixin.
- No palette-swapping or per-tenant theming mechanism is introduced — one fixed brand palette everywhere.
  - Risk: a theming hook adds runtime palette state the whole component layer must then account for.
  - Fix: a single `mat.theme()` at the root; per [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/adr/brand-theming-scope.md|brand-theming-scope]].
# Unittest TestCases

- [ ] WHEN a custom component needs a Material-modeled concept (color, typography, elevation) THEN
  - [ ] it references the corresponding `--mat-sys-*` token directly, with no `--ds-*` alias
- [ ] WHEN the theme is toggled between light and dark (OS preference change) THEN
  - [ ] every color token, both `--mat-sys-*` and `--ds-*`, resolves to the correct variant via `light-dark()`, without any JavaScript-driven class toggle

## SHOULD
- **Defining `--ds-color-primary: var(--mat-sys-primary)` "for consistency"** — Consequence: adds an indirection layer with no naming improvement over the already-semantic `--mat-sys-primary`, and now two names exist for the same concept — Instead: reference `--mat-sys-primary` directly; reserve `--ds-*` for concepts Material doesn't model
- **Hand-setting a `--mat-*` custom property directly in a component's raw CSS to achieve a one-off visual tweak** — Consequence: bypasses the Sass overrides API's validation (correct spelling, forward-compatibility if a token is renamed in a future Material version), and is exactly the "upgrade-hostile" pattern Angular Material's own guidance warns against — Instead: use the appropriate override mixin (`mat.theme-overrides`, or the specific component's override mixin)
