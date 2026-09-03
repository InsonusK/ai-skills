---
tags:
  - concern/architecture
  - stack/typescript
---

# design-system Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], from the non-common features of [[skills/angular/architecture/v3.1/design-system/feature/feature-model.md|design-system/feature/feature-model.md]]. Sibling catalogs: [[skills/angular/architecture/v3.1/monolith/variability-map.md|monolith]], [[skills/angular/architecture/v3.1/platform-host/variability-map.md|platform-host]], [[skills/angular/architecture/v3.1/embeddable-app/variability-map.md|embeddable-app]].

## Variation Points

**This catalog has one Variation Point: `MultiTenantTheming`.** The four features of the [[skills/angular/architecture/v3.1/design-system/feature/feature-model.md|feature model]] (`DesignSystemWorkspace`, `HybridDesignTokens`, `EncapsulatedComponentLayer`, `ComponentTesting`) are common — every design-system repository composes all four. `MultiTenantTheming` is the one thing that varies: a repo either ships the single fixed brand palette (`plateau-design-system`) or generalizes it into swappable per-tenant palettes (`plateau-multi-tenant-design-system`).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **MultiTenantTheming** — swappable per-tenant palettes / per-tenant theme resolution on top of the single fixed brand palette? | Yes / No | **requires `HybridDesignTokens`** (it generalizes that feature's single palette) | Yes → [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md\|solution-design-system-multi-tenant-theming]] (full — a `styles/tenants/` layer: the `ds-tenant-theme` colour-only mixin, one `:root[data-tenant='<id>']` file per tenant, a `DsTenant` union, CSS-attribute resolution) | Cross-feature: `HybridDesignTokens` becomes "the single-tenant default" — `styles/theme.scss` is unchanged and is the no-`data-tenant` fallback; a tenant overrides colour only | No |

## Features that are not VPs

- **`DesignSystemWorkspace`, `HybridDesignTokens`, `EncapsulatedComponentLayer`, `ComponentTesting`** — all common. `solution-design-system-components` `depends_on solution-design-system-tokens` is a real edge but both are always co-present — a build order, not a VP.
- **`solution-ui-testing` split — DONE (Stage 3d).** `solution-ui-testing` (monolith) + `solution-design-system-ui-testing` (this catalog; `depends_on` `solution-ui-testing` + `design-system-structure` + `design-system-components` + `app-testing`). The three ADRs and four spec patterns stay in `solution-ui-testing` and are reused verbatim.

## Plateau Map derivation

**Two plateaus are built:**

| Plateau | Composes | Adds | VP1 |
| --- | --- | --- | --- |
| [`plateau-design-system`](skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md) | — (from scratch) | `structure` + `tokens` + `components` + `ui-testing` | No |
| [`plateau-multi-tenant-design-system`](skills/angular/architecture/v3.1/design-system/plateau/plateau-multi-tenant-design-system/plateau-multi-tenant-design-system.skill/plateau-multi-tenant-design-system.skill.md) | plateau-design-system | `solution-design-system-multi-tenant-theming` | Yes |

Both `standalone: true`. Runnable Angular CLI examples (ng-packagr + Vitest green). One benign intersection group, tracked at both plateaus: [`design-system-repository`](skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/registry/design-system-repository.md) (N = 4 at `plateau-design-system`, [N = 5](skills/angular/architecture/v3.1/design-system/plateau/plateau-multi-tenant-design-system/registry/design-system-repository.md) at `plateau-multi-tenant-design-system`).

### Reference: V1 → v3.1

| V1 plateau | v3.1 design-system plateau | VP answers |
| --- | --- | --- |
| `plateau-design-system` | `plateau-design-system` (built) | VP1=No — composes all four common solutions |
| *(none)* | `plateau-multi-tenant-design-system` (built) | VP1=Yes — + `solution-design-system-multi-tenant-theming` |

### Combinations v3.1 allows that V1 has no plateau for

- **VP1=Yes** — a multi-tenant-themed design system. `plateau-multi-tenant-design-system` is it — the catalog's first non-trivial plateau, built on `plateau-design-system` via `parent_plateaus`.

## Out of scope

- **This catalog has two plateaus** (`plateau-design-system` and its VP1 child `plateau-multi-tenant-design-system`) and so the three consuming catalogs (`monolith/`, `platform-host/`, `embeddable-app/`) have a defined package to point at.
- **Consumption** (npm dependency vs. federation singleton, version negotiation, theme scoping) is modeled in the consuming catalogs, not here.
- **`Migration = No`** — the design system's own structure does not change post-release in any V1-observed way.
