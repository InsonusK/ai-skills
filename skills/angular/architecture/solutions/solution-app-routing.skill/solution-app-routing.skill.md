---
name: solution-app-routing
description: Hierarchical route ownership — shell, embeddable modules, and features each define routes only relative to their own root segment
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - stack/typescript
  - routing
  - framework/angular
  - concern/architecture
  - solution/app-routing

triggers:
  - Adding routing to a new feature
  - Mounting a new embeddable module or feature into the shell
  - Reviewing whether a route definition reaches outside the segment it owns
creates:
  - libs/{feature}/feature/src/lib/{feature}.routes.ts
extends:
  - apps/platform-shell/src/app/app.routes.ts
  - libs/{feature}/feature
  - Repository
depends_on:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]"
adr:
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/adr/route-ownership-location|Route Ownership Location ADR]]"
---

# Goal

- Give every level of the application (shell, feature) an unambiguous, non-overlapping authority over the part of the URL tree it owns
- Keep routing changes inside a feature from ever requiring a change to the level above it, preserving the affected-based CI benefit from solution #1
- Define hierarchical route ownership as a generic pattern any future level of mounting (including an embeddable module, once the platform-embeddability solution introduces that concept) can reuse without modification

# Capabilities

- Internal navigation changes inside a feature never touch `apps/platform-shell`
- A feature can be freely remounted at a different segment without internal changes, since it never assumes its own mount prefix
- One consistent mounting mechanism (`loadChildren` against an exported `Routes` array) for every directly-owned feature — reusable unchanged by any later solution that introduces another kind of mounting parent

# Core Principles

- Routes are owned hierarchically: the shell only knows first-level root segments (a directly-owned feature's root); a feature only knows paths relative to its own root
- A project never defines a path that reaches outside the segment it owns, and never assumes the segment name under which its parent will mount it
- The parent (the shell, at this plateau) assigns the mount segment at the point of mounting — the child never bakes its own name into its own routes
- A feature's routes are part of its public API, exported from `index.ts` alongside its Signal Store

# Adr

- [[skills/angular/architecture/solutions/solution-app-routing.skill/adr/route-ownership-location|Hierarchical route ownership instead of a single centralized routes file in the shell]]
  - Selected variant: hierarchical ownership — chosen to mirror the module-boundary principle from solution #1, preserve affected-based CI, and stay generic enough for any future mounting parent to reuse without modification

# Requirements

SOLUTION:
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]
  - Extends the `index.ts` public-API convention to include a feature's exported `Routes`

NPM:
- @angular/router
  - Standard Angular Router; no additional routing library is introduced by this solution

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository]] - extend - add the convention that every routable feature exports its `Routes` via `index.ts`, relative to its own root only
PROJECT:
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|apps/platform-shell]] - extend - root `app.routes.ts` mounting feature root segments only
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|{Feature}/feature (generic pattern)]] - create - feature's own root-relative routes, exported from `index.ts`, applied by any future feature-owning solution

# Workflow

## Mount a new feature directly under the shell (happy path)

1. The feature exports `{FEATURE}_ROUTES` from its `index.ts`, with paths relative to its own root (e.g. `''`, `':id'`).
2. The shell adds one entry to `app.routes.ts`: `{ path: 'feature1', loadChildren: () => import('@feature1/feature').then(m => m.FEATURE1_ROUTES) }`.
3. The feature is now reachable at `/feature1` and `/feature1/:id`, without the shell knowing anything about what's inside.

## Boundary violation (failure path)

1. A developer adds a route to `apps/platform-shell` that targets a path nested inside a feature (e.g. `feature1/page`) instead of mounting only `feature1`.
2. This is caught in review against the rule in [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend#MUST NOT]] — the shell must not reference a path two or more levels below its own mount point.
3. Fix: the shell mounts only `feature1`; the `page` path is defined inside the feature's own `{feature}.routes.ts`.

# Rules

## MUST
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend#MUST|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create#MUST|{feature}.routes.ts]]

## MUST NOT
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend#MUST NOT|Repository]]

# Anti-patterns

- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|See Repository.extend.md]] — a feature baking its own mount segment into its routes; the shell reaching into a feature's internal path structure.
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|See platform-shell.project.extend.md]] — the shell defining a nested path instead of mounting a single root segment.
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|See {feature}.routes.ts.create.md]] — a feature baking its own name into its own route paths.

# Check list

- [ ] `apps/platform-shell`'s `app.routes.ts` contains only first-level root segments, no nested paths
- [ ] Every routable feature exports its `Routes` from `index.ts`, with paths relative only to its own root
- [ ] No project at any level references or assumes the mount segment assigned to it by its parent