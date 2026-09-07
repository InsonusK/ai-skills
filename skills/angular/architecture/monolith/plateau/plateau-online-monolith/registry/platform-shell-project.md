---
name: registry-platform-shell-project
description: Conflict Detection result for the `platform-shell-project` element — the composition-root app, extended at bootstrap by several solutions
tags:
  - concern/architecture
  - stack/typescript
  - element/platform-shell-project
---

# Element
`element/platform-shell-project` — `apps/platform-shell`, the single deployable unit. Created as part of the workspace by `solution-repository-structure`; extended at its composition root (`app.config.ts` / `app.routes.ts` / `main.ts` / `project.json`) by several solutions.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] (`.extend` — `app.routes.ts` mounts each first-level feature segment via `loadChildren`) — present at `plateau-online-monolith`
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] (`.extend` — `app.config.ts` calls `provideGlobalStore()`) — present at `plateau-online-monolith`
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] (`.extend` — registers `withPreloading(SelectivePreloadingStrategy)`, marks `data.preload`, adds `budgets`) — added at `plateau-async-monolith` (VP1)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (`.extend` — `main.ts` registers `/sw.js`, `project.json` gains `build-sw`, the shell mounts `<ui-offline-banner>`) — added at `plateau-offline-read-monolith` (VP4)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (`.extend` — `app.config.ts` calls `provideOfflineSync()`) — added at `plateau-offline-full-monolith` (VP5)
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] (`.extend` — registers a global `ErrorHandler`) — added at [`plateau-multiuser-monolith`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/registry/platform-shell-project.md) (VP6)
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] (`.extend` — `withInterceptors([authInterceptor])`, one bootstrap `provideAppInitializer` silent refresh, `/login` + `/forbidden` routes) — added at [`plateau-multiuser-monolith`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/registry/platform-shell-project.md) (VP7)
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] (`.extend` — Native Federation host config) — added at `plateau-platform-host`

# Classification
`FMN` / `TMN` — the composition root. Category `M` (bootstrap wiring). Kind `N` (independent): each `.extend` adds one distinct provider or config block — a router mount, `provideGlobalStore()`, a preloading strategy + budgets, a service-worker registration, a `GlobalErrorHandler`, a federation host config. No two edit the same statement. Where a VP↔VP constraint exists it is `T`, otherwise `F`; canonical either way.

# Ordering
`source: ordering-only` — provider registration order in `ApplicationConfig.providers` does not matter for these blocks (Angular DI resolves lazily). `solution-federation-host`'s host wiring registers after the monolith's own bootstrap wiring, but only because `platform-host` composes the monolith plateau via `parent_plateaus` — the ordering is a consequence of composition, not a required sequence.

# Resolution
**Canonical — resolved by design, no resolver.** The composition root is meant to be the one place features wire themselves in. Each solution's structure skill (`plateau-{name}--project-platform-shell`) records its own addition with an `__Applied solutions:__` trailer.

# Architectural signal
N ≥ 3 (up to 7 solutions — `app-routing` + `global-store` + `performance-tuned-routing` + `offline-first` + `offline-sync` + `logging-global` + `authentication`, before `federation-host` at `plateau-platform-host`). **Benign.** A composition root extended once per cross-cutting concern is the correct design, not a mis-drawn VP. Recorded per the N≥3 rule to make the review explicit; the full VP6+VP7 review lives at [`plateau-multiuser-monolith/registry/platform-shell-project.md`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/registry/platform-shell-project.md).
