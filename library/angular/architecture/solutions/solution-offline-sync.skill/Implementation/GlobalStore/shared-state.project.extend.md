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
- The `notifications` slice is registered in the same `provideGlobalStore()` seam as `connectivity` — never a separate provider call.
  - Risk: a slice wired through its own provider is easy to omit in a test setup or a second app.
  - Fix: add `provideState(notificationsFeature)` to `store.config.ts`.
- `selectNotifications` is the only public selector feature/UI code consumes.
  - Risk: UI reading raw slice fields couples to the slice's shape and can render the list inconsistently.
  - Fix: `index.ts` exports `selectNotifications` + `NotificationsActions` only.
- The slice has no effects — a notification is pushed by whoever detects the event (here `ReplayOrchestrator.handleConflict`).
  - Risk: an effect in a pure UI-message slice re-introduces the Action→Effect→Action indirection this slice exists to avoid.
  - Fix: reducer + selectors only; callers dispatch `show(...)`.
- Feature code never constructs or stores conflict messages of its own.
  - Risk: parallel notification systems that show duplicate or inconsistent messages.
  - Fix: dispatch `NotificationsActions.show(...)` and render `selectNotifications`.

## SHOULD
- **Adding an effect to the `notifications` slice to "listen" for conflicts** — Consequence: couples the slice to the replay orchestrator and to Dexie — Instead: the orchestrator dispatches `show(...)`; the slice stays a pure list.

# Check list

- [ ] `notifications` reducer is registered in `provideGlobalStore()` next to `connectivity`
- [ ] `selectNotifications` is exported for feature/UI consumption
- [ ] The slice has no effects

# Unittest TestCases

- [ ] WHEN `NotificationsActions.show` is dispatched THEN `selectNotifications` includes the new entry with a generated id
- [ ] WHEN the root store is inspected after `provideGlobalStore()` THEN it carries both a `connectivity` and a `notifications` key
