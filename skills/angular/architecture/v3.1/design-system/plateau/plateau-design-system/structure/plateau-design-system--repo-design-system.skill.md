---
name: plateau-design-system--repo-design-system
description: Plain Angular CLI multi-project workspace for the design system — a publishable ng-packagr component library (M3 theme, --ds-* tokens, signal-based ds-* components), an unpublished demo preview app that doubles as the visual/a11y target, and Changesets releases. Not Nx. — design-system plateau
domain: skill
type: template
whenToUse: when scaffolding the design-system repository, adding a project, wiring ng-packagr / Changesets / the demo app, or checking a workspace-level rule (Nx-vs-CLI, publishable artifact, no Storybook/Chromatic)
plateau: design-system
version: 20260903170000
tags:
  - skill/template/repo
  - plateau/design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]"
---

> **The single plateau of the `design-system` catalog** ([variability map](skills/angular/architecture/v3.1/design-system/variability-map.md) — no VPs; VP1 `MultiTenantTheming` is aspirational). This is a **separate repository** from the Nx platform monorepo — deliberately a plain **Angular CLI multi-project workspace**, published and consumed as an independently versioned npm package. `monolith`, `platform-host` and `embeddable-app` all consume the package produced here.

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
    /[design-system](./design-system/plateau-design-system--project-design-system.skill.md)   <- publishable library (ng-packagr, Angular Package Format)
    /[demo](./demo/plateau-design-system--project-demo.skill.md)                              <- Angular app, NOT published; the component preview + visual/a11y target
  /dist/design-system                   <- ng-packagr output (module/typings/exports + styles assets)
```

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /projects/design-system | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/plateau-design-system--project-design-system.skill\|project-design-system]] | The publishable library — `styles/theme.scss`, `styles/custom-tokens.scss`, `src/lib/{component}/` `ds-*` components, `src/public-api.ts`. Built with ng-packagr → Angular Package Format (Ivy partial compilation). The only project published to npm. |
| /projects/demo | [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/demo/plateau-design-system--project-demo.skill\|project-demo]] | Plain Angular application, never published. Consumes `design-system` (the built package) for `theme.scss`/`custom-tokens.scss` at the root, and mounts one route per component preview — the answer to component review (Storybook was rejected) and the navigation target for every `.visual` / `.style-snapshot` / `.a11y` spec. |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

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

## SHOULD
- Every new component gets a `projects/demo` preview page (its `spec/preview/` file, imported by the demo), so review and the visual/a11y specs both have a stable target.
- Before building a component's internals, evaluate whether Angular Material's own equivalent satisfies the real functional/performance/accessibility need — delegate internally if so; build fully custom only for an identified gap.
- Never migrate a working component to a fully custom internal build "for consistency" — the delegate/custom choice is per-component and requirement-driven.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Check list

- [ ] The workspace is Angular CLI (`angular.json`), not Nx (`nx.json`)
- [ ] `ng build design-system` produces Angular Package Format output in `dist/design-system`
- [ ] `grep -r "@angular/material" dist/design-system/types/` returns nothing
- [ ] A library-touching PR without a changeset fails CI
- [ ] `.changeset/config.json` lists `demo` under `ignore`
- [ ] No `--ds-*` token duplicates a `--mat-sys-*` concept; every colour token uses `light-dark()`
- [ ] Every `ds-*` component has all four spec layers under `src/lib/{component}/spec/`
- [ ] No Storybook / Chromatic in `package.json`

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

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill.md|solution-design-system-tokens]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]
