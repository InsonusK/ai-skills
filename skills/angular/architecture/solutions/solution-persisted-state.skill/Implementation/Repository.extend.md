---
description: Extend the base workspace with the persisted-state convention — a persistence/ folder in libs/shared/state, an explicit per-slice key allow-list, a central sensitive-key registry, and a lint/test invariant that no token key is ever persisted
element_kind: repository
change_kind: extend
tags:
  - solution/persisted-state
  - element/monolith-repository
---

# Structure

No new top-level directories and no new Nx project. This extension adds one folder inside the existing `libs/shared/state` and a workspace-level invariant on top of [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]] and [[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/Implementation/GlobalStore/shared-state.project.create.md|the global store]].

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /libs/shared/state/src/lib/persistence | New folder: `persisted-state.ts` (the `persistKeys()` metaReducer factory + `SENSITIVE_STATE_KEYS` registry + `assertPersistable()` guard) and `with-persisted-draft.ts` (the `signalStoreFeature` for the feature tier). Per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/persisted-state.ts.create.md|persisted-state.ts]] and [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/with-persisted-draft.ts.create.md|with-persisted-draft.ts]]. |
| /libs/shared/state/src/lib/preferences | New classical-NgRx slice — the first persisted slice (theme, list density, last-opened feature tab). Per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/GlobalStore/preferences.store.ts.create.md|preferences.store.ts]]. |
| /libs/{feature}/feature | A feature may opt its own Signal Store into `withPersistedDraft()` for an in-progress form draft. Per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/{Feature}.project.extend.md|{Feature}.project.extend]]. |

# Rules

## MUST
- Persistence is enabled per slice / per feature store, declaring an explicit list of keys — never `*`, never "the whole slice".
  - Risk: a whole-slice persist captures fields that were never meant to survive a session (a `loading` flag, an error string, a server id) and silently grows to include any field added later.
  - Fix: `persistKeys({ key, keys: ['theme', 'density'], storage })` — the `keys` array is the allow-list; the factory persists and rehydrates only those.
- No field that an owning solution marks sensitive is ever persisted — the access token above all, plus anything on the central `SENSITIVE_STATE_KEYS` registry.
  - Risk: a persisted token reintroduces the XSS-driven theft vector [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/adr/token-storage-strategy.md|token-storage-strategy]] closed, made worse by federated third-party code sharing the runtime.
  - Fix: `assertPersistable()` runs inside `persistKeys()` at construction and throws if any allow-listed key is on `SENSITIVE_STATE_KEYS`; a unit test asserts it throws for `accessToken`.
- The `auth` slice is never given a persistence metaReducer.
  - Risk: even an allow-list that omits `accessToken` today drifts; the safest rule is that the slice holding the token is simply never a persistence target.
  - Fix: `provideState(authFeature)` carries no `metaReducers`; reload recovery stays silent-refresh-on-bootstrap.
- Rehydration completes before the first render that reads the state — a synchronous metaReducer for a slice, a `withHooks({ onInit })` for a feature store, never a post-render patch.
  - Risk: merging persisted state after render produces a visible flash — the theme toggles a frame in, a draft form blanks then fills.
  - Fix: per [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/rehydration-timing.md|rehydration-timing]] — `localStorage` is read synchronously in the metaReducer's handling of the store-init action.

# Unittest TestCases

- [ ] WHEN the codebase is searched for a persistence metaReducer on the `auth` feature THEN
  - [ ] none is found
- [ ] WHEN `persistKeys()` is constructed with `keys` containing `accessToken` (or any `SENSITIVE_STATE_KEYS` entry) THEN
  - [ ] it throws at construction, failing the test suite
- [ ] WHEN every `persistKeys(...)` / `withPersistedDraft(...)` call site is inspected THEN
  - [ ] each passes a finite `keys` array, never `'*'` or a spread of all state keys

## SHOULD
- **Reaching for a generic third-party `localStorage` sync metaReducer instead of the `persistKeys()` factory** — Consequence: loses the two invariants this solution exists to enforce — the mandatory finite allow-list and the `SENSITIVE_STATE_KEYS` guard become configuration discipline again — Instead: use `persistKeys()`; extend it if a real gap appears
- **Persisting a slice "to be safe" when nothing actually needs to survive a reload** — Consequence: storage fills with state that has no reason to persist, and a future sensitive field added to that slice is now one code review away from leaking — Instead: persist only state a user would be annoyed to lose across a session (a preference, an in-progress draft)
