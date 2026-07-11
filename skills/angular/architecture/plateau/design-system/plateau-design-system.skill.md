---
name: plateau-design-system
description: The design system as an independently versioned npm package — M3 theme, custom design tokens, and a signal-based ds-* component library, ready to be consumed by the platform monorepo and any embeddable app
domain: skill
type: template
version: 20260711120000
tags:
  - skill/template/plateau
  - plateau/design-system
created_by:
  - "[[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill|solution-design-system-structure]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-tokens.skill/solution-design-system-tokens.skill|solution-design-system-tokens]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill|solution-design-system-components]]"
---

> This plateau lives in its own repository, separate from the main Nx platform monorepo. It is a standalone product (an npm package), not a stage that the main application's own plateau chain passes through. The main application's [[skills/angular/architecture/plateau/tested/plateau-tested.skill.md|tested]] plateau onward consumes this package directly as an npm dependency; federation-aware consumption (version-negotiated sharing between the platform host and embeddable apps) is added by the [[skills/angular/architecture/plateau/platform/plateau-platform.skill.md|platform]] plateau.

# Core Principles

- The design system is versioned and released independently of every consumer, via Changesets — a misclassified breaking change is far less likely to slip out under a non-major version
- Angular Material's own M3 `--mat-sys-*` tokens are consumed directly wherever they already model a concept; a small `--ds-*` layer exists only for genuine domain-specific gaps (status/priority colors, spacing, radius)
- Every component fully encapsulates Angular Material — its own `ds-*` selector, signal-based (`input()`/`output()`/`model()`) API, and independently designed usage axes, with no Material type ever reaching the public surface
- A single, fixed brand palette is used everywhere for now; multi-tenant theming is explicitly deferred until a real requirement appears

# Capabilities

- packaging & release
  - Plain Angular CLI multi-project workspace (library + demo), ng-packagr build producing Angular Package Format output, Changesets-driven versioning and CHANGELOG
- theming
  - One `mat.theme()` definition applied at the root, native `light-dark()` for light/dark with no JavaScript, `--ds-*` tokens for the handful of concepts Material doesn't model
- components
  - Signal-based, fully encapsulated `ds-*` components; each one's internal implementation independently decided between delegating to Angular Material or a fully custom build
- preview
  - Every shipped component gets a live example page in the `demo` app instead of Storybook

# Usecases

## Add a new component to the library

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant Lib as projects/design-system
    participant Demo as projects/demo
    participant CS as Changesets

    Dev->>Dev: identify real usage axes (not Material's categorization)
    Dev->>Lib: scaffold Ds{Name}Component (ds-* selector, input()/output()/model())
    Dev->>Lib: decide delegate-to-Material vs fully custom per real requirement
    Dev->>Demo: add example page
    Dev->>CS: add changeset describing the change
    CS-->>Dev: on release — version bump, CHANGELOG, npm publish
```

## Consuming application picks up a design-system release

```mermaid
sequenceDiagram
    autonumber
    actor App as Consuming app (platform or embeddable)
    participant NPM as design-system (npm)

    App->>NPM: bump design-system dependency to new version
    App->>App: apply theme.scss once at the app root
    App->>App: use ds-* components; no Material knowledge required
    Note over App,NPM: A breaking change only reaches App at the version App explicitly opts into — Changesets guarantees it was flagged major
```
