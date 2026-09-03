---
name: registry-shared-state-project
description: Conflict Detection result for the `shared-state-project` element — the global NgRx store seam where each slice-adding solution registers its reducer + effects
tags:
  - concern/architecture
  - stack/typescript
  - element/shared-state-project
---

# Element
`element/shared-state-project` — `libs/shared/state`, the classical NgRx global store, and specifically the `store.config.ts` registration seam (`provideGlobalStore()`) where each cross-cutting slice registers its reducer + effects.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] (VP2, `.create` — `GlobalStore/shared-state.project.create` — the lib + the empty-but-wired root store + the `store.config.ts` seam, built for exactly this extension pattern)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (VP4, `.extend` — `GlobalStore/shared-state.project.extend` — registers the `connectivity` slice: `provideState(connectivityFeature)` + `provideEffects(ConnectivityEffects)`)

Grows further down the chain (delta-conflict-analysis.md [Finding 4](skills/angular/architecture/v3.1/delta-conflict-analysis.md#findings) — now closed):
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (VP5) — the `notifications` slice, at [`plateau-offline-full-monolith`](skills/angular/architecture/v3.1/monolith/plateau/plateau-offline-full-monolith/structure/shared-state/plateau-offline-full-monolith--project-shared-state.skill.md). **`solution-offline-sync` now carries its own `Implementation/GlobalStore/shared-state.project.extend.md`** (Finding 4 was "registered only in prose").
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] (VP7) — the `auth` slice, at [`plateau-multiuser-monolith`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/registry/shared-state-project.md) (**built — `solution-authentication` now carries `Implementation/GlobalStore/shared-state.project.extend.md`; Finding 4 fully closed**).

# Classification
`TMN` — Constraint `T` (VP4 `requires` VP2; VP5 requires VP4→VP2; VP7 requires VP2 — every slice-adding VP is gated on `GlobalStore`). Category `M` (code change to `store.config.ts`). Kind `N` (independent): each solution registers a *distinct* slice at the seam the create built; none touches another's reducer/effects. Mirrors dotnet v3.1's `app-infrastructure-csproj` "created by the first persistence solution, extended by the rest" pattern.

# Ordering
`source: constraint` — each slice-adding VP requires VP2, so `solution-global-store` (create) always precedes every `.extend`. The order among the extends does not matter (independent `provideState` calls).

# Resolution
**Canonical — no resolver.** The `store.config.ts` seam exists specifically to be extended once per slice. The `plateau-offline-read-monolith` example's `store.config.ts` registers `connectivity` alongside the root store; `store.config.spec.ts` asserts the slice is wired and `selectIsOnline` resolves.

# Architectural signal
N = 2 at `plateau-offline-read-monolith`; **N = 3 at `plateau-offline-full-monolith`** (global-store + offline-first + offline-sync); **N = 4 at [`plateau-multiuser-monolith`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/registry/shared-state-project.md)** (+ the `auth` slice, VP7). **Benign.** A single auditable global store extended once per cross-cutting slice is the intended design — the state-tiering policy's top tier — not a mis-drawn VP: the `store.config.ts` seam was built for exactly this, each `.extend` registers a distinct, member-disjoint slice, and every slice-adding VP is already gated on VP2 by a real constraint. This is the direct analogue of dotnet v3.1's `app-infrastructure-csproj` "created by the first persistence solution, extended by the rest".
