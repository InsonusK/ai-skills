---
name: solution-persisted-state
description: Sync selected NgRx state (a GlobalStore slice or a feature Signal Store) to localStorage or IndexedDB so it survives a browser session — user preferences, draft forms, filter state — never the access token
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - ngrx
  - state-management
  - framework/angular
  - concern/architecture
  - solution/persisted-state

whenToUse: when a piece of NgRx state needs to survive a browser session (preferences, drafts, filters), or when reviewing whether persisted state accidentally includes sensitive data
creates: []
extends:
  - libs/shared/state (opt-in storage-sync on a slice)
  - libs/{feature}/feature (opt-in storage-sync on a feature Signal Store)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
adr: []
---

> **Draft contract — no consumer yet.** This solution realizes the owner-confirmed `PersistedState` VP (monolith VP8). No V1 solution provides it (`offline-sync`'s Dexie queue and `logging-global`'s IndexedDB queue are feature-specific, not a general state-persistence mechanism). The shape below is a sketch; full Implementation is deferred until a real consumer exists.

# Goal
- Let a chosen piece of NgRx state (a `GlobalStore` slice, or a feature Signal Store) be written to `localStorage` / IndexedDB on change and rehydrated on bootstrap.
- Make the opt-in **explicit per slice / per store** — persistence is never automatic and never whole-store.
- Guarantee sensitive fields (the access token above all) are excluded.

# Core Principle
- Persistence is opt-in at the slice / feature-store level, declaring exactly which keys are persisted — never `*`.
- `localStorage` for small synchronous values (a chosen tab, a filter set); IndexedDB for larger or structured drafts.
- Rehydration happens once, at bootstrap, before the first render that reads the state.
- The `auth` slice's `accessToken` (and anything `solution-authentication` marks sensitive) is **never** persisted — `solution-authentication`'s `token-storage-strategy` ADR forbids it and this solution enforces it.

# Boundaries
- monolith VP8, `requires GlobalStore` (VP2) — or the feature Signal Store tier.
- Not offline sync — persisted state is a per-session convenience, not a durable write queue (`solution-offline-sync`).
- Not the service-worker cache (`solution-offline-first`) — that is transport-level, this is state-level.

# Rules

## MUST
- Never persist a field an owning solution marks sensitive — the access token, PII, anything under a `@sensitive` marker.
  - Risk: reintroduces the XSS token-theft vector `solution-authentication` closed off.
  - Fix: an allow-list of persisted keys per slice; a build-time check that the auth token key is never on one.
- Never persist a whole store or slice with `*` — always an explicit key list.

# Check list
- [ ] Every persisted slice/store declares an explicit key allow-list.
- [ ] No auth token / PII key appears on any allow-list.
- [ ] Rehydration completes before the first render that reads the state.
