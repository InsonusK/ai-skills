---
name: registry-platform-shell-project
description: Conflict Detection result for the `platform-shell-project` element — the monolith composition-root app, extended at bootstrap by every monolith cross-cutting solution
tags:
  - concern/architecture
  - stack/typescript
  - element/platform-shell-project
---

# Element
`element/platform-shell-project` — `apps/platform-shell`, the single deployable unit. Created as part of the workspace by `solution-repository-structure`; extended at its composition root (`app.config.ts` / `app.routes.ts` / `main.ts` / `project.json`) by several solutions.

# Involved solutions
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] (`.extend` — `app.routes.ts` mounts each first-level feature segment via `loadChildren`)
- [[skills/angular/architecture/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] (`.extend` — `app.config.ts` calls `provideGlobalStore()`)
- [[skills/angular/architecture/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] (VP1, `.extend` — registers `withPreloading(SelectivePreloadingStrategy)`, marks `data.preload`, adds `budgets`)
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (VP4, `.extend` — `main.ts` registers `/sw.js`, `project.json` gains `build-sw`, the shell mounts `<ui-offline-banner>`)
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (VP5, `.extend` — `app.config.ts` calls `provideOfflineSync()`)
- [[skills/angular/architecture/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] (VP6, `.extend` — registers a global `ErrorHandler`)
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] (VP7, `.extend` — `withInterceptors([authInterceptor])`, one bootstrap `provideAppInitializer` silent refresh, `/login` + `/forbidden` routes)

`solution-federation-host` (from the `platform-host` catalog) also extends this same physical `apps/platform-shell` project, on top of everything above — but that intersection is recorded as its own entry, [[skills/angular/architecture/platform-host/registry/platform-shell-project.md|platform-host's own platform-shell-project.md]], not merged into this file. A `platform-host` plateau composes a `monolith` plateau via `parent_plateaus`, but its own extending solutions (`solution-federation-host`, `solution-session-sharing`, `solution-host-design-system-consumption`) belong to `platform-host`'s own `variability-map.md`, not the monolith's — see [delta-conflict-analysis Finding 5](skills/angular/architecture/delta-conflict-analysis.md#findings) for why the cross-catalog intersection stays a separate file rather than growing this one.

# Classification
`FMN` / `TMN` — the composition root. Category `M` (bootstrap wiring). Kind `N` (independent): each `.extend` adds one distinct provider or config block — a router mount, `provideGlobalStore()`, a preloading strategy + budgets, a service-worker registration, `provideOfflineSync()`, a `GlobalErrorHandler`, an HTTP interceptor + app initializer + two routes. No two edit the same statement. Where a VP↔VP constraint exists it is `T`, otherwise `F`; canonical either way.

# Ordering
`source: ordering-only` — provider registration order in `ApplicationConfig.providers` does not matter for these blocks (Angular DI resolves lazily).

# Resolution
**Canonical — resolved by design, no resolver.** The composition root is meant to be the one place features wire themselves in. Each solution's structure skill (`plateau-{name}--project-platform-shell`) records its own addition with an `__Applied solutions:__` trailer.

# Architectural signal
N = 7 at the deepest monolith plateau (`app-routing` + `global-store` + `performance-tuned-routing` + `offline-first` + `offline-sync` + `logging-global` + `authentication`). **Benign.** A composition root extended once per cross-cutting concern is the correct design, not a mis-drawn VP.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-online-monolith` | 2 | First real: `solution-app-routing` + `solution-global-store` | Baseline app bootstraps with routing and the store provider |
| `plateau-async-monolith` | 3 | `solution-performance-tuned-routing` (VP1) joins | Production build confirms selective preloading + budgets |
| `plateau-offline-read-monolith` | 4 | `solution-offline-first` (VP4) joins | `/sw.js` registration + offline banner confirmed in the built app |
| `plateau-offline-full-monolith` | 5 | `solution-offline-sync` (VP5) joins | `provideOfflineSync()` wired; `app.spec.ts` bootstraps with the replay orchestrator present |
| `plateau-multiuser-monolith` | 7 | `solution-logging-global` (VP6) and `solution-authentication` (VP7) join together | `app.config.ts` carries all six wirings; `app.spec.ts` + `auth.interceptor.spec.ts` confirm every provider is in place and exactly one silent-refresh dispatch per 401 |
