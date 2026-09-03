---
name: registry-design-system-repository
description: Conflict Detection result for the `design-system-repository` element — the Angular CLI design-system workspace, created by one solution and extended by three, all member-disjoint
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

All four coexist at this plateau (the catalog's only plateau — every design-system repo composes exactly these four common solutions).

# Classification
`FMN` / `TMN` — a repository-level bucket. Category `M` (workspace-config / convention change). Kind `N` (independent): `structure` creates the CLI workspace; `tokens` adds two SCSS entry points; `components` adds the `ds-*` authoring convention; `ui-testing` adds the `projects/demo` visual/a11y targets and the `spec/` layout. **Member-disjoint** — no two edit the same file or rule. `solution-design-system-components` declares `depends_on solution-design-system-tokens` (a real build-order edge — a component consumes the tokens), and `solution-design-system-ui-testing` declares `depends_on` on `structure` + `components` + `solution-ui-testing` + `app-testing`; every other pair is `F` (no Feature-Model constraint — all four are common baseline features).

# Ordering
`source: ordering-only` — the create-then-extend order is recorded by each extending solution's `depends_on`. Nothing about the resulting workspace depends on the relative order of the three extends (a SCSS entry point, an authoring convention, a test-layer convention are order-independent).

# Resolution
**Canonical — resolved by design, no resolver.** This is the intended shape: a small repo where every common capability adds one distinct piece. The [`plateau-design-system` example](skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/example/) builds it end to end — `ng build design-system` (Angular Package Format), `ng test design-system` (7 behavioural tests), `ng build demo`, and the Playwright specs all green.

# Architectural signal
N = 4. **Benign.** A two-project design-system repo touched by all four of its common solutions is the correct design, not a mis-drawn variation point — the direct analogue of the monolith's [`monolith-repository`](skills/angular/architecture/v3.1/monolith/plateau/plateau-online-monolith/registry/monolith-repository.md) benign bucket. Recorded per the delta-conflict-detection N≥3 rule to make the review explicit.
