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
- A Facade must explicitly opt an operation into queueing by catching `OfflineTransportError` and calling `MutationQueueService.enqueue` — queueing is never automatic or implicit for every method.
- A queued operation's return type must clearly distinguish "queued for later" from an immediate successful result (e.g. `{ queued: true }`), so the calling Signal Store can reflect a pending state rather than treating it as a completed success.

- Never enqueue an operation whose business validation (e.g. `OrdersValidationError`) already failed before the Client was ever called — only genuine `OfflineTransportError` failures are queueable.
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
