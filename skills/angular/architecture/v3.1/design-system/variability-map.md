---
tags:
  - concern/architecture
  - stack/typescript
---

# design-system Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], from the non-common features of [[skills/angular/architecture/v3.1/design-system/feature/feature-model.md|design-system/feature/feature-model.md]]. Sibling catalogs: [[skills/angular/architecture/v3.1/monolith/variability-map.md|monolith]], [[skills/angular/architecture/v3.1/platform-host/variability-map.md|platform-host]], [[skills/angular/architecture/v3.1/embeddable-app/variability-map.md|embeddable-app]].

## Variation Points

**This catalog has no Variation Points today.** All four features of the [[skills/angular/architecture/v3.1/design-system/feature/feature-model.md|feature model]] (`DesignSystemWorkspace`, `HybridDesignTokens`, `EncapsulatedComponentLayer`, `ComponentTesting`) are common — every design-system repository composes all four, and the written baseline needs all four to produce a usable, releasable, tested library. The V1 `plateau-design-system` composes exactly these four solutions and there is no second plateau to vary against.

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **MultiTenantTheming** *(aspirational — no solution yet)* — swappable palettes / per-tenant theme resolution on top of the single fixed brand palette? | Yes / No | **requires `HybridDesignTokens`** (it generalizes that feature's single palette) | *(none)* — `solution-design-system-tokens` **explicitly defers** this ("multi-brand/per-tenant theming is explicitly deferred to a future solution, informed by real requirements if and when they appear"); a new `solution-design-system-multi-tenant-theming` would realize it | Cross-feature: if built, `HybridDesignTokens` becomes "the single-tenant variant" of a `Theming` VP | No |

## Features that are not VPs

- **`DesignSystemWorkspace`, `HybridDesignTokens`, `EncapsulatedComponentLayer`, `ComponentTesting`** — all common. `solution-design-system-components` `depends_on solution-design-system-tokens` is a real edge but both are always co-present — a build order, not a VP.
- **`solution-ui-testing` split — DONE (Stage 3d).** `solution-ui-testing` (monolith) + `solution-design-system-ui-testing` (this catalog; `depends_on` `solution-ui-testing` + `design-system-structure` + `design-system-components` + `app-testing`). The three ADRs and four spec patterns stay in `solution-ui-testing` and are reused verbatim.

## Plateau Map derivation

**[`plateau-design-system`](skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md) is built** — the catalog's single plateau, composing all four common solutions (`structure` + `tokens` + `components` + `ui-testing`), `standalone: true`, `parent_plateaus: []`. Runnable Angular CLI example (ng-packagr + Vitest green). One benign intersection group: [`design-system-repository`](skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/registry/design-system-repository.md) (N = 4).

### Reference: V1 → v3.1

| V1 plateau | v3.1 design-system plateau | VP answers |
| --- | --- | --- |
| `plateau-design-system` | `plateau-design-system` (built) | (no VPs) — composes all four common solutions |

### Combinations v3.1 allows that V1 has no plateau for

- **VP1=Yes** — a multi-tenant-themed design system (aspirational). Would be the catalog's first non-trivial plateau.

## Out of scope

- **This catalog is single-plateau today.** It exists so `MultiTenantTheming` has somewhere to attach and so the three consuming catalogs (`monolith/`, `platform-host/`, `embeddable-app/`) have a defined package to point at.
- **Consumption** (npm dependency vs. federation singleton, version negotiation, theme scoping) is modeled in the consuming catalogs, not here.
- **`Migration = No`** — the design system's own structure does not change post-release in any V1-observed way.
