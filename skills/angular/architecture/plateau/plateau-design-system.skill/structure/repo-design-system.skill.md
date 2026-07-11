---
name: repo-design-system
description: Angular CLI multi-project workspace for the design system — publishable component library, demo preview app, M3 theme/tokens, ng-packagr build, Changesets releases
domain: skill
type: template
plateau: design-system
version: 20260711120000
tags:
  - skill/template/repo
  - plateau/design-system
created_by:
  - "[[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill|solution-design-system-structure]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill|solution-design-system-components]]"
---

> This is a separate repository from the main platform monorepo (which uses Nx) — the design system is deliberately a plain Angular CLI multi-project workspace, published and consumed as an independently versioned npm package.

# Structure

## Workspace Structure

```
/design-system-repo
  angular.json
  /projects
    /design-system              <- publishable library (ng-packagr)
      /src
        /lib
          /{component-name}     <- one dir per component, see [[design-system/classes/class-component-name.skill.md|class-component-name.skill]]
        /styles
          theme.scss            <- see [[design-system/classes/class-theme.skill.md|class-theme.skill]]
          custom-tokens.scss    <- see [[design-system/classes/class-custom-tokens.skill.md|class-custom-tokens.skill]]
      ng-package.json
      package.json
    /demo                       <- Angular application, component preview (not published)
      /src
  /.changeset
  package.json
```

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /projects/design-system | [[skills/angular/architecture/plateau/plateau-design-system.skill/structure/design-system/project-design-system.skill\|project-design-system]] | Publishable component library — theme, custom tokens, `ds-*` components. Built with ng-packagr, producing Ivy partial compilation (Angular Package Format) output. The only project published to npm. |
| /projects/demo | — | Plain Angular application, not published. Imports `design-system` as a regular dependency and renders example usage of every component — the answer to component preview/visual review (Storybook was deliberately rejected after prior friction). |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill|solution-design-system-structure]] - [[skills/angular/architecture/solutions/solution-design-system-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill|solution-design-system-components]] - [[skills/angular/architecture/solutions/solution-design-system-components.skill/Implementation/Repository.extend|Repository.extend]]

# Rules

## MUST
- The repository MUST be a plain Angular CLI multi-project workspace, not an Nx workspace.
- The library project MUST be built with ng-packagr — no custom Vite/Rollup build.
- Every pull request that changes the published library's public API or behavior MUST include a changeset file.
- The `demo` project MUST NOT be published to npm — only `design-system` is a publishable artifact.
- Every custom component MUST consume `--mat-sys-*` tokens directly for colors/typography/elevation — MUST NOT define a `--ds-*` alias for a concept Material already models.
- `--ds-*` tokens MUST only be introduced for concepts with no Material equivalent (domain-specific semantic colors, spacing, radius).
- Every color token, both `--mat-sys-*` and `--ds-*`, MUST use `light-dark()`.
- Token values MUST only be overridden via Angular Material's Sass override mixins — components MUST NOT hand-set a `--mat-*` custom property directly in raw CSS.
- Every component MUST use the `ds-` selector prefix — never expose or re-export a Material selector directly.
- Every component's public API MUST use `input()`, `output()`, `model()` — no `@Input()`/`@Output()` decorators, no `EventEmitter`.
- Every component's API MUST be designed around this application's real usage axes — MUST NOT mirror Material's own input names or category model 1:1.
- Any component participating in forms MUST implement `ControlValueAccessor`.
- No Angular Material selector, input, or type MUST appear in this library's public API surface.

## SHOULD
- Every new component added to the library SHOULD get a corresponding example page in `demo`, so visual review stays available for every shipped component.
- Before building a component's internal implementation, SHOULD first evaluate whether Angular Material's own equivalent satisfies the real functional/performance/accessibility requirements; delegate to it internally if so, build fully custom only when it does not.

## MUST NOT
- This plateau MUST NOT introduce a palette-swapping or per-tenant theming mechanism — a single fixed brand palette is used everywhere; multi-brand theming is deferred until a real requirement appears.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill|solution-design-system-structure]] - [[skills/angular/architecture/solutions/solution-design-system-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill|solution-design-system-components]] - [[skills/angular/architecture/solutions/solution-design-system-components.skill/Implementation/Repository.extend|Repository.extend]]

# Anti-patterns

- **Publishing changes to the library without a changeset**
  - Consequence: release tooling has nothing to base the next version bump/changelog entry on, reintroducing manual-discipline release risk
  - Instead: every PR touching the library's public surface includes a changeset
- **Reaching for Storybook again "just for this one component"**
  - Consequence: reintroduces the exact friction already identified from prior experience, and fragments preview tooling across two approaches
  - Instead: add the component's preview to the existing `demo` app
- **Defining `--ds-color-primary: var(--mat-sys-primary)` "for consistency"**
  - Consequence: adds an indirection layer with no naming improvement — two names now exist for the same concept
  - Instead: reference `--mat-sys-primary` directly; reserve `--ds-*` for concepts Material doesn't model
- **Hand-setting a `--mat-*` custom property directly in a component's raw CSS for a one-off tweak**
  - Consequence: bypasses the Sass overrides API's name validation and forward-compatibility guarantee
  - Instead: use the appropriate override mixin (`mat.theme-overrides` or the component-specific one)
- **Exposing a Material input type or enum directly through a component's public API**
  - Consequence: leaks Material's own API surface to consumers, couples them to Material's own versioning
  - Instead: define the design system's own type/enum, mapped internally to whatever Material (or custom implementation) needs
- **Building a fully custom internal implementation by default, without first checking whether Material's own component already satisfies the requirement**
  - Consequence: unnecessary duplicated effort and maintenance burden
  - Instead: default to delegating to Material internally; go custom only when a real, identified gap justifies it

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill|solution-design-system-structure]] - [[skills/angular/architecture/solutions/solution-design-system-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill|solution-design-system-components]] - [[skills/angular/architecture/solutions/solution-design-system-components.skill/Implementation/Repository.extend|Repository.extend]]

# Unittest TestCases

- [ ] WHEN the library is built THEN
  - [ ] the output conforms to the Angular Package Format (Ivy partial compilation)
- [ ] WHEN a PR modifies the library's public API THEN
  - [ ] CI fails if no changeset file is present
- [ ] WHEN a custom component needs a Material-modeled concept (color, typography, elevation) THEN
  - [ ] it references the corresponding `--mat-sys-*` token directly, with no `--ds-*` alias
- [ ] WHEN the theme is toggled between light and dark (OS preference change) THEN
  - [ ] every color token, both `--mat-sys-*` and `--ds-*`, resolves to the correct variant via `light-dark()`, with no JavaScript-driven class toggle
- [ ] WHEN a component's public API (its exported class, inputs, outputs) is inspected THEN
  - [ ] no Angular Material type, selector, or enum appears in it
- [ ] WHEN any component's source is inspected THEN
  - [ ] no `@Input()`/`@Output()` decorator or `EventEmitter` is used

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill|solution-design-system-structure]] - [[skills/angular/architecture/solutions/solution-design-system-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]] - [[skills/angular/architecture/solutions/solution-design-system-tokens.skill/Implementation/Repository.extend|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill|solution-design-system-components]] - [[skills/angular/architecture/solutions/solution-design-system-components.skill/Implementation/Repository.extend|Repository.extend]]
