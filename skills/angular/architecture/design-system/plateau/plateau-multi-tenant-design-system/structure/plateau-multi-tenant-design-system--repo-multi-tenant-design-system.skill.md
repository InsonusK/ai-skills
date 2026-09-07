---
name: plateau-multi-tenant-design-system--repo-multi-tenant-design-system
description: Plain Angular CLI multi-project workspace for the design system — a publishable ng-packagr component library (M3 theme, --ds-* tokens, signal-based ds-* components, plus a styles/tenants/ layer of swappable per-tenant palettes), an unpublished demo preview app that doubles as the visual/a11y target, and Changesets releases. Not Nx. — multi-tenant-design-system plateau
domain: skill
type: template
whenToUse: when scaffolding the design-system repository, adding a project or a tenant palette, wiring ng-packagr / Changesets / the demo app / the styles/tenants/ layer, or checking a workspace-level rule (Nx-vs-CLI, publishable artifact, no Storybook/Chromatic, colour-only tenants)
plateau: multi-tenant-design-system
version: 20260903200000
tags:
  - skill/template/repo
  - plateau/multi-tenant-design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]]"
---

> **The `design-system` catalog's `MultiTenantTheming` plateau (VP1 = Yes)** ([variability map](skills/angular/architecture/v3.1/design-system/variability-map.md)). Composes [`plateau-design-system`](skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md) (all four common solutions) and adds [`solution-design-system-multi-tenant-theming`](skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md) — a `styles/tenants/` layer that generalizes the single fixed brand palette into swappable per-tenant palettes, resolved by a CSS `[data-tenant]` attribute the consuming app sets. This is a **separate repository** from the Nx platform monorepo — a plain **Angular CLI multi-project workspace**, published and consumed as an independently versioned npm package. `monolith`, `platform-host` and `embeddable-app` all consume the package produced here.

# Structure

## Workspace Structure

```
/design-system-repo
  angular.json
  package.json                          <- root: build/test/e2e/changeset scripts
  playwright.config.ts                  <- visual + style-snapshot + a11y specs, target = projects/demo
  tsconfig.json                         <- paths: "design-system" -> dist; "@ds-preview/*" -> lib source
  /.changeset                           <- Changesets config; the demo project is ignore-d
  /projects
    /[design-system](./design-system/plateau-multi-tenant-design-system--project-design-system.skill.md)   <- publishable library (ng-packagr, Angular Package Format)
      /styles/theme.scss                 <- base brand (default when no data-tenant)
      /styles/custom-tokens.scss
      /styles/tenants.scss               <- NEW (VP1): aggregator asset; @use'd by a consumer after `theme`
      /styles/tenants/_tenant-theme.scss <- NEW: the ds-tenant-theme mixin (colour only)
      /styles/tenants/_{tenant}.scss     <- NEW: one :root[data-tenant='<id>'] block per tenant
      /src/lib/tenants.ts                <- NEW: DS_TENANTS tuple + DsTenant union
    /[demo](./demo/plateau-multi-tenant-design-system--project-demo.skill.md)                              <- Angular app, NOT published; the component preview + visual/a11y target; a tenant switcher sets document.documentElement.dataset.tenant
  /dist/design-system                   <- ng-packagr output (module/typings/exports + styles assets, incl. styles/tenants/**)
```

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /projects/design-system | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-multi-tenant-design-system/structure/design-system/plateau-multi-tenant-design-system--project-design-system.skill\|project-design-system]] | The publishable library — `styles/theme.scss`, `styles/custom-tokens.scss`, `src/lib/{component}/` `ds-*` components, `src/public-api.ts`. Built with ng-packagr → Angular Package Format (Ivy partial compilation). The only project published to npm. |
| /projects/demo | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-multi-tenant-design-system/structure/demo/plateau-multi-tenant-design-system--project-demo.skill\|project-demo]] | Plain Angular application, never published. Consumes `design-system` (the built package) for `theme.scss`/`custom-tokens.scss`/`tenants.scss` at the root, mounts one route per component preview, and carries a tenant switcher that sets `document.documentElement.dataset.tenant` — the navigation target for every `.visual` / `.style-snapshot` / `.a11y` spec, including the per-tenant snapshot specs. |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Repository.extend.md|Repository.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/material, @angular/cdk | latest supporting M3 | `mat.theme()`, M3 system tokens, Sass override mixins, `matButton` (etc.) — used internally by delegating components, **never** in the public API. `peerDependencies` of the library. |
| ng-packagr | 22.x | Library build → Angular Package Format / Ivy partial compilation |
| @changesets/cli | latest | Per-PR version-bump classification + CHANGELOG |
| @testing-library/angular, @testing-library/user-event, @testing-library/jest-dom | latest compatible | Behavioural component specs (`ng test` via `@angular/build:unit-test`, Vitest + jsdom) |
| @playwright/test, @axe-core/playwright | latest compatible | Visual regression + style-snapshot + accessibility specs against `projects/demo` |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Rules

