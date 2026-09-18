---
name: registry-platform-shell-project
description: Conflict Detection result for the `platform-shell-project` element at plateau-platform-host — the cross-catalog N≥3 point where solution-federation-host (+ session-sharing + host-design-system-consumption) join the monolith's own shell extenders
tags:
  - concern/architecture
  - stack/typescript
  - element/platform-shell-project
---

# Element
`element/platform-shell-project` — `apps/platform-shell`, the single deployable Angular application, at its composition root (`federation.config.mjs`, `src/main.ts`, `app.config.ts`, `app.routes.ts`, `sw-src.ts`).

# Involved solutions
Project created by `solution-repository-structure` as part of the monolith workspace. `.extend`ed, in the composed monolith plateau, by `app-routing` / `global-store` / `performance-tuned-routing` / `offline-first` / `offline-sync` / `logging-global` / `authentication` — see [[skills/angular/architecture/monolith/registry/platform-shell-project.md|monolith's own platform-shell-project.md]]. This plateau adds:
- [[skills/angular/architecture/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] — `federation.config.mjs`, `initFederation(...)` in `main.ts`, `RemoteRegistryService`, the remote route mount, and (conditionally) the 5th SW rule
- [[skills/angular/architecture/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] — `{ provide: SESSION_CONTRACT, useExisting: HostSession }` in `app.config.ts`
- [[skills/angular/architecture/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md|solution-host-design-system-consumption]] — the `design-system` shared-dependency declaration + the root-styles theme import

This is the shallowest plateau where the federation extenders coexist with the monolith's own — a `platform-host` plateau composes a `monolith` plateau via `parent_plateaus`, so per [delta-conflict-analysis Finding 5](skills/angular/architecture/delta-conflict-analysis.md#findings) the cross-catalog intersection lives here, not in the monolith map.

# Classification
`FMN` / `TMN` — Category `M` (bootstrap wiring). Kind `N` (independent): each `.extend` adds one distinct block — a federation config, an `initFederation` call, a `SESSION_CONTRACT` binding, a shared-dependency declaration, a root-styles import, a 5th SW rule. No two edit the same statement. `solution-federation-host` `depends_on solution-repository-structure` + `solution-app-routing`; `solution-session-sharing` `depends_on` the monolith `solution-authentication` + `solution-platform-contracts` + `solution-federation-host`; `solution-host-design-system-consumption` `depends_on solution-federation-host`.

# Ordering
`source: ordering-only` — the federation host wiring registers after the monolith's own bootstrap wiring, but only as a consequence of composition (`parent_plateaus`), not a required sequence. Provider order in `ApplicationConfig.providers` does not matter for these blocks; `initFederation` must precede `import('./bootstrap')`, which the `main.ts` structure enforces directly, not via provider ordering.

# Resolution
**Canonical — resolved by design, no resolver.** The composition root accumulates one bootstrap wiring per cross-cutting capability, federation included. The `example/` (a limited federation smoke test) wires `federation.config.mjs` + `initFederation` + `RemoteRegistryService` + the `SESSION_CONTRACT` provider + the remote route mount; `remote-registry.service.spec.ts` and `host-session.spec.ts` confirm the runtime-resolution and single-session behaviour.

# Architectural signal
**N ≥ 3 here** (federation-host + session-sharing + host-design-system-consumption on top of the monolith's own seven extenders — see [[skills/angular/architecture/monolith/registry/platform-shell-project.md|monolith's own platform-shell-project.md]] for that side of the growth). **Benign.** This is what a composition root is for. Not a mis-drawn VP.

# Growth history
| Plateau | N (this catalog's own contributors) | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-platform-host` | 3 | First (and, so far, only) plateau where `solution-federation-host`, `solution-session-sharing`, `solution-host-design-system-consumption` are simultaneously present, on top of the monolith's own already-composed shell | The `example/` federation smoke test wires all three; `remote-registry.service.spec.ts` and `host-session.spec.ts` confirm runtime resolution and single-session behaviour |
