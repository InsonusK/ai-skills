---
name: solution-persisted-state
description: Sync an explicit, allow-listed subset of NgRx state (a GlobalStore slice or a feature Signal Store draft) to localStorage / sessionStorage / IndexedDB so it survives a browser session — user preferences, in-progress form drafts, filter state — with a build-time guard that the access token can never be persisted
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

whenToUse: when a piece of NgRx state needs to survive a browser session (preferences, an in-progress form draft, a filter set), when wiring the persistKeys() metaReducer or withPersistedDraft(), or when reviewing whether persisted state accidentally includes sensitive data
creates: []
extends:
  - libs/shared/state (persistence/ mechanism + the preferences slice + persistKeys metaReducer registration)
  - libs/{feature}/feature (opt-in withPersistedDraft on a dedicated draft Signal Store)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/storage-backend-choice.md|storage-backend-choice]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/rehydration-timing.md|rehydration-timing]]"
---

# Goal
- Let a chosen piece of NgRx state — a `GlobalStore` slice, or a feature Signal Store draft — be written to browser storage on change and rehydrated on the next visit, before the first render that reads it.
- Make the opt-in **explicit per slice / per store**, declaring exactly which keys are persisted — never `*`, never the whole slice.
- Make it structurally impossible to persist a field an owning solution marks sensitive — the access token above all.

# Capabilities
- **preferences that stick** — a `preferences` slice (theme, list density, last-opened feature tab) in `libs/shared/state`, persisted to `localStorage`, rehydrated synchronously so there is no flash of default theme.
- **form drafts that survive a reload** — a feature adds a dedicated `{Feature}DraftStore` composed with `withPersistedDraft()`; a user who reloads mid-entry comes back to their half-filled form; the draft clears on successful submit.
- **a sensitive-key guard** — `persistKeys()` and `withPersistedDraft()` both call `assertPersistable()`, which throws at construction if the allow-list names any key on the central `SENSITIVE_STATE_KEYS` registry (`accessToken`, `refreshToken`, …). A misconfigured allow-list fails the app on boot and the test suite in CI.
- **per-tab and large-draft variants** — pass `sessionStorage` for state that must not outlive the tab; use a Dexie-backed draft (hydrated in `onInit`) for a draft too large or structured for a JSON string.

# Core Principle
- Persistence is opt-in at the slice / feature-store level, declaring a finite key allow-list — never `*`.
- `localStorage` is the default backend (synchronous, ~5 MB, survives a tab close); `sessionStorage` for per-tab state; IndexedDB via Dexie only for a large or structured draft — per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/storage-backend-choice.md|storage-backend-choice]].
- Rehydration happens once, before the first render that reads the state — a synchronous metaReducer for a slice, a `withHooks({ onInit })` for a feature store, never a post-render patch — per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/rehydration-timing.md|rehydration-timing]].
- The `auth` slice's `accessToken` (and anything `solution-authentication`'s `token-storage-strategy` ADR marks sensitive) is **never** persisted — the slice holding the token is simply never given a persistence metaReducer.
- The persistence mechanism (`persistKeys`, `withPersistedDraft`, `SENSITIVE_STATE_KEYS`) lives in `libs/shared/state/src/lib/persistence/` and holds no slice-specific config — the `key` / `keys` are passed at each call site.

# Boundaries
- monolith VP8. `requires GlobalStore` (VP2) — the mechanism ships in `libs/shared/state`; the feature-tier variant reuses the same guard.
- Not offline sync — a persisted slice is a per-session convenience, not a durable write queue (`solution-offline-sync`). It has no replay, no conflict handling, no server round trip.
- Not the service-worker cache (`solution-offline-first`) — that is transport-level (which HTTP responses are available offline); this is state-level (which store values survive a reload).
- Adds no Nx project and no new `type:*` tag — one folder in `libs/shared/state`, one new slice, an opt-in feature-store feature.

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/storage-backend-choice.md|storage-backend-choice]] — `localStorage` default, `sessionStorage` for per-tab, IndexedDB (Dexie) only for large/structured drafts; no generic third-party sync library (the allow-list + sensitive-key guard are the point); no cookie. Rejected: IndexedDB for everything; `ngrx-store-localstorage`; a cookie.
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/rehydration-timing.md|rehydration-timing]] — synchronous merge inside a per-feature metaReducer on the store-init action for slices; `withHooks({ onInit })` for feature stores; never an effect on `ROOT_EFFECTS_INIT`, `APP_INITIALIZER`, or a component effect. Rejected: hydrate-action effect; `provideAppInitializer`; component `effect()`.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]
  - [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|libs/shared/state]] - hosts the `persistence/` folder and the `preferences` slice
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] *(when VP7 is also present)*
  - its [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/token-storage-strategy.md|token-storage-strategy]] ADR is the reason `accessToken` is on `SENSITIVE_STATE_KEYS` and the `auth` slice is never a persistence target

