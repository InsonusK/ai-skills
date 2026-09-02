# Feature Model — design-system

An independently versioned Angular component library, published as an npm package and consumed as a plain npm dependency by [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md|`monolith/`]] and as a version-negotiated federation singleton by [[skills/angular/architecture/v3.1/platform-host/feature/feature-model.md|`platform-host/`]] and [[skills/angular/architecture/v3.1/embeddable-app/feature/feature-model.md|`embeddable-app/`]]. It lives in its own repository with its own release cadence. Derived from the V1 `plateau-design-system` (4 solutions).

The **root product is `DesignSystem`** — you build one per design-system repository. Every V1 solution in this family sits in the single `plateau-design-system`, so the family currently has **no Variation Points** — every path through it is identical. The model still exists so a future optional capability has somewhere to attach and so the two consuming catalogs have a defined thing to point at.

Built per [[skills/dotnet/architecture/v3.1/design/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]]. See [[skills/angular/architecture/v3.1/README.md|the catalog overview]].

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

`MultiTenantTheming` is aspirational (no `Realized by`); it requires `HybridDesignTokens` because it would generalize the single fixed brand palette that feature establishes.

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| DesignSystemWorkspace | The design system as an independently versioned npm package: a plain Angular CLI multi-project workspace (library + demo app — not Nx, whose affected-builds / boundaries / federation generators don't apply at two-project scale), library built with ng-packagr for Angular Package Format + Ivy partial compilation, releases via Changesets (every PR touching the public surface ships a changeset, CI-enforced), component preview in a self-built `projects/demo` app (not Storybook). | true |
| HybridDesignTokens | Consume Angular Material's own M3 `--mat-sys-*` tokens directly wherever Material already models the concept (color, typography, elevation) — no redundant alias layer. Define `--ds-*` tokens only for genuine gaps: domain-specific semantic colors (priority, workflow status), spacing scale, radius scale. Every color token uses `light-dark()`. Token values changed only through Material's Sass override mixins, never by hand-setting `--mat-*` in raw CSS. A single fixed brand palette, applied at the root selector. | true |
| EncapsulatedComponentLayer | Every component authored signal-based (`input()`/`output()`/`model()` exclusively — no decorators, no `EventEmitter`), with its own `ds-*` selector and an independently designed API — never a 1:1 passthrough of an underlying Material component's inputs. No Angular Material type, selector, or enum ever appears in the library's public surface. Internally a component may delegate to Material or be fully custom, decided per component (default: delegate). Form-participating components implement `ControlValueAccessor`. | true |
| ComponentTesting | The same three-layer discipline as `monolith/`'s `ComponentTesting`, applied to `ds-*` components against `projects/demo`: behavioral (Testing Library), visual regression (Playwright screenshots, light + dark), accessibility (`@axe-core/playwright`), each visual spec paired with a computed-style snapshot. Reuses `solution-app-testing`'s Vitest/Playwright tool ADRs without re-arguing them. No Storybook, no Chromatic. | true |

### Deliberately not rows

- **`projects/demo`** is baseline structure, not a feature — the library cannot be visually reviewed or visual-tested without it.
- **The `ds-button` worked example** in `solution-design-system-components` is illustration, not a feature.

## Aspirational candidates (owner asked to consider)

| Candidate | Rationale | Shape |
| --- | --- | --- |
| MultiTenantTheming | `solution-design-system-tokens` **explicitly defers** this: "multi-brand/per-tenant theming is explicitly deferred to a future solution, informed by real requirements if and when they appear." The single fixed brand palette is a deliberate current-scope choice, so multi-tenant is a clean future VP: swappable palettes / per-tenant theme resolution on top of the existing token layer. | optional, library-level |

The `monolith/` aspirational candidates (SSR, i18n, telemetry, feature-flags, runtime-config) do **not** apply to a component library.

## Open questions on V1

1. **Is `HybridDesignTokens` common, or is "single fixed brand palette" a variant of a `Theming` VP?** Today there is exactly one palette and one plateau, so it reads as common. If `MultiTenantTheming` is built, `HybridDesignTokens` becomes "the single-tenant variant" of a `Theming` VP. **Working hypothesis: common now**, revisit if multi-tenant is built.
2. **`solution-ui-testing` is shared with `monolith/` and its `depends_on` conflates both sides.** For this family, `ComponentTesting`'s real deps are `design-system-structure` + `design-system-components` + `app-testing` (for the tool ADRs). **Working hypothesis: the shared `solution-ui-testing` splits during delta-conflict-detection** into a monolith-side and a design-system-side realization.
3. **`solution-design-system-components` `depends_on solution-design-system-tokens`** is a real edge (a component consumes `--mat-sys-*`/`--ds-*`), but both are common and always co-present — a build order, not a VP. Recorded for completeness, no question.

## Out of scope

- **This family has no Variation Points today.** The [[skills/angular/architecture/v3.1/design-system/variability-map.md|design-system Variability Map]] will say so and list `MultiTenantTheming` as the single aspirational row.
- **Consumption is not modeled here** — how `monolith/` / `platform-host/` / `embeddable-app/` pull in the package belongs to those catalogs.
- **`IsCommon` is a judgment call** — all four features are common because the single V1 plateau composes all four and the written baseline needs all four to produce a usable, releasable, tested library.
