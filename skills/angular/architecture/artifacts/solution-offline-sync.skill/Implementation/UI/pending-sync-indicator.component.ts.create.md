---
description: Shared pending-sync indicator, reactively reading MutationQueueService's live query
project_name: shared-ui
name: pending-sync-indicator
artifact_type: component
change_kind: create
---

# Goals

- Show the user how many of their actions are queued and waiting to sync, per feature

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------------------- | -------------------- | --------- |
| Pending sync indicator | PendingSyncIndicatorComponent | PendingSyncIndicatorComponent | pending-sync-indicator.component.ts | pending-sync-indicator.component.ts |

# Implementation changes

```code example
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

# Rule changes

## MUST
- The indicator MUST read from `MutationQueueService.pendingForFeature$`, never poll or query the Dexie table directly outside that service.
- A feature that queues mutations MUST mount this indicator (or otherwise surface `pendingForFeature$`) somewhere in its UI — a queued action MUST NOT be invisible to the user.

# Anti-patterns

- **A feature queueing mutations without ever showing a pending indicator**
  - Consequence: the user has no way to know their action wasn't immediately applied, which can look like data loss or a bug
  - Instead: every feature that queues mutations surfaces this indicator (or an equivalent) somewhere relevant

# Check list

- [ ] The indicator's count reactively updates as entries are enqueued/synced
- [ ] Every feature that enqueues mutations surfaces this indicator somewhere in its UI

# Unittest TestCases

- [ ] WHEN a mutation is enqueued for a feature THEN
  - [ ] that feature's indicator count increases by one, reactively
- [ ] WHEN a queued mutation is successfully synced THEN
  - [ ] the indicator count decreases accordingly
