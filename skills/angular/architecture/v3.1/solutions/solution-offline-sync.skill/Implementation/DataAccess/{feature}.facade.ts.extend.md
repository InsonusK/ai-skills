---
description: Extend the generic Facade pattern from `solution-api-http-layer` to catch OfflineTransportError (from `solution-offline-first`) and enqueue queueable operations instead of failing outright
project_name: "{Feature}"
name: "{feature}"
element_kind: service
change_kind: extend
tags:
  - solution/offline-sync
  - element/feature-facade-ts
---

# How this generic file is used
This extends [[skills/angular/architecture/v3.1/solutions/solution-api-http-layer.skill/Implementation/DataAccess/{Feature}.project.create/{feature}.facade.ts.create]], applied to any feature's `{feature}.facade.ts`, for operations that Facade explicitly marks as queueable.

# Goals

- Let a mutation attempted while offline be queued for later sync instead of surfacing an immediate failure, for operations where that makes sense

# Implementation changes

```typescript
// orders.facade.ts — extended
export class OrdersFacade {
  constructor(
    private readonly client: OrdersClient,
    private readonly queue: MutationQueueService,
  ) {}

  async addOrder(input: AddOrderInput): Promise<Order | { queued: true }> {
    if (input.quantity <= 0) {
      throw new OrdersValidationError('quantity must be positive');
    }
    try {
      return await this.client.addOrder(input);
    } catch (error) {
      if (error instanceof OfflineTransportError) {
        await this.queue.enqueue({
          feature: 'orders',
          operationName: 'addOrder',
          payload: input,
          touchedFields: Object.keys(input),
        });
        return { queued: true };
      }
      throw error;
    }
  }
}
```

# Rule changes

## MUST
- A Facade explicitly opts an operation into queueing — `catch (OfflineTransportError)` → `MutationQueueService.enqueue`. Queueing is never implicit for every method.
  - Risk: auto-queueing replays one-time or time-sensitive actions later and produces a wrong result.
  - Fix: only the operations the Facade wraps are queueable; the rest rethrow `OfflineTransportError`.
- A queued operation's return type distinguishes "queued for later" from a completed result — `{ queued: true, idempotencyKey, optimistic }`.
  - Risk: the store treats a queued op as a success and shows no pending state; the user thinks it saved.
  - Fix: `isQueued(result)` branch in the store; it appends `optimistic` with `syncStatus: 'queued'`.
- Never enqueue an operation whose business validation already failed before the Client was called.
  - Risk: an invalid command sits in the queue and fails forever on replay.
  - Fix: validation runs first and throws its `ValidationError`; only a genuine `OfflineTransportError` is queueable.
## SHOULD
- **Enqueueing an operation whose business validation already failed** — Consequence: queues a command that will never succeed, wasting a replay attempt and confusing the user with a "pending" state for something that was actually invalid — Instead: business validation always runs and fails before any queueing decision is considered

# Check list

- [ ] Only operations explicitly marked queueable catch `OfflineTransportError` and enqueue
- [ ] The queued-vs-completed distinction is visible in the Facade's return type

# Unittest TestCases

- [ ] WHEN a queueable operation's Client call throws `OfflineTransportError` THEN
  - [ ] the Facade enqueues the operation and returns a queued indicator instead of throwing
- [ ] WHEN business validation fails before the Client is called THEN
  - [ ] the Facade throws its validation error and does not enqueue anything
