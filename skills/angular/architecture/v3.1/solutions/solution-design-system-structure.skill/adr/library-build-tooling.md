---
name: library-build-tooling
description: What tool packages the design system's Angular library for npm publishing
problem: An Angular library needs to be compiled and packaged according to the Angular Package Format, with Ivy partial compilation, for safe consumption by both the platform monorepo and any embeddable app's own repository
decision: Use ng-packagr
tags:
  - solution/design-system-structure
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The design system is published as an npm package, consumed by the platform monorepo and by every independently repositoried embeddable app (per `solution-federation-host`). Consumers may be on different Angular versions within a supported range, so the package needs Ivy partial compilation (compiled at publish time, fully compiled by each consumer's own Angular version) rather than a fully pre-compiled build tied to one specific Angular version.

# Selected variant

**Selected variant:** [[#ng-packagr]]

# Searched variants

## ng-packagr

### Description

The official Angular library packaging tool, producing output that conforms to the Angular Package Format (APF): Ivy partial compilation, correctly structured `package.json` exports, and pre-built metadata consumers' own Angular CLI builds rely on.

### Benefits

- Produces Ivy partial compilation output, letting each consumer (the platform monorepo, and every independent embeddable-app repository) finish compilation with their own installed Angular version, rather than being locked to whatever version built the package
- Official, Angular-team-maintained tool, so it stays aligned with however the Angular Package Format evolves — a hand-rolled build is a maintenance liability against a moving target
- Directly integrated with the Angular CLI's own library generator (`ng generate library`), minimizing custom build configuration

### Costs

- Less flexible than a hand-rolled Rollup/Vite config for unusual bundling needs (e.g. non-standard entry points) — though the design system's needs (a standard component library) don't require this flexibility

## Custom build (Vite/Rollup directly)

### Description

Hand-configure Vite or Rollup to compile and bundle the library without ng-packagr.

### Benefits

- Full control over every aspect of the build
- Potentially faster iteration for very unusual bundling requirements

### Costs

- Responsible for correctly implementing the Angular Package Format by hand (Ivy partial compilation, `package.json` exports map, metadata) — a nontrivial, easy-to-get-subtly-wrong surface that ng-packagr already solves
- Diverges from the tooling every consumer's own Angular CLI build expects, risking subtle incompatibilities as Angular's own compiler evolves