## MUST
- The repository is a plain Angular CLI multi-project workspace — never an Nx workspace. Nx's affected-builds / boundary-enforcement / federation generators do not pay for their complexity in a two-project repo.
- The library is built with ng-packagr — no custom Vite/Rollup build. Only ng-packagr guarantees Angular Package Format + Ivy partial compilation for a range of consumer Angular versions.
- Every PR that changes the published library's public API or behaviour includes a changeset file; CI fails a library-touching PR with none.
- `projects/demo` is never published to npm — only `design-system` is a publishable artifact. `.changeset/config.json` lists `demo` under `ignore`.
- Every custom component consumes `--mat-sys-*` tokens directly for colour/typography/elevation — never a `--ds-color-primary`-style alias for a concept Material already models.
- `--ds-*` tokens exist only for concepts with no Material equivalent (domain-specific semantic colour, spacing, radius). Every colour token — `--mat-sys-*` and `--ds-*` — uses `light-dark()`.
- Token values are overridden only via Angular Material's Sass override mixins — a component never hand-sets a `--mat-*` custom property in raw CSS.
- Every component uses the `ds-` selector prefix and a signal-based API (`input()`/`output()`/`model()`) — no `@Input()`/`@Output()` decorators, no `EventEmitter`. Its API is designed around real usage axes, never a 1:1 mirror of Material's own input names.
- **No Angular Material selector, input, or type appears in the library's public API surface** — check the *built* `dist/design-system/types/*.d.ts`, not just the `.ts` source: ng-packagr emits `protected` members too, so an internal helper typed with a Material-exported type leaks it. Use local literal types for internal Material mapping.
- Any component participating in a form implements `ControlValueAccessor`.
- Every component ships all four test layers — behavioural (Testing Library), visual (Playwright screenshot), style-snapshot (computed CSS), accessibility (`@axe-core/playwright`) — per [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]. A behavioural spec provides nothing beyond the component's own inputs — a pure `ds-*` component injects no dependency to fake.
- Never reach for Storybook or Chromatic — component preview is `projects/demo`; visual regression is a Playwright screenshot against it.
- **VP1** — a tenant is a colour palette scoped to `:root[data-tenant='<id>']`, emitted through the shared `ds-tenant-theme` mixin (colour only — no typography / density keys). `styles/theme.scss` is unchanged and is the no-attribute default; typography, density and Material base styles are emitted once, there.
- **VP1** — the active tenant is selected by the consuming app setting `document.documentElement.dataset.tenant` to a `DsTenant` value; the library ships no runtime `applyTenant()` and no per-tenant CSS bundle. Every tenant colour uses `light-dark()`.
- **VP1** — a new tenant is three edits in one change: `styles/tenants/_<id>.scss`, a `@use` line in `styles/tenants.scss`, and an entry in `DS_TENANTS` (`src/lib/tenants.ts`). `tenants.scss` is a `ng-package.json` asset (with `styles/tenants/`), consumed as `@use 'design-system/styles/tenants'` **after** `theme`.
- **VP1** — no `ds-*` component references a tenant name or a tenant-specific value; components consume `--mat-sys-*` / `--ds-*`, only the values behind them vary.

## SHOULD
- Every new component gets a `projects/demo` preview page (its `spec/preview/` file, imported by the demo), so review and the visual/a11y specs both have a stable target.
- Before building a component's internals, evaluate whether Angular Material's own equivalent satisfies the real functional/performance/accessibility need — delegate internally if so; build fully custom only for an identified gap.
- Never migrate a working component to a fully custom internal build "for consistency" — the delegate/custom choice is per-component and requirement-driven.
- Never override `--ds-spacing-*` / `--ds-radius-*` per tenant, or emit a bare `mat.theme()` in a tenant file — a tenant varies colour only; use `ds-tenant-theme`.
- Never ship a per-tenant compiled stylesheet a consumer injects via `<link>`, or a runtime JS token-rewrite — the first flashes, the second is the JS theme toggle the token layer's ADR rejected.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Repository.extend.md|Repository.extend]]

# Check list

- [ ] The workspace is Angular CLI (`angular.json`), not Nx (`nx.json`)
- [ ] `ng build design-system` produces Angular Package Format output in `dist/design-system`
- [ ] `grep -r "@angular/material" dist/design-system/types/` returns nothing
- [ ] A library-touching PR without a changeset fails CI
- [ ] `.changeset/config.json` lists `demo` under `ignore`
- [ ] No `--ds-*` token duplicates a `--mat-sys-*` concept; every colour token uses `light-dark()`
- [ ] Every `ds-*` component has all four spec layers under `src/lib/{component}/spec/`
- [ ] No Storybook / Chromatic in `package.json`
- [ ] `styles/tenants.scss` is a `ng-package.json` asset (with `styles/tenants/`); `dist/design-system/styles/tenants.scss` + `styles/tenants/_*.scss` ship
- [ ] With no `data-tenant`, `--mat-sys-primary` is the base brand; with `data-tenant='<id>'` it is that tenant's; typography / density tokens are identical across tenants
- [ ] Every `DS_TENANTS` entry has a `_<id>.scss` file and a `@use` line in `tenants.scss`; `public-api.ts` exports `DS_TENANTS` + `DsTenant`
- [ ] No component references a tenant name; no runtime JS writes theme tokens

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Unittest TestCases

- [ ] WHEN the library is built THEN the output conforms to the Angular Package Format (Ivy partial compilation)
- [ ] WHEN a PR modifies the library's public API without a changeset THEN CI fails
- [ ] WHEN a custom component needs a Material-modeled concept (colour, typography, elevation) THEN it references the `--mat-sys-*` token directly, with no `--ds-*` alias
- [ ] WHEN the OS switches to dark mode THEN every `--mat-sys-*` and `--ds-*` colour token resolves to its dark variant via `light-dark()`, with no JavaScript
- [ ] WHEN a component's public API (exported class, inputs, outputs) OR the built `types/*.d.ts` is inspected THEN no Angular Material type, selector, or enum appears
- [ ] WHEN any component's source is inspected THEN no `@Input()`/`@Output()` decorator or `EventEmitter` is used
- [ ] WHEN a component ships without one of its four spec layers THEN CI fails
- [ ] WHEN `data-tenant='globex'` is set THEN a component using `--mat-sys-primary` renders the globex value in both light and dark, and `--mat-sys-body-medium` is unchanged
- [ ] WHEN `data-tenant` is removed THEN the same component falls back to the base brand palette

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Repository.extend.md|Repository.extend]]
