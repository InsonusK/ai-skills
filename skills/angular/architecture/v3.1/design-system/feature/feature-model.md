# Feature Model — design-system

An independently versioned Angular component library, published as an npm package and consumed as a plain npm dependency by [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md|`monolith/`]] and as a version-negotiated federation singleton by [[skills/angular/architecture/v3.1/platform-host/feature/feature-model.md|`platform-host/`]] and [[skills/angular/architecture/v3.1/embeddable-app/feature/feature-model.md|`embeddable-app/`]]. It lives in its own repository with its own release cadence. Derived from the V1 `plateau-design-system` (4 solutions).

The **root product is `DesignSystem`** — you build one per design-system repository. The four V1 features are common; the family has **one Variation Point, `MultiTenantTheming`** (single fixed brand palette vs. swappable per-tenant palettes), realized by [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]].

Built per [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]]. See [[skills/angular/architecture/v3.1/README.md|the catalog overview]].

## The common baseline this model assumes (concretely)

```
angular.json                                   (Angular CLI multi-project workspace — NOT Nx)
package.json                                   (ng-packagr, @changesets/cli, @angular/material)
.changeset/config.json                         (Changesets — every public-surface change ships a changeset, CI-enforced)
projects/
  design-system/                               (the publishable library — ng-packagr, Angular Package Format, Ivy partial)
    src/styles/theme.scss                       (single mat.theme() at the root selector)
    src/styles/custom-tokens.scss               (--ds-* tokens for gaps Material does not model)
    src/lib/{component}/ds-{component}.component.ts
    src/public-api.ts
  demo/                                        (preview app — imports design-system by its published path, never published)
```

`projects/demo` is the only non-library project. There is no Nx, no `apps/`/`libs/`, no federation config in the design-system repo itself — federation is a *consumer* concern (`platform-host/`'s `HostDesignSystemConsumption`, `embeddable-app/`'s `RemoteDesignSystemConsumption`).

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

`MultiTenantTheming` requires `HybridDesignTokens` because it generalizes the single fixed brand palette that feature establishes (`styles/theme.scss` stays the no-`data-tenant` default; a tenant overrides colour only).

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| DesignSystemWorkspace | The design system as an independently versioned npm package: a plain Angular CLI multi-project workspace (library + demo app — not Nx, whose affected-builds / boundaries / federation generators don't apply at two-project scale), library built with ng-packagr for Angular Package Format + Ivy partial compilation, releases via Changesets (every PR touching the public surface ships a changeset, CI-enforced), component preview in a self-built `projects/demo` app (not Storybook). | true |
| HybridDesignTokens | Consume Angular Material's own M3 `--mat-sys-*` tokens directly wherever Material already models the concept (color, typography, elevation) — no redundant alias layer. Define `--ds-*` tokens only for genuine gaps: domain-specific semantic colors (priority, workflow status), spacing scale, radius scale. Every color token uses `light-dark()`. Token values changed only through Material's Sass override mixins, never by hand-setting `--mat-*` in raw CSS. A single fixed brand palette, applied at the root selector. | true |
| EncapsulatedComponentLayer | Every component authored signal-based (`input()`/`output()`/`model()` exclusively — no decorators, no `EventEmitter`), with its own `ds-*` selector and an independently designed API — never a 1:1 passthrough of an underlying Material component's inputs. No Angular Material type, selector, or enum ever appears in the library's public surface. Internally a component may delegate to Material or be fully custom, decided per component (default: delegate). Form-participating components implement `ControlValueAccessor`. | true |
| ComponentTesting | The same three-layer discipline as `monolith/`'s `ComponentTesting`, applied to `ds-*` components against `projects/demo`: behavioral (Testing Library), visual regression (Playwright screenshots, light + dark), accessibility (`@axe-core/playwright`), each visual spec paired with a computed-style snapshot. Reuses `solution-app-testing`'s Vitest/Playwright tool ADRs without re-arguing them. No Storybook, no Chromatic. | true |
| MultiTenantTheming | On top of the single fixed brand palette: a `styles/tenants/` layer — a shared `ds-tenant-theme` colour-only mixin, one `:root[data-tenant='<id>']` palette file per tenant, and a `DsTenant` union exported from `public-api`. The consumer selects a tenant with `document.documentElement.dataset.tenant`; `styles/theme.scss` is unchanged and is the no-attribute default. Typography, density, and spacing never vary by tenant. Requires `HybridDesignTokens`. | false |

### Deliberately not rows

- **`projects/demo`** is baseline structure, not a feature — the library cannot be visually reviewed or visual-tested without it.
- **The `ds-button` worked example** in `solution-design-system-components` is illustration, not a feature.

## Aspirational candidates (owner asked to consider)

None. `MultiTenantTheming` was the one aspirational candidate last cycle; it is now a Features-table row above (the catalog's VP1), realized by [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]]. The `monolith/` aspirational candidates (SSR, i18n, telemetry, feature-flags, runtime-config) do **not** apply to a component library.

## Open questions on V1

**All resolved.**

1. **Is `HybridDesignTokens` common, or is "single fixed brand palette" a variant of a `Theming` VP?** **Resolved:** `HybridDesignTokens` stays common (the single-tenant default); `MultiTenantTheming` is a separate optional VP layered on top of it, not a variant that replaces it.
2. **`solution-ui-testing` is shared with `monolith/` and its `depends_on` conflates both sides.** **Resolved:** split during delta-conflict-detection into a monolith-side `solution-ui-testing` and a design-system-side [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] (deps: `solution-ui-testing` + `design-system-structure` + `design-system-components` + `app-testing`).
3. **`solution-design-system-components` `depends_on solution-design-system-tokens`** is a real edge (a component consumes `--mat-sys-*`/`--ds-*`), but both are common and always co-present — a build order, not a VP. Recorded for completeness, no question.

## Out of scope

- **The plateau↔VP view** — the matrix and V1 reference mapping — lives in [[skills/angular/architecture/v3.1/design-system/plateau/plateau-repository.md|plateau/plateau-repository.md]], not here.
- **Consumption is not modeled here** — how `monolith/` / `platform-host/` / `embeddable-app/` pull in the package belongs to those catalogs.
- **`IsCommon` is a judgment call** — the four V1 features are common because the written baseline needs all four to produce a usable, releasable, tested library; `MultiTenantTheming` is the one genuine optional axis (a repo either needs per-tenant palettes or it does not).
