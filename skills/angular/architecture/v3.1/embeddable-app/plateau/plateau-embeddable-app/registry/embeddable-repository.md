---
name: registry-embeddable-repository
description: Conflict Detection result for the `embeddable-repository` element — the Native Federation remote workspace, created by solution-federation-remote and extended by the two remote-side consumption solutions
tags:
  - concern/architecture
  - stack/typescript
  - element/embeddable-repository
---

# Element
`element/embeddable-repository` — the independent remote repository: `federation.config.mjs`, `src/main.ts` (`initFederation`), the exposed module (`./Routes`), `package.json` (the `@platform/contracts` / `design-system` version ranges), `src/styles.scss`. Any tooling; a plain Angular CLI workspace here.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] (`.create` — `Repository.create` — the Native Federation remote baseline: `remoteEntry`, one exposed module, `@platform/contracts` + Angular strict singletons, independent CI/CD; `.extend` — `routes.ts.extend` — the exposed module mounts its own root segments)
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] (VP1, `.extend` — `session-consumption.extend` — `requirePermission` / `*hasPermission` reading `SESSION_CONTRACT`, a not-authenticated state)
- [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/solution-remote-design-system-consumption.skill.md|solution-remote-design-system-consumption]] (VP2, `.extend` — `federation.extend` — `design-system` as a version-negotiated `singleton: true, strictVersion: false` shared dependency + the standalone-dev theme import)

The retag from the shared `element/repository` `.create` conflation is recorded in [delta-conflict-analysis](skills/angular/architecture/v3.1/delta-conflict-analysis.md) — `solution-federation-remote` creates the *embeddable* repo, a genuinely different product from the Nx monolith and the CLI design-system workspaces.

# Classification
`FMN` / `TMN` — a repository-level bucket. Category `M` (federation-config / convention change). Kind `N` (independent): `federation-remote` establishes the `remoteEntry` + exposed module; `session-consumption` adds a `session/` folder reading one DI token; `remote-design-system-consumption` adds one shared-dependency line + a style import. Member-disjoint. Both VP-side solutions `depends_on solution-federation-remote` (the create-before-extend order is recorded); the two VPs have no constraint between them and are `F` (no legality gate — both are "near-universal but optional" per the [embeddable-app variability map](skills/angular/architecture/v3.1/embeddable-app/variability-map.md)).

# Ordering
`source: ordering-only` — the federation config is additive (an `exposes` entry, a `shared` entry, a `session/` folder). No solution must run before another to place its entry beyond the recorded `depends_on solution-federation-remote`.

# Resolution
**Canonical — resolved by design, no resolver.** A small remote repo where each of its (one common + two VP) solutions adds one distinct piece — the analogue of `monolith-repository` / `design-system-repository`. The [`plateau-embeddable-app` example](skills/angular/architecture/v3.1/embeddable-app/plateau/plateau-embeddable-app/plateau-embeddable-app.skill/example/) builds it: `ng build` emits a `remoteEntry.json` exposing `./Routes` and sharing `@platform/contracts` as a strict singleton; `ng test` covers `requirePermission` + the not-authenticated state.

# Architectural signal
N = 3. **Benign.** A contract-conformant remote repo touched by its own common + VP solutions is the correct design, not a mis-drawn variation point. Recorded per the delta-conflict-detection N≥3 rule.
