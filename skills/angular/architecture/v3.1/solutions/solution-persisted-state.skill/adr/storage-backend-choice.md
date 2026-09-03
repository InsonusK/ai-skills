---
name: storage-backend-choice
description: Which browser storage a persisted slice / feature-store draft is written to — localStorage as the default, sessionStorage for per-tab, IndexedDB (Dexie) only for large or structured drafts
problem: solution-persisted-state syncs selected NgRx state across a browser session. The browser offers several storage backends (localStorage, sessionStorage, IndexedDB, cookies) with different size limits, sync/async access, and lifetimes. One default must be chosen, with a clear rule for when to deviate.
decision: localStorage is the default backend — synchronous, ~5 MB, survives a tab close. sessionStorage is used only when the state must not outlive the tab. IndexedDB via Dexie is used only for a draft too large or too structured for a JSON string in localStorage. A generic third-party sync library is not adopted — the allow-list and the sensitive-key guard are the point.
tags:
  - solution/persisted-state
  - stack/typescript
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`solution-persisted-state` writes a chosen piece of NgRx state to browser storage so it survives a session — a theme choice, a list density, a half-filled form. The browser offers four candidate backends and they differ on the axes that matter here:

- **Size** — `localStorage` / `sessionStorage` cap at roughly 5 MB per origin; IndexedDB is effectively unbounded; a cookie is ~4 KB and rides every request.
- **Access model** — `localStorage` / `sessionStorage` are synchronous, so state can be merged *inside* a reducer at store-init time; IndexedDB is asynchronous and cannot.
- **Lifetime** — `sessionStorage` dies with the tab; `localStorage` and IndexedDB persist until cleared; a cookie has its own expiry.

A single default has to be picked, with an explicit rule for the exceptions, or every consumer re-litigates it per slice.

# Selected variant

**Selected variant:** [[#localStorage default, sessionStorage for per-tab, IndexedDB only for large or structured drafts (selected)]]

# Searched variants

## localStorage default, sessionStorage for per-tab, IndexedDB only for large or structured drafts (selected)

### Description

The `persistKeys()` metaReducer factory takes a `storage: Storage` argument and defaults to `localStorage`. A consumer passes `sessionStorage` when the state must not outlive the tab (a wizard's in-progress step). For a draft that does not fit a JSON string comfortably — a rich-text body, an array of attachments, anything past a few tens of KB — the feature uses `withPersistedDraft()` backed by a Dexie table instead, hydrated in an `onInit` hook rather than a reducer.

### Benefits

- `localStorage`'s synchronous read lets the metaReducer merge persisted values into the initial state *before the first render that reads it* — no flash of default state, no post-render patch (see [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/adr/rehydration-timing.md|rehydration-timing]]).
- One backend covers the overwhelmingly common case (a handful of scalar preferences), so most consumers never make a decision.
- `sessionStorage` is the same API with a different lifetime — no new code path, just a different argument.
- Dexie is already a workspace dependency (`solution-offline-sync`'s mutation queue, `solution-logging-global`'s retry queue), so the large-draft path adds no new package.
- The 5 MB cap is a useful forcing function: if a slice does not fit, that is a signal it should not be a blob in `localStorage`.

### Costs

- Two code paths for persistence (synchronous metaReducer vs. async `withPersistedDraft` + Dexie). Accepted: they map cleanly onto "slice of scalars" vs. "structured draft", and the split is what keeps the common path a one-liner.
- `localStorage` is shared across all tabs of the origin, so a write in one tab is visible to another only after its next store-init (no live cross-tab sync). Accepted: cross-tab state sync is out of scope for a per-session convenience.

## IndexedDB for everything

### Description

Every persisted slice and draft goes through a single Dexie-backed store; hydration is always asynchronous, gated behind a `provideAppInitializer`.

### Benefits

- One code path, one backend, no size ceiling.
- Structured data stored without a `JSON.stringify` round trip.

### Costs

- Asynchronous hydration means either the app blocks on `provideAppInitializer` until IndexedDB resolves (a measurable startup delay for what is often two booleans) or the first render shows default state and then patches — a flash. Both are worse than a synchronous `localStorage` read for the common case.
- Heavyweight for storing `{ theme: 'dark' }` — a full IndexedDB schema, versioning, and an async open for a value that fits in 12 bytes.

## A generic third-party sync library (e.g. `ngrx-store-localstorage`)

### Description

Adopt an off-the-shelf metaReducer that syncs configured slices to `localStorage`.

### Benefits

- No factory to write or maintain.
- Well-trodden configuration surface.

### Costs

- The two things this solution most needs — an **explicit per-slice key allow-list** (never `*`, never "the whole slice") and a **build/test-time guard that the auth token key can never appear on one** — are exactly what a generic library leaves to configuration discipline. Writing the ~40-line factory buys the guard as a first-class, tested invariant instead of a convention.
- Another dependency to track against the Angular/NgRx major cadence for a small, stable piece of logic.

## A cookie

### Description

Persist the state as a serialized cookie.

### Benefits

- Available to the server on the first request (useful for SSR theming).

### Costs

- ~4 KB ceiling; rides every HTTP request, wasting bandwidth on state the server does not need; awkward serialization; `SameSite` / `Secure` / expiry semantics irrelevant to client-only state. This catalog's apps are client-rendered SPAs — the one cookie benefit does not apply.
