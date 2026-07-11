---
name: class-pending-sync-indicator-component
description: Shared pending-sync indicator, reactively reading MutationQueueService's live query to show how many actions are queued for a feature
domain: skill
type: template
plateau: platform
artifact_type: component
version: 20260711150000
tags:
  - skill/template/class
  - plateau/platform
created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]]"
---

# Goal

- Show the user how many of their actions are queued and waiting to sync, per feature

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Reads exclusively through `MutationQueueService.pendingForFeature$`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------------- | -------------------- | --------- |
| Pending sync indicator | `PendingSyncIndicatorComponent` | `PendingSyncIndicatorComponent` | `pending-sync-indicator.component.ts` | `pending-sync-indicator.component.ts` |

# Implementation

```typescript
// Skill: class-pending-sync-indicator-component
// Plateau: platform
// Version: 20260711150000

@Component({
  selector: 'app-pending-sync-indicator',
  template: `
    @if (pendingCount() > 0) {
      <div role="status">{{ pendingCount() }} action(s) waiting to sync</div>
    }
  `,
})
export class PendingSyncIndicatorComponent {
  @Input({ required: true }) feature!: string;

  private readonly queue = inject(MutationQueueService);
  protected readonly pendingCount = toSignal(
    toObservable(computed(() => this.feature)).pipe(
      switchMap(feature => from(this.queue.pendingForFeature$(feature))),
      map(entries => entries.length),
    ),
    { initialValue: 0 },
  );
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]

# Rules

## MUST
- The indicator MUST read from `MutationQueueService.pendingForFeature$`.
- A feature that queues mutations MUST mount this indicator somewhere in its UI.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **A feature queueing mutations without ever showing a pending indicator**
  - Consequence: the user has no way to know their action wasn't immediately applied
  - Instead: every feature that queues mutations surfaces this indicator somewhere relevant

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]

# Check list

- [ ] The indicator's count reactively updates as entries are enqueued/synced
- [ ] Every feature that enqueues mutations surfaces this indicator somewhere in its UI

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]

# Unittest TestCases

- [ ] WHEN a mutation is enqueued for a feature THEN
  - [ ] that feature's indicator count increases by one, reactively
- [ ] WHEN a queued mutation is successfully synced THEN
  - [ ] the indicator count decreases accordingly

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-sync.skill/solution-offline-sync.skill|solution-offline-sync]] - [[skills/angular/architecture/solutions/solution-offline-sync.skill/Implementation/UI/pending-sync-indicator.component.ts.create|UI/pending-sync-indicator.component.ts.create]]
