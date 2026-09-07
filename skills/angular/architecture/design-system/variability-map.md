---
tags:
  - concern/architecture
  - stack/typescript
---

# design-system Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], from the non-common features of [[skills/angular/architecture/design-system/feature/feature-model.md|design-system/feature/feature-model.md]]. Sibling catalogs: [[skills/angular/architecture/monolith/variability-map.md|monolith]], [[skills/angular/architecture/platform-host/variability-map.md|platform-host]], [[skills/angular/architecture/embeddable-app/variability-map.md|embeddable-app]].

## Variation Points

**This catalog has one Variation Point: `MultiTenantTheming`.** The four features of the [[skills/angular/architecture/design-system/feature/feature-model.md|feature model]] (`DesignSystemWorkspace`, `HybridDesignTokens`, `EncapsulatedComponentLayer`, `ComponentTesting`) are common — every design-system repository composes all four. `MultiTenantTheming` is the one thing that varies: a repo either ships the single fixed brand palette or generalizes it into swappable per-tenant palettes.

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **MultiTenantTheming** — swappable per-tenant palettes / per-tenant theme resolution on top of the single fixed brand palette? | Yes / No | **requires `HybridDesignTokens`** (it generalizes that feature's single palette) | Yes → [[skills/angular/architecture/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md\|solution-design-system-multi-tenant-theming]] (full — a `styles/tenants/` layer: the `ds-tenant-theme` colour-only mixin, one `:root[data-tenant='<id>']` file per tenant, a `DsTenant` union, CSS-attribute resolution) | Cross-feature: `HybridDesignTokens` becomes "the single-tenant default" — `styles/theme.scss` is unchanged and is the no-`data-tenant` fallback; a tenant overrides colour only | No |

## Features that are not VPs

- **`DesignSystemWorkspace`, `HybridDesignTokens`, `EncapsulatedComponentLayer`, `ComponentTesting`** — all common. `solution-design-system-components` `depends_on solution-design-system-tokens` is a real edge but both are always co-present — a build order, not a VP.
- **`solution-ui-testing` is split** — a monolith-side [[skills/angular/architecture/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] + this catalog's [[skills/angular/architecture/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] (`depends_on` `solution-ui-testing` + `design-system-structure` + `design-system-components` + `app-testing`). The three ADRs and four spec patterns stay in `solution-ui-testing` and are reused verbatim.

## Out of scope

- **The plateau↔VP view** — the matrix, lineage, and V1 reference mapping — lives in [[skills/angular/architecture/design-system/plateau/plateau-repository.md|plateau/plateau-repository.md]], not here ([[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/no-plateau-view-in-variability-map.md|ADR]]).
- **Consumption** (npm dependency vs. federation singleton, version negotiation, theme scoping) is modeled in the consuming catalogs, not here.
- **`Migration = No`** — the design system's own structure does not change post-release in any V1-observed way.
