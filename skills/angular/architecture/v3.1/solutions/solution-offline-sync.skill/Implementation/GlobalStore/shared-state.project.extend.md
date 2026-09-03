---
description: Extend libs/shared/state with the notifications slice used by the replay orchestrator to surface server-wins conflicts
name: shared-state
project_kind: library
element_kind: project
change_kind: extend
tags:
  - solution/offline-sync
  - element/shared-state-project
---

# Goals

- Give the whole application one place for transient user notifications, and give `ReplayOrchestrator` somewhere to surface a server-wins conflict message

# Structure

## Project Structure

```
/libs/shared/state
  /src
    /lib
      notifications/
        notifications.actions.ts
        notifications.reducer.ts
        notifications.selectors.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| notifications/ | New classical-NgRx slice — a list of `{ id, message, detail? }` with `show` / `dismiss` / `clearAll`. Same shape as the existing `connectivity` slice (from `solution-offline-first`). |

# Implementation changes

Register the slice in `provideGlobalStore()` (from `solution-global-store`), alongside `connectivity`:

```typescript
// libs/shared/state/src/lib/store.config.ts
import { notificationsFeature } from './notifications/notifications.reducer';

export function provideGlobalStore(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideStore({}),
    provideState(connectivityFeature),   // solution-offline-first
    provideState(notificationsFeature),  // this solution
    provideEffects(ConnectivityEffects),
  ]);
}
```

# Rules

## MUST
- The `notifications` slice must be registered in the same `provideGlobalStore()` seam as the `connectivity` slice — never in a separate provider call.
- `selectNotifications` must be the only public selector consumed by feature/UI code.
- The slice must have no effects — a notification is pushed by whoever detects the event (here, `ReplayOrchestrator.handleConflict`), not by the slice itself.

- feature code must never construct or store conflict messages of its own — it dispatches `NotificationsActions.show(...)` and renders `selectNotifications`.

## SHOULD
- **Adding an effect to the `notifications` slice to "listen" for conflicts** — Consequence: couples the slice to the replay orchestrator and to Dexie — Instead: the orchestrator dispatches `show(...)`; the slice stays a pure list.

# Check list

- [ ] `notifications` reducer is registered in `provideGlobalStore()` next to `connectivity`
- [ ] `selectNotifications` is exported for feature/UI consumption
- [ ] The slice has no effects

# Unittest TestCases

- [ ] WHEN `NotificationsActions.show` is dispatched THEN `selectNotifications` includes the new entry with a generated id
- [ ] WHEN the root store is inspected after `provideGlobalStore()` THEN it carries both a `connectivity` and a `notifications` key
