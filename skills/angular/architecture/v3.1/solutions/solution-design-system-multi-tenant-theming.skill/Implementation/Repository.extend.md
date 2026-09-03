---
description: Extend the design system repository with a styles/tenants/ layer — one SCSS file per tenant scoped to :root[data-tenant='<id>'], a shared ds-tenant-theme mixin, and an exported DsTenant union; the base theme.scss stays the default brand
element_kind: repository
change_kind: extend
tags:
  - solution/design-system-multi-tenant-theming
  - element/design-system-repository
---

# Structure

## Workspace Structure

```
/projects/design-system
  /styles
    theme.scss                <- unchanged — the default brand + the sole source of typography/density
    custom-tokens.scss         <- unchanged
    tenants/
      _tenant-theme.scss       <- @mixin ds-tenant-theme($primary, $ds-overrides) — colour only, light-dark() preserved
      _acme.scss               <- :root[data-tenant='acme']  { @include ds-tenant-theme(...) }
      _globex.scss             <- :root[data-tenant='globex'] { @include ds-tenant-theme(...) }
      tenants.scss             <- entrypoint: @use './acme'; @use './globex'; — consumed as design-system/styles/tenants
  /src/lib
    tenants.ts                 <- export const DS_TENANTS + export type DsTenant (the valid-tenant contract)
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| /styles/tenants/_tenant-theme.scss | The one place `mat.theme((color: ...))` is called for a tenant. Per [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenant-theme.scss.create.md|tenant-theme.scss]]. |
| /styles/tenants/{tenant}-palette (`_acme.scss`, `_globex.scss`) | One file per tenant — a `:root[data-tenant='<id>']` block that `@include`s the mixin. Per [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/{tenant}-palette.scss.create.md|{tenant}-palette.scss]]. |
| /styles/tenants/tenants.scss | Aggregates every tenant file; shipped as a package asset, `@use`d once by the consuming app after `theme`. |
| /src/lib/tenants.ts | `DS_TENANTS` tuple + `DsTenant` union — the design system's contract for which tenant ids exist. Per [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Tenants/tenants.ts.create.md|tenants.ts]]. |

# Rules

## MUST
- `theme.scss` is left exactly as `solution-design-system-tokens` wrote it — the bare-`:root` default brand and the only place typography, density, and Material base styles are emitted.
  - Risk: moving the default palette into a tenant file means a consumer with no `data-tenant` set gets an un-themed page.
  - Fix: the base theme stays the fallback; tenant files only add more-specific colour overrides.
- A tenant is expressed as a `:root[data-tenant='<id>']` rule set that changes only colour tokens.
  - Risk: a tenant file that re-emits typography or density lets brands drift on concerns that must stay uniform, and multiplies CSS weight.
  - Fix: every tenant file goes through `ds-tenant-theme`, which exposes only a palette and `--ds-*` colour overrides; per [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/adr/tenant-palette-scope.md|tenant-palette-scope]].
- The active tenant is selected by a CSS attribute (`document.documentElement.dataset.tenant`), never by JavaScript that rewrites custom properties or by a per-tenant CSS bundle.
  - Risk: a JS token-rewrite is the theme toggle the base token ADR forbids; a per-tenant bundle breaks federation single-instance sharing.
  - Fix: ship the tenants stylesheet; the consuming app sets one attribute; per [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/adr/tenant-resolution-strategy.md|tenant-resolution-strategy]].
- Every colour value in a tenant file uses `light-dark()`.
  - Risk: a fixed colour in a tenant renders wrong in one scheme while the rest of that tenant adapts.
  - Fix: `ds-tenant-theme` passes `theme-type: color-scheme` to Material; `--ds-*` overrides are authored `light-dark(<light>, <dark>)`.
- `tenants.scss` is added to `ng-package.json` `assets` and consumed as `@use 'design-system/styles/tenants'` after `@use 'design-system/styles/theme'`.
  - Risk: `@use`d before `theme`, the base `:root` block would win the cascade over the equally-or-less-specific parts and the default could bleed through; not shipped as an asset, consumers cannot import it.
  - Fix: asset entry + documented import order.

# Unittest TestCases

- [ ] WHEN `document.documentElement` has no `data-tenant` attribute THEN
  - [ ] `getComputedStyle(document.documentElement).getPropertyValue('--mat-sys-primary')` is the base brand value
- [ ] WHEN `data-tenant='globex'` is set THEN
  - [ ] `--mat-sys-primary` resolves to the globex palette value, and typography / density tokens are unchanged
- [ ] WHEN a `ds-*` component is rendered under each tenant THEN
  - [ ] its source contains no tenant name and no tenant-specific value — only `--mat-sys-*` / `--ds-*`

## SHOULD
- **Shipping a per-tenant compiled stylesheet the consuming app injects via `<link>`** — Consequence: a guaranteed flash of the default palette until the stylesheet loads, plus a `<link>`-orchestration layer every consumer reimplements — Instead: one tenants stylesheet in the package, one attribute the consumer sets before first paint
- **Adding a `tenant` input to a component so it can "render for a specific tenant"** — Consequence: components become tenant-aware and the token indirection that makes them reusable breaks — Instead: the component consumes tokens; the attribute on `<html>` decides the values
