---
name: solution-app-routing
description: Hierarchical route ownership — shell, embeddable modules, and features each define routes only relative to their own root segment
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - angular
  - routing
triggers:
  - Adding routing to a new feature
  - Mounting a new embeddable module or feature into the shell
  - Reviewing whether a route definition reaches outside the segment it owns
creates:
  - libs/{feature}/feature/src/lib/{feature}.routes.ts
extends:
  - apps/platform-shell/src/app/app.routes.ts
  - "{embeddable-module}/routes.ts"
  - libs/{feature}/feature
  - Repository
depends_on:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|Структура репозитория (база)]]"
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|Встраиваемость платформы]]"
adr:
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/adr/route-ownership-location|Route Ownership Location ADR]]"
---

# Goal

- Give every level of the application (shell, embeddable module, feature) an unambiguous, non-overlapping authority over the part of the URL tree it owns
- Keep routing changes inside a feature or module from ever requiring a change to the level above it, preserving the affected-based CI benefit from solution #1
- Make the shell agnostic to whether a mounted segment is a directly-owned feature or a federated embeddable module (solution #2) — both are mounted the same way

# Capabilities

- Internal navigation changes inside a feature never touch `apps/platform-shell`
- An embeddable module or feature can be freely remounted at a different segment without internal changes, since it never assumes its own mount prefix
- One consistent mounting mechanism (`loadChildren` against an exported `Routes` array) for both directly-owned features and federated embeddable modules

# Core Principles

- Routes are owned hierarchically: the shell only knows first-level root segments (a module's or a directly-owned feature's root); a module only knows the root segments of the features it contains; a feature only knows paths relative to its own root
- A project never defines a path that reaches outside the segment it owns, and never assumes the segment name under which its parent will mount it
- The parent (shell or module) assigns the mount segment at the point of mounting — the child never bakes its own name into its own routes
- A feature's routes are part of its public API, exported from `index.ts` alongside its Signal Store

# Adr

- [[skills/angular/architecture/solutions/solution-app-routing.skill/adr/route-ownership-location|Hierarchical route ownership instead of a single centralized routes file in the shell]]
  - Selected variant: hierarchical ownership — chosen to mirror the module-boundary principle from solution #1, preserve affected-based CI, and compose cleanly with the federation-based embeddability from solution #2

# Requirements

SOLUTION:
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|Структура репозитория (база)]]
  - Extends the `index.ts` public-API convention to include a feature's exported `Routes`
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|Встраиваемость платформы]]
  - An embeddable module is mounted by the shell the same way a directly-owned feature is — as a root segment resolved via `loadChildren`

NPM:
- @angular/router
  - Standard Angular Router; no additional routing library is introduced by this solution

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|Repository]] - extend - add the convention that every routable feature exports its `Routes` via `index.ts`, relative to its own root only
PROJECT:
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|apps/platform-shell]] - extend - root `app.routes.ts` mounting module/feature root segments only
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/EmbeddableModule/routes.ts.extend|Embeddable module (generic pattern)]] - extend - mounts its own features' root segments, applied by any multi-feature embeddable module
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|{Feature}/feature (generic pattern)]] - create - feature's own root-relative routes, exported from `index.ts`, applied by any future feature-owning solution

# Workflow

## Mount a new feature directly under the shell (happy path)

1. The feature exports `{FEATURE}_ROUTES` from its `index.ts`, with paths relative to its own root (e.g. `''`, `':id'`).
2. The shell adds one entry to `app.routes.ts`: `{ path: 'feature1', loadChildren: () => import('@feature1/feature').then(m => m.FEATURE1_ROUTES) }`.
3. The feature is now reachable at `/feature1` and `/feature1/:id`, without the shell knowing anything about what's inside.

## Mount an embeddable module (happy path)

1. The embeddable module exposes its own root-relative routes as part of its federation entry point (see [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/EmbeddableModule/routes.ts.extend]]).
2. The shell mounts it exactly like a feature — one root-segment entry in `app.routes.ts`, resolved via the runtime remote registry from the platform-embeddability solution instead of a static import.
3. Inside the module, its own features are mounted the same way, one level down.

![Mount an embeddable module (happy path)](skills/angular/architecture/solutions/solution-app-routing.skill/diagrams/mount-an-embeddable-module-happy-path.mmd)

## Boundary violation (failure path)

1. A developer adds a route to `apps/platform-shell` that targets a path nested inside a feature (e.g. `feature1/page`) instead of mounting only `feature1`.
2. This is caught in review against the rule in [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend#MUST NOT]] — the shell must not reference a path two or more levels below its own mount point.
3. Fix: the shell mounts only `feature1`; the `page` path is defined inside the feature's own `{feature}.routes.ts`.

# Rules

## MUST
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend#MUST|Repository]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend#MUST|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/EmbeddableModule/routes.ts.extend#MUST|EmbeddableModule/routes.ts.extend]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create#MUST|{feature}.routes.ts]]

## MUST NOT
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend#MUST NOT|Repository]]

# Anti-patterns

- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/Repository.extend|See Repository.extend.md]] — a feature baking its own mount segment into its routes; the shell reaching into a feature's internal path structure.
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|See platform-shell.project.extend.md]] — the shell defining a nested path instead of mounting a single root segment.
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/EmbeddableModule/routes.ts.extend|See routes.ts.extend.md]] — a module hardcoding its own expected mount prefix.
- [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/FeatureRoutes/{Feature}.project.extend/{feature}.routes.ts.create|See {feature}.routes.ts.create.md]] — a feature baking its own name into its own route paths.

# Check list

- [ ] `apps/platform-shell`'s `app.routes.ts` contains only first-level root segments, no nested paths
- [ ] Every routable feature exports its `Routes` from `index.ts`, with paths relative only to its own root
- [ ] No project at any level references or assumes the mount segment assigned to it by its parent
- [ ] An embeddable module mounts its own features the same way the shell mounts modules/features — one root segment per `loadChildren` entry