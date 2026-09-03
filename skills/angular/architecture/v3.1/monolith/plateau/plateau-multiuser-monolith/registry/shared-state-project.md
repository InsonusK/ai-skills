---
name: registry-shared-state-project
description: Conflict Detection result for the `shared-state-project` element at plateau-multiuser-monolith — the global NgRx store seam, now extended by the `auth` slice (VP7), reaching N = 4
tags:
  - concern/architecture
  - stack/typescript
  - element/shared-state-project
---

# Element
`element/shared-state-project` — `libs/shared/state`, the classical NgRx global store, and specifically the `store.config.ts` registration seam (`provideGlobalStore()`) where each cross-cutting slice registers its reducer + effects.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]] (VP2, `.create` — `GlobalStore/shared-state.project.create` — the lib + the empty-but-wired root store + the `store.config.ts` seam, built for exactly this extension pattern)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] (VP4, `.extend` — `GlobalStore/shared-state.project.extend` — the `connectivity` slice)
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] (VP5, `.extend` — `GlobalStore/shared-state.project.extend` — the `notifications` slice)
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] (VP7, `.extend` — `GlobalStore/shared-state.project.extend` **+** `GlobalStore/auth.store.ts.create` — the `auth` slice: `provideState(authFeature)` + `provideEffects(AuthEffects)`, plus the in-memory access token, permission strings and silent refresh)

This is the shallowest plateau where all four coexist (VP7 requires VP2; VP4 and VP5 already landed in the parent chain).

# Classification
`TMN` — Constraint `T` (VP4 `requires` VP2; VP5 requires VP4→VP2; VP7 requires VP2 — every slice-adding VP is gated on `GlobalStore` by a real `depends_on` edge). Category `M` (code change to `store.config.ts`). Kind `N` (independent): each solution registers a *distinct* slice (`connectivity` / `notifications` / `auth`) at the seam the create built; none touches another's reducer, effects or selectors. Mirrors dotnet v3.1's `app-infrastructure-csproj` "created by the first persistence solution, extended by the rest" pattern.

# Ordering
`source: constraint` — each slice-adding VP requires VP2, so `solution-global-store` (create) always precedes every `.extend`. The order among the extends does not matter (independent `provideState` / `provideEffects` calls).

# Resolution
**Canonical — no resolver.** Delta-conflict-analysis [Finding 4](skills/angular/architecture/v3.1/delta-conflict-analysis.md#findings) is now fully closed: `solution-authentication` carries its own `Implementation/GlobalStore/shared-state.project.extend.md` (previously the `auth` slice was registered only in prose). The example's `store.config.ts` registers `connectivity`, `notifications` and `auth` alongside the root store; `store.config.spec.ts` asserts all three are wired.

# Architectural signal
N = 2 at `plateau-offline-read-monolith`; N = 3 at `plateau-offline-full-monolith`; **N = 4 here** (global-store + offline-first + offline-sync + authentication). **Benign.** A single auditable global store extended once per cross-cutting slice is the intended design — the state-tiering policy's top tier — not a mis-drawn VP: the `store.config.ts` seam was built for exactly this, each `.extend` registers a distinct, member-disjoint slice, and every slice-adding VP is already gated on VP2 by a real constraint.
