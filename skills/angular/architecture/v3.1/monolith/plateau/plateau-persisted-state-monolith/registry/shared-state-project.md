---
name: registry-shared-state-project
description: Conflict Detection result for the `shared-state-project` element at plateau-persisted-state-monolith — the global NgRx store seam, now extended by the `preferences` slice + its persistence metaReducer (VP8), reaching N = 5
tags:
  - concern/architecture
  - stack/typescript
  - element/shared-state-project
---

# Element
`element/shared-state-project` — `libs/shared/state`, the classical NgRx global store, and specifically the `store.config.ts` registration seam (`provideGlobalStore()`) where each cross-cutting slice registers its reducer + effects (and, for VP8, a persistence metaReducer).

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] (VP2, `.create` — `GlobalStore/shared-state.project.create` — the lib + the empty-but-wired root store + the `store.config.ts` seam, built for exactly this extension pattern)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (VP4, `.extend` — `GlobalStore/shared-state.project.extend` — the `connectivity` slice)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (VP5, `.extend` — `GlobalStore/shared-state.project.extend` — the `notifications` slice)
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] (VP7, `.extend` — `GlobalStore/shared-state.project.extend` **+** `GlobalStore/auth.store.ts.create` — the `auth` slice: `provideState(authFeature)` + `provideEffects(AuthEffects)`, in-memory access token, permission strings, silent refresh)
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] (VP8, `.extend` — `GlobalStore/shared-state.project.extend` **+** `GlobalStore/persisted-state.ts.create` **+** `GlobalStore/preferences.store.ts.create` — the `persistence/` mechanism + the `preferences` slice, registered via the three-arg `provideState(preferencesFeature.name, preferencesFeature.reducer, { metaReducers: [persistKeys(...)] })`)

This is the shallowest plateau where all five coexist (VP8 requires VP2; VP4/VP5/VP7 already landed in the parent chain).

# Classification
`TMN` — Constraint `T` (VP4 `requires` VP2; VP5 requires VP4→VP2; VP7 requires VP2; VP8 requires VP2 — every slice-adding VP is gated on `GlobalStore` by a real `depends_on` edge). Category `M` (code change to `store.config.ts`). Kind `N` (independent): each solution registers a *distinct* slice (`connectivity` / `notifications` / `auth` / `preferences`) at the seam the create built; the VP8 metaReducer is a **feature-local** metaReducer on the `preferences` `provideState` only — it does not touch any other slice's registration. Mirrors dotnet v3.1's `app-infrastructure-csproj` "created by the first persistence solution, extended by the rest" pattern.

# Ordering
`source: constraint` — each slice-adding VP requires VP2, so `solution-global-store` (create) always precedes every `.extend`. The order among the extends does not matter (independent `provideState` / `provideEffects` calls; the VP8 metaReducer is scoped to its own `provideState`).

# Resolution
**Canonical — no resolver.** The example's `store.config.ts` registers `connectivity`, `notifications`, `auth` and `preferences` alongside the root store; `store.config.spec.ts` asserts all four are wired **and** that `preferences` rehydrates from `localStorage` on init while `auth` carries no persistence metaReducer.

# Architectural signal
N = 2 at `plateau-offline-read-monolith`; N = 3 at `plateau-offline-full-monolith`; N = 4 at `plateau-multiuser-monolith`; **N = 5 here** (global-store + offline-first + offline-sync + authentication + persisted-state). **Benign.** A single auditable global store extended once per cross-cutting slice is the intended design — the state-tiering policy's top tier — not a mis-drawn VP: the `store.config.ts` seam was built for exactly this, each `.extend` registers a distinct, member-disjoint slice, and every slice-adding VP is already gated on VP2 by a real constraint. VP8's contribution is additive in a second dimension (a metaReducer on one slice's registration) and structurally cannot collide with the others.
