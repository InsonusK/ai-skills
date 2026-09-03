---
name: plateau-persisted-state-monolith--class-feature-draft-store
description: Generic pattern for a feature's {Feature}DraftStore — a signalStore composed with withPersistedDraft() holding only a form's editable fields, so an in-progress form survives a browser reload; cleared on submit success (VP8) — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when a feature adds a persisted form draft — creating {feature}-draft.store.ts, or wiring it into the form component (VP8)
plateau: persisted-state-monolith
artifact_type: store
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]]"

> `libs/{feature}/feature/src/lib/{feature}-draft.store.ts`. A dedicated store — never `withPersistedDraft` on the feature's main list/detail store.

# Goal

- Let a user who reloads mid-entry come back to their half-filled form
- Keep the persistence opt-in narrow: a separate store, an explicit field allow-list, cleared on submit

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The store holds only the form's editable fields — nothing derived, nothing server-assigned, no `submitting`/`error` flag
- `key` is namespaced `app:{feature}:draft`; the allow-list is exactly those fields
- Cleared on submit success — the persist effect then writes the emptied state

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Feature draft store | `{Feature}DraftStore` | `OrderDraftStore` | `{feature}-draft.store.ts` | `orders-draft.store.ts` |

# Implementation

```typescript
// Skill: class-feature-draft-store
// Plateau: persisted-state-monolith
// Version: 20260903190000
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { withPersistedDraft } from '@org/shared-state';

export interface OrderDraftState {
  product: string;
  quantity: number | null;
}
const empty: OrderDraftState = { product: '', quantity: null };

export const OrderDraftStore = signalStore(
  { providedIn: 'root' },
  withState(empty),
  withPersistedDraft<OrderDraftState>({ key: 'app:orders:draft', keys: ['product', 'quantity'] }),
  withMethods((store) => ({
    patch(partial: Partial<OrderDraftState>): void {
      patchState(store, partial);
    },
    clear(): void {
      patchState(store, empty);
    },
  })),
);
```

The form component binds `[ngModel]` / `(ngModelChange)` to the draft store's signals and calls `draft.clear()` after a successful submit.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]

# Rules

## MUST
- The draft store is separate from the feature's main list/detail Signal Store.
- `keys` lists only editable form fields — nothing derived or server-assigned.
- The form clears the draft on submit success.
- `key` is namespaced `app:{feature}:draft`.
- Never apply several plateau templates per class/artifact.
- Never add `withPersistedDraft` to the feature's main store — persisted server data goes stale with no invalidation.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]

# Check list

- [ ] The draft store is separate from the feature's main store
- [ ] `keys` lists only editable form fields
- [ ] The form clears the draft on submit success
- [ ] The `key` is namespaced

# Unittest TestCases

- [ ] WHEN storage holds `{ "product": "Widget", "quantity": 3 }` THEN the store rehydrates those on construction
- [ ] WHEN `patch({ product: 'Gadget' })` runs and the effect flushes THEN storage reflects `"product":"Gadget"`
- [ ] WHEN `clear()` runs THEN the store and storage are back to `empty`
- [ ] WHEN the form is filled, the page reloads, and the form re-renders THEN the fields show the previously entered values
- [ ] WHEN the form is submitted successfully THEN the draft store is `empty`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/solution-persisted-state.skill.md|solution-persisted-state]] - [[skills/angular/architecture/v3.1/solutions/solution-persisted-state.skill/Implementation/FeatureStore/{Feature}.project.extend.md|FeatureStore/{Feature}.project.extend]]
