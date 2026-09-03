---
name: plateau-offline-full-monolith--class-pending-sync-indicator-component
description: The shared, presentational "N actions waiting to sync" indicator in libs/shared/ui — takes a count input, rendered only when count > 0, mounted per feature — offline-full-monolith plateau
domain: skill
type: template
plateau: offline-full-monolith
artifact_type: component
version: 20260903120000
tags:
  - skill/template/class
  - plateau/offline-full-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]]"

> `libs/shared/ui/src/lib/pending-sync-indicator/`. Presentational: it takes `count` as an input; the owning feature feeds it from `MutationQueueService.pendingForFeature$(...)` (via its feature Signal Store). This keeps `libs/shared/ui` free of a `type:store` dependency — same call the plateau made for `OfflineBannerComponent`.

# Goal

- Show the user how many of their actions are queued and waiting to sync, per feature

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create.md|UI/pending-sync-indicator.component.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Presentational only — no store, no `MutationQueueService`, no Dexie; it renders from its `count` input
- `OnPush`; nothing rendered while `count` is `0`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create.md|UI/pending-sync-indicator.component.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Pending-sync indicator | `PendingSyncIndicatorComponent` | `PendingSyncIndicatorComponent` | `pending-sync-indicator.component.ts` | `pending-sync-indicator.component.ts` |
| Selector | `ui-{name}` | `ui-pending-sync-indicator` | — | — |

# Implementation

```typescript
// Skill: class-pending-sync-indicator-component
// Plateau: offline-full-monolith
// Version: 20260903120000

@Component({
  selector: 'ui-pending-sync-indicator',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (count() > 0) {
      <div role="status" class="pending-sync">
        {{ count() }} {{ count() === 1 ? 'action' : 'actions' }} waiting to sync
      </div>
    }
  `,
})
export class PendingSyncIndicatorComponent {
  readonly count = input.required<number>();
}
```

```typescript
// the owning feature's Signal Store keeps `pendingSync` in step with the queue:
trackPendingSync: rxMethod<void>(pipe(
  switchMap(() => queue.pendingForFeature$('orders')),
  tap((entries) => patchState(store, { pendingSync: entries.length })),
)),
// template:  <ui-pending-sync-indicator [count]="store.pendingSync()" />
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create.md|UI/pending-sync-indicator.component.ts.create]]

# Rules

## MUST
- The component must render from its `count` input only — never inject `MutationQueueService`, never touch the Dexie table.
- A feature that enqueues mutations must surface this indicator (or `pendingForFeature$` some other way) somewhere in its UI — a queued action must never be invisible.
- The message container must carry `role="status"`.
- Never apply several plateau templates per class/artifact.
- Never let a feature build its own pending-sync indicator — reuse this component.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create.md|UI/pending-sync-indicator.component.ts.create]]

# Check list

- [ ] Nothing rendered while `count()` is `0`
- [ ] No `MutationQueueService` / Dexie reference — only the `count` input
- [ ] Singular / plural wording is correct

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create.md|UI/pending-sync-indicator.component.ts.create]]

# Unittest TestCases

- [ ] WHEN `count` is `0` THEN nothing is rendered
- [ ] WHEN `count` is `1` THEN a `role="status"` element reads "1 action waiting to sync"
- [ ] WHEN `count` is `> 1` THEN the message is pluralised

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/solution-offline-sync.skill.md|solution-offline-sync]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create.md|UI/pending-sync-indicator.component.ts.create]]
