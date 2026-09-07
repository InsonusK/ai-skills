---
name: classical-ngrx-for-the-global-tier
description: Why the third state tier (libs/shared/state) is classical NgRx (actions/reducers/effects/selectors) rather than another Signal Store or a plain service
problem: solution-state-tiering establishes three tiers. Tiers 1-2 (component signal, feature Signal Store) are settled. The third tier — cross-cutting state shared by unrelated features — needs a concrete technology. The feature tier already uses @ngrx/signals; reusing it upward is the obvious default.
decision: The global tier is classical NgRx — @ngrx/store + @ngrx/effects. The action log gives cross-cutting flows (auth session lifecycle, offline-sync replay, conflict handling) an auditable, replayable history, and Effects give a first-class place for retry/timer/HTTP side effects that the flows on this tier all need. The feature tier stays @ngrx/signals.
tags:
  - solution/global-store
  - stack/typescript
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`solution-state-tiering` defines three tiers and settles the lower two. The third — state read or dispatched by more than one unrelated feature (auth session, connectivity, notifications, the offline-sync queue) — needs one concrete technology chosen for it. The feature tier already commits the workspace to `@ngrx/signals`, so "use a Signal Store here too" is the path of least resistance and needs a reason not to take it.

# Selected variant

**Selected variant:** [[#Classical NgRx (@ngrx/store + @ngrx/effects) for the global tier (selected)]]

# Searched variants

## Classical NgRx (@ngrx/store + @ngrx/effects) for the global tier (selected)

### Description
`libs/shared/state` holds one slice per cross-cutting concern, each with actions / reducer / effects / selectors. Reducers and effects are registered once in `apps/platform-shell` via `store.config.ts`. Components and feature Signal Stores dispatch plain actions and read selectors.

### Benefits
- The action log is an auditable, time-ordered record of every session and sync event — directly useful for the flows on this tier (a silent-refresh sequence, an offline replay with a conflict) where "what happened, in what order" is the debugging question.
- Effects are a first-class home for the retry loops, timers, `sendBeacon` flushes, and health-check polls these flows need — the feature tier has no equivalent seam.
- Redux DevTools time-travel works out of the box for the state most likely to produce a hard-to-reproduce bug.
- The catalog's own `solution-offline-sync` was written against exactly this shape (effect → facade → client, server-wins conflict in an effect).

### Costs
- Two state libraries in the workspace (`@ngrx/signals` for the feature tier, `@ngrx/store` for the global tier) — engineers learn both. Mitigated by the tiering rule making it unambiguous which one applies where.
- More boilerplate per slice than a Signal Store. Accepted: the global tier has few slices (four across the whole catalog) and they are the ones that most benefit from the ceremony.

## A Signal Store for the global tier too

### Description
`libs/shared/state` exposes `signalStore()`-based stores with `withMethods`, one per concern, provided at the root.

### Benefits
- One state library across all tiers; less to learn.
- Less boilerplate per slice.

### Costs
- No action log — the exact history of a session/sync sequence is not recoverable after the fact, which is where these flows fail.
- No Effects equivalent — retry/timer/polling logic goes into `withMethods` bodies or ad-hoc `effect()`s, scattered and harder to test in isolation.
- `solution-offline-sync` would have to be re-specified against a different orchestration shape.

## A hand-rolled service with signals

### Description
A plain injectable holding `signal()`s, with methods for the transitions, no state library at the global tier.

### Benefits
- Zero new dependency.

### Costs
- Reinvents actions, effects, and selectors badly; no DevTools; no consistent testing pattern; every cross-cutting concern grows its own idiosyncratic service. This is the "one tool stretched to fit every case" that `solution-state-tiering` exists to prevent.
