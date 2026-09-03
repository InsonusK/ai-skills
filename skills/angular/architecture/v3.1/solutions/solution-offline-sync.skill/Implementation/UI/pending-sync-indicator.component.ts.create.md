---
description: Shared, presentational pending-sync indicator in libs/shared/ui — a `count` input, fed a value derived from the feature store's rows that carry a syncStatus
project_name: shared-ui
name: pending-sync-indicator
element_kind: component
change_kind: create
tags:
  - solution/offline-sync
  - element/pending-sync-indicator-component-ts
---

# Goals

- Show the user how many of their actions are queued and waiting to sync, per feature — as an aggregate on top of the per-row `syncStatus` badges the feature already shows

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------------- | -------------------- | --------- |
| Pending sync indicator | PendingSyncIndicatorComponent | PendingSyncIndicatorComponent | pending-sync-indicator.component.ts | pending-sync-indicator.component.ts |

# Implementation changes

**Presentational only** — it takes a `count` input and renders when `count > 0`. It never injects `MutationQueueService` and never touches Dexie, keeping `libs/shared/ui` free of a `type:store` dependency (the same call the plateau made for `OfflineBannerComponent`).

```typescript
@Component({
  selector: 'ui-pending-sync-indicator',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (count() > 0) {
      <div role="status">
        {{ count() }} {{ count() === 1 ? 'action' : 'actions' }} waiting to sync
      </div>
    }
  `,
})
export class PendingSyncIndicatorComponent {
  readonly count = input.required<number>();
}
```

The owning feature feeds `count` from a **computed derived from its store's rows**, not a separately tracked number:

```typescript
// in {feature}.store.ts
pendingSyncCount: computed(() =>
  orders().filter((o) => o.syncStatus === 'queued' || o.syncStatus === 'sending' || o.syncStatus === 'failed').length),
// template:  <ui-pending-sync-indicator [count]="store.pendingSyncCount()" />
```

# Rule changes

## MUST
- The component renders from its `count` input only — never injects `MutationQueueService`, never queries Dexie.
  - Risk: injecting the queue adds a `type:ui → type:store` boundary dependency and duplicates the reactive read.
  - Fix: `readonly count = input.required<number>()`; the feature feeds it.
- The `count` the feature feeds it is **derived from the store's rows that carry a `syncStatus`** — not a number tracked in parallel.
  - Risk: a separately maintained `pendingSync` counter drifts from the visible rows (e.g. after a partial `hydratePending`).
  - Fix: `pendingSyncCount = computed(() => rows().filter(r => r.syncStatus === 'queued' || 'sending' || 'failed').length)`.
- A feature that queues mutations surfaces a **per-row `syncStatus`** (via `<ui-status-badge>`), not just this aggregate.
  - Risk: a lone "3 pending" tells the user something is syncing but not which item — indistinguishable from data loss after a reload.
  - Fix: each row renders its own status badge; the indicator is the sum.

## SHOULD
- **A feature queueing mutations without ever showing pending state** — Consequence: the user has no way to know their action wasn't immediately applied, which can look like data loss or a bug — Instead: every feature that queues mutations shows both the per-row status and this aggregate indicator

# Check list

- [ ] The component takes only a `count` input — no `MutationQueueService`, no Dexie reference
- [ ] The `count` a feature feeds it is a computed over the store's rows with a `syncStatus`
- [ ] Every feature that enqueues mutations shows a per-row `syncStatus` and this indicator

# Unittest TestCases

- [ ] WHEN `count` is `0` THEN nothing is rendered
- [ ] WHEN `count` is `1` THEN a `role="status"` element reads "1 action waiting to sync"; `> 1` pluralises
- [ ] WHEN a mutation is enqueued for a feature THEN the derived count increases by one, reactively (the store row gains `syncStatus: 'queued'`)
- [ ] WHEN a queued mutation is successfully synced THEN the derived count decreases accordingly (the optimistic row is dropped)
