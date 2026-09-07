---
description: How a feature lib opts one of its own Signal Stores into withPersistedDraft() so an in-progress form survives a browser reload — a dedicated draft store, an explicit allow-list, cleared on successful submit
project_name: "{Feature}"
name: "{feature}"
project_kind: library
element_kind: project
change_kind: extend
tags:
  - solution/persisted-state
  - element/feature-feature-project
---

# How this generic file is used

Applies to any `libs/{feature}/feature` that has a form worth preserving across a reload — a multi-field create form, a wizard step. The feature adds a **dedicated draft store**; it does not persist its main list/detail store.

# Goals

- Let a user who reloads mid-entry come back to their half-filled form
- Keep the persistence opt-in narrow: a separate `{Feature}DraftStore`, an explicit field allow-list, cleared on submit

# Structure

## Project Structure

```
/libs/{feature}/feature
  /src/lib
    {feature}-draft.store.ts      <- new: signalStore + withPersistedDraft, draft fields only
    {feature}/{feature}-form/
      {feature}-form.component.ts  <- binds the Signal Form to the draft store; clears it on submit success
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| `{feature}-draft.store.ts` | A `signalStore({ providedIn: 'root' })` holding only the form's editable fields, composed with `withPersistedDraft({ key: 'app:{feature}:draft', keys: [...] })`. |

# Implementation changes

```typescript
// libs/{feature}/feature/src/lib/{feature}-draft.store.ts
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { withPersistedDraft } from '@org/shared-state';

interface OrderDraftState { product: string; quantity: number; notes: string; }
const empty: OrderDraftState = { product: '', quantity: 1, notes: '' };

export const OrderDraftStore = signalStore(
  { providedIn: 'root' },
  withState(empty),
  withPersistedDraft<OrderDraftState>({ key: 'app:orders:draft', keys: ['product', 'quantity', 'notes'] }),
  withMethods((store) => ({
    patch(partial: Partial<OrderDraftState>) { patchState(store, partial); },
    clear() { patchState(store, empty); },
  })),
);
```

The form component reads the draft store for its initial values and calls `clear()` after a successful submit (which also clears storage, via the persist effect writing the emptied state).

# Rules

## MUST
- The persisted draft lives in a **separate** `{Feature}DraftStore`, never on the feature's main list/detail Signal Store.
  - Risk: adding `withPersistedDraft` to the main store risks persisting loaded entities or a `submitting` flag, and couples list state to storage.
  - Fix: a dedicated store holding only editable fields; the main store is unchanged.
- The draft is cleared on successful submit.
  - Risk: a stale draft reappears the next time the user opens the form for a *new* record.
  - Fix: `draftStore.clear()` in the submit-success path; the persist effect then writes the emptied state.
- The allow-list is exactly the form's editable fields — nothing derived, nothing server-assigned.
  - Risk: persisting a computed total or a generated id makes the restored draft inconsistent.
  - Fix: `keys` lists the raw input fields only.

## SHOULD
- **Persisting the draft to `sessionStorage` when the user genuinely expects it across a full browser restart** — Consequence: `sessionStorage` is cleared on tab close, so the draft is gone — Instead: use the default `localStorage` for a draft meant to outlive the session; reserve `sessionStorage` for a per-tab wizard

# Check list

- [ ] The draft store is separate from the feature's main store
- [ ] `keys` lists only editable form fields
- [ ] The form clears the draft on submit success
- [ ] The `key` is namespaced (`app:{feature}:draft`)

# Unittest TestCases

- [ ] WHEN the form is filled, the page reloads, and the form re-renders THEN
  - [ ] the fields show the previously entered values
- [ ] WHEN the form is submitted successfully THEN
  - [ ] `OrderDraftStore` is back to `empty` and `localStorage` reflects that
