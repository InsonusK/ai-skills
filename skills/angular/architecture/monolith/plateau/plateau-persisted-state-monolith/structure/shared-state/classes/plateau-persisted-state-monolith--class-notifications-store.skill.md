---
name: plateau-persisted-state-monolith--class-notifications-store
description: The notifications slice in libs/shared/state — a classical NgRx feature holding a list of user notifications (show / dismiss / clearAll), used by offline-sync to surface server-wins conflicts — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when editing the notifications slice (VP5) — show / dismiss / clearAll, selectNotifications
plateau: persisted-state-monolith
artifact_type: store
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"

> `libs/shared/state/src/lib/notifications/`. Registered via `provideGlobalStore()` alongside `connectivity`. Same classical-NgRx shape as `connectivity` and the future `auth` slice.

# Goal

- Give the app one global place for transient user notifications, and offline-sync a place to surface a server-wins conflict message

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Pure slice: no effects — it holds UI state only; the id is generated in the reducer
- Only `selectNotifications` is exported for feature/UI consumption

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Action group | `{Slice}Actions` | `NotificationsActions` | `{slice}.actions.ts` | `notifications.actions.ts` |
| Feature + reducer | `{slice}Feature` / `{slice}Reducer` | `notificationsFeature` | `{slice}.reducer.ts` | `notifications.reducer.ts` |
| Public selector | `select{X}` | `selectNotifications` | `{slice}.selectors.ts` | `notifications.selectors.ts` |

# Implementation

```typescript
// Skill: class-notifications-store
// Plateau: persisted-state-monolith
// Version: 20260903120000

export const NotificationsActions = createActionGroup({
  source: 'Notifications',
  events: {
    Show: props<{ message: string; detail?: Record<string, unknown> }>(),
    Dismiss: props<{ id: string }>(),
    'Clear All': emptyProps(),
  },
});

export const notificationsFeature = createFeature({
  name: 'notifications',
  reducer: createReducer(
    { items: [] as readonly Notification[] },
    on(NotificationsActions.show, (s, { message, detail }) => ({
      items: [...s.items, { id: crypto.randomUUID(), message, detail }],
    })),
    on(NotificationsActions.dismiss, (s, { id }) => ({ items: s.items.filter((n) => n.id !== id) })),
    on(NotificationsActions.clearAll, () => ({ items: [] })),
  ),
});
export const { reducer: notificationsReducer, selectItems: selectNotifications } = notificationsFeature;
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/OfflineSync/replay-orchestrator.ts.create.md|OfflineSync/replay-orchestrator.ts.create]]

# Rules

## MUST
- The slice must be registered in `provideGlobalStore()` alongside `connectivity`.
- Only `selectNotifications` is exported for consumption; the raw feature selectors stay internal.
- Never apply several plateau templates per class/artifact.
- Never add effects here — a notification is pushed by whoever detects the event (e.g. `ReplayOrchestrator`).

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/GlobalStore/shared-state.project.extend.md|GlobalStore/shared-state.project.extend]]

# Check list

- [ ] `show` appends with a generated id; `dismiss` removes only that id; `clearAll` empties
- [ ] Registered in `provideGlobalStore()`
- [ ] `index.ts` exports `selectNotifications` + the actions, not the reducer directly

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/GlobalStore/shared-state.project.extend.md|GlobalStore/shared-state.project.extend]]

# Unittest TestCases

- [ ] WHEN `show` is dispatched THEN a notification with a generated id is appended
- [ ] WHEN `dismiss` is dispatched THEN only that notification is removed
- [ ] WHEN `clearAll` is dispatched THEN the slice is empty
- [ ] WHEN `selectNotifications` reads state THEN it returns the items list

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/GlobalStore/shared-state.project.extend.md|GlobalStore/shared-state.project.extend]]
