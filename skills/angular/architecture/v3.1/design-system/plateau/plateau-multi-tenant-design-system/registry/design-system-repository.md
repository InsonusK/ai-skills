---
name: registry-design-system-repository
description: Conflict Detection result for the `design-system-repository` element at plateau-multi-tenant-design-system — the Angular CLI design-system workspace, created by one solution and extended by four (VP1 adds the styles/tenants/ layer), all member-disjoint
tags:
  - concern/architecture
  - stack/typescript
  - element/design-system-repository
---

# Element
`element/design-system-repository` — the plain Angular CLI multi-project workspace: `angular.json`, the `projects/design-system` + `projects/demo` layout, `ng-package.json`, the Changesets config, `playwright.config.ts`, the TS path aliases. Everything above an individual project.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] (`.create` — `Repository.create` — the CLI workspace, the library + demo projects, ng-packagr, Changesets)
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] (`.extend` — `Repository.extend` — adds `styles/theme.scss` + `styles/custom-tokens.scss` and the token-consumption rules)
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] (`.extend` — `Repository.extend` — adds the `ds-*` selector convention, signal-API authoring, `ControlValueAccessor` for form controls, the per-component delegate-vs-custom rule)
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] (`.extend` — `demo.project.extend` — makes `projects/demo` the visual/a11y target and adds the four-spec-layer convention under `src/lib/{component}/spec/`)
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] (VP1, `.extend` — `Repository.extend` — adds the `styles/tenants/` layer: the `ds-tenant-theme` mixin, one `_<id>.scss` per tenant, the `tenants.scss` aggregator asset, the `[data-tenant]` selector convention; **+** `design-system.project.extend` for the `ng-package.json` asset entry and the `DsTenant` export)

All five coexist at this plateau (VP1 = Yes — the catalog's `MultiTenantTheming` plateau, composing the four common solutions plus this one).

# Classification
`FMN` / `TMN` — a repository-level bucket. Constraint on the VP1 pair: `T` — `solution-design-system-multi-tenant-theming` `depends_on solution-design-system-tokens` (it generalizes that feature's palette; a real Feature-Model constraint). Category `M` (workspace-config / convention change). Kind `N` (independent): `structure` creates the CLI workspace; `tokens` adds two SCSS entry points; `components` adds the `ds-*` authoring convention; `ui-testing` adds the `projects/demo` visual/a11y targets and the `spec/` layout; `multi-tenant-theming` adds a **new** `styles/tenants/` folder + `src/lib/tenants.ts` + a `ng-package.json` asset line. **Member-disjoint** — VP1 touches only files the other four do not (`styles/theme.scss` is left unchanged; the tenant layer is more-specific overrides in new files). Every non-VP1 pair is `F` (all four are common baseline features).

# Ordering
`source: ordering-only` — the create-then-extend order is recorded by each extending solution's `depends_on`. The VP1 extend must land after `tokens` (it builds on `theme.scss` being the default and `custom-tokens.scss` defining the base `--ds-*` values); the `depends_on` edge records it. Order among the other three extends is still irrelevant.

# Resolution
**Canonical — resolved by design, no resolver.** The [`plateau-multi-tenant-design-system` example](skills/angular/architecture/v3.1/design-system/plateau/plateau-multi-tenant-design-system/plateau-multi-tenant-design-system.skill/example/) builds it end to end — `ng build design-system` (Angular Package Format, `styles/tenants/**` shipped as assets, `DsTenant` in `types/*.d.ts`, no `@angular/material` leak), `ng test design-system` (9 behavioural tests, incl. the `DS_TENANTS` shape), `ng build demo` (the compiled root CSS carries `:root[data-tenant='acme']` / `[data-tenant='globex']`), `tsc -p tsconfig.e2e.json`, and the per-tenant Playwright specs all green.

# Architectural signal
N = 4 at `plateau-design-system`; **N = 5 here** (structure + tokens + components + ui-testing + multi-tenant-theming). **Benign.** A two-project design-system repo touched by all its common solutions plus its one VP is the correct design, not a mis-drawn conflict — the direct analogue of the monolith's [`monolith-repository`](skills/angular/architecture/v3.1/monolith/plateau/plateau-online-monolith/registry/monolith-repository.md) / [`shared-state-project`](skills/angular/architecture/v3.1/monolith/plateau/plateau-persisted-state-monolith/registry/shared-state-project.md) benign buckets. VP1's contribution is entirely in new files under a new `styles/tenants/` directory; it cannot collide with the base token/component/test conventions. Recorded per the delta-conflict-detection N≥3 rule to make the review explicit.