NPM:
- No new package for the default path. `dexie` (already present via `solution-offline-sync` / `solution-logging-global`) only if a large-draft variant is used.

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/Repository.extend.md|Repository]] - extend - the `persistence/` convention, the finite-allow-list rule, the `SENSITIVE_STATE_KEYS` invariant, the "`auth` slice is never persisted" rule

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/shared-state.project.extend.md|libs/shared/state]] - extend - add `persistence/` + the `preferences` slice; register `preferences` with `persistKeys(...)` in `store.config.ts`
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/{Feature}.project.extend.md|libs/{feature}/feature]] - extend - opt a dedicated `{Feature}DraftStore` into `withPersistedDraft()`

Artifact-level:
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/persisted-state.ts.create.md|persisted-state.ts]] - create - the `persistKeys()` metaReducer factory + `SENSITIVE_STATE_KEYS` + `assertPersistable()`
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/preferences.store.ts.create.md|preferences.store.ts]] - create - the `preferences` classical-NgRx slice (the reference persisted slice)
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/with-persisted-draft.ts.create.md|with-persisted-draft.ts]] - create - the `signalStoreFeature` for the feature tier

# Workflow

## Persisting a global slice

1. The slice is flat and all-scalar (a `Date` / `Map` / nested object does not round-trip through JSON).
2. Register it in `store.config.ts` with `provideState(fooFeature, { metaReducers: [persistKeys<FooState>({ key: 'app:foo', keys: [...] })] })`.
3. The `keys` array is the allow-list — list every persisted field literally.
4. `assertPersistable()` runs inside `persistKeys()` — if a key is on `SENSITIVE_STATE_KEYS`, the app throws on boot.

## Persisting a feature form draft

1. Add a dedicated `{Feature}DraftStore` — `signalStore({ providedIn: 'root' }, withState(empty), withPersistedDraft({ key: 'app:{feature}:draft', keys: [...] }))`.
2. Bind the form's Signal Form to the draft store; call `draftStore.clear()` on submit success.
3. Never add `withPersistedDraft` to the feature's main list/detail store.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/Repository.extend.md#MUST|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/shared-state.project.extend.md#MUST|shared-state.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/persisted-state.ts.create.md#MUST|persisted-state.ts]]
- Never persist a whole store or slice with `*` — always an explicit, finite key list.
  - Risk: a whole-slice persist captures fields never meant to survive a session and silently absorbs any field added later.
  - Fix: `keys: ['theme', 'density']` — a literal array; adding a persisted field is a deliberate edit.
- Never give the `auth` slice a persistence metaReducer, and never put a token key on any allow-list.
  - Risk: a persisted token is the XSS-driven theft vector `solution-authentication` closed off.
  - Fix: `assertPersistable()` throws on a `SENSITIVE_STATE_KEYS` entry; the `auth` slice's `provideState` stays metaReducer-free.
- Never rehydrate after the first render that reads the state.
  - Risk: a post-render merge flashes default state to the persisted value.
  - Fix: synchronous metaReducer for a slice, `withHooks({ onInit })` for a feature store.

## SHOULD
- Avoid persisting a slice when nothing actually needs to survive a reload — persist only state a user would be annoyed to lose across a session.
- Avoid a generic third-party `localStorage` sync library — it turns the mandatory allow-list and the sensitive-key guard back into configuration discipline.

# Check list
- [ ] Every persisted slice / draft store declares a finite `keys` allow-list, never `*`.
- [ ] No auth token / PII key appears on any allow-list; `assertPersistable()` covers `SENSITIVE_STATE_KEYS`.
- [ ] The `auth` slice's `provideState` has no `metaReducers`.
- [ ] Rehydration completes before the first render — metaReducer for slices, `onInit` for feature stores.
- [ ] The `persistence/` folder holds mechanism only; `key` / `keys` are passed at the call site.
