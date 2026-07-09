---
name: design-system-structure
description: Baseline repository structure for the design system — a separate Angular CLI multi-project workspace (library + demo app), ng-packagr build, and Changesets-based releases, published as an independently versioned npm package
domain: skill
type: architecture
version: 1.0
tags:
  - skill/architecture/solution
  - angular
  - design-system
  - ng-packagr
  - changesets
triggers:
  - Setting up the design system's own repository for the first time
  - Adding a new component or preparing a release
  - Reviewing the design system's build/release tooling
creates:
  - "design-system-repo (separate repository, projects/design-system, projects/demo)"
extends:
  # No existing repository — this is a separate repository, not part of the platform monorepo
depends_on:
  # None — this is the base solution for the design-system's own architecture graph
adr:
  - "[[adr/design-system-workspace-tooling.md]]"
  - "[[adr/library-build-tooling.md]]"
  - "[[adr/release-versioning-strategy.md]]"
  - "[[adr/component-preview-tooling.md]]"
---

# Goal

- Establish the design system as an independently versioned, independently repositoried npm package, consumable by the platform monorepo and by every independently deployed embeddable app
- Keep the tooling proportional to the repository's actual size (a library + a demo app), rather than importing the platform monorepo's Nx tooling by default
- Give the design system a reliable, low-discipline-dependent release process, since a misclassified breaking change would simultaneously affect multiple independent consumer teams

# Capabilities

- Independent versioning and release cadence, decoupled from the platform monorepo's own release cycle
- Ivy partial compilation output usable by consumers on a range of Angular versions, not locked to one specific version
- A low-friction component preview workflow that matches this team's actual prior tooling experience, rather than a conventional choice that already proved painful
- A release process where a breaking change is far less likely to slip out under a non-major version, given Changesets' deliberate, per-PR bump classification

# Core Principles

- The design system's repository is separate from the platform monorepo, matching the pattern already used for `@platform/contracts` (see the "Встраиваемость платформы" solution) — consumed as an external, independently versioned dependency, not a workspace-internal lib
- Workspace tooling is chosen for this repository's actual scale (two projects), not for consistency with the platform monorepo's Nx setup — Nx's benefits require a scale this repository doesn't have
- The library is built with ng-packagr, the Angular-team-maintained tool for producing Angular Package Format-compliant output
- Every change to the library's public surface is accompanied by a changeset, making version bump classification a deliberate, per-PR decision rather than a release-time judgment call
- Component preview happens in a self-built demo Angular app, not Storybook, based on direct prior experience with the friction Storybook introduced for this exact use case

# Adr

- [[adr/design-system-workspace-tooling.md|Angular CLI multi-project workspace instead of Nx]]
  - Selected variant: plain Angular CLI workspace — chosen because Nx's benefits (affected-builds, enforced boundaries, federation generators) don't meaningfully apply to a two-project repository
- [[adr/library-build-tooling.md|ng-packagr instead of a custom Vite/Rollup build]]
  - Selected variant: ng-packagr — the official tool for Angular Package Format compliance and Ivy partial compilation
- [[adr/release-versioning-strategy.md|Changesets instead of manual semver/CHANGELOG]]
  - Selected variant: Changesets — chosen because manual versioning risks a breaking change reaching multiple independent consumer teams under a non-major version
- [[adr/component-preview-tooling.md|Self-built demo Angular app instead of Storybook]]
  - Selected variant: demo app — chosen based on direct prior experience with Storybook's friction for this exact use case

# Requirements

SOLUTION:
- None — this is the base solution for the design-system's own architecture graph, analogous to the "Структура репозитория" solution for the platform monorepo

NPM:
- ng-packagr
  - Library build, per [[adr/library-build-tooling.md]]
- @changesets/cli
  - Version bump classification and CHANGELOG generation, per [[adr/release-versioning-strategy.md]]

# Template Skill Mutations

REPOSITORY:
- [[./Implementation/Repository.create.md|Repository]] - create - Angular CLI multi-project workspace with the publishable `design-system` library, the `demo` preview application, ng-packagr build config, and Changesets configuration

No project- or artifact-level (component/service) implementation files are introduced by this solution — it establishes only the repository-level structure. Individual components and tokens are introduced by the "Дизайн-система: токены и theming" and "Дизайн-система: компонентный слой" solutions.

# Workflow

## Bootstrap the design system repository (happy path)

1. A new Angular CLI multi-project workspace is created, with `projects/design-system` (library, ng-packagr) and `projects/demo` (application).
2. Changesets is configured (`.changeset/config.json`), and CI is set up to require a changeset file on any PR touching the library's public surface.
3. The demo app imports `design-system` as a workspace-local path dependency during development, exercising the same import path a real external consumer would use once published.

## Adding a new component and releasing it (happy path)

1. A new component is added to `projects/design-system`.
2. A corresponding example page is added to `projects/demo`, per this solution's SHOULD rule, so it's visually reviewable.
3. The PR includes a changeset file describing the change and its intended bump (e.g. `minor`, for a new non-breaking component).
4. On release, Changesets tooling aggregates pending changesets, bumps the version, generates the CHANGELOG, and publishes the built (ng-packagr) output to npm.

## Missing changeset (failure path, caught in CI)

1. A PR modifies the library's public API but does not include a changeset file.
2. CI fails per [[./Implementation/Repository.create.md#Unittest TestCases]], blocking merge until a changeset is added.
3. This prevents the release-time uncertainty ("what should this bump be? did we forget something since the last release?") that motivated choosing Changesets in the first place.

# Rules

## MUST
- [[./Implementation/Repository.create.md#MUST|Repository.create]]

## SHOULD
- [[./Implementation/Repository.create.md#SHOULD|Repository.create]]

# Anti-patterns

- [[./Implementation/Repository.create.md|See Repository.create.md]] — publishing without a changeset; reintroducing Storybook for an individual component after this repository already decided against it.

# Check list

- [ ] The repository is a plain Angular CLI multi-project workspace, not Nx
- [ ] The library builds via ng-packagr, producing Angular Package Format-compliant output
- [ ] Every PR touching the library's public surface includes a changeset file
- [ ] The `demo` app is never published to npm — only `design-system` is
- [ ] Every shipped component has a corresponding example page in `demo`
