---
name: solution-taskbox
description: Stack-agnostic base of TaskBox (VP-C003) — deferred, retried, ordered task execution stored by the common TaskBox contract; fixes how a service chooses the store, enqueues atomically with its data, writes handlers, and proves conformance with one shared feature file
whenToUse: when a backend web-service must run work later, retry it until it succeeds, or keep it in order per key — and in particular when that work must be enqueued atomically with a data change — or when reviewing a stack's TaskBox realization for conformance to the VP-C003 contract
domain: skill
type: architecture
version: 20260928000000
updated: 20260928
tags:
  - skill/architecture/solution
  - solution/taskbox
  - stack
  - concern/architecture
  - concern/testing
  - concern/testing/bdd
  - taskbox
creates:
  - "taskbox-conformance.feature"
extends:
depends_on:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
built_on_plateau:
adr:
  - "[[skills/common-workflow/architecture/solutions/solution-taskbox.skill/adr/one-conformance-feature-for-every-stack.md|One conformance feature for every stack]]"
  - "[[skills/common-workflow/architecture/solutions/solution-taskbox.skill/adr/enqueue-through-the-data-port.md|Enqueue through the data port that owns the transaction]]"
---

# Goal
- A service whose deferred work is stored as TaskBox tasks per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox.contract|the TaskBox contract]], in the store its criticality allows.
- Data ports that take the tasks a data change triggers, so data and task commit together.
- One task handler per task type, as an inbound adapter that returns an HTTP status code.
- The stack realization's run of [[./Implementation/features/taskbox-conformance.feature.create.md|taskbox-conformance.feature]], green once per supported store.

# Capabilities
- Work that must not be lost (a follow-up call, a notification, a re-check) survives crashes and restarts, because it is written in the same transaction as the change that caused it.
- Failures are retried by one classification shared with outbound calls; a failure that retrying cannot fix stops early instead of burning attempts.
- Per-key order (like a Kafka key) without a broker: tasks of one `queue_group` run one at a time, in commit order.
- A stack switch keeps the service's tables, streams, and pending tasks — every stack stores tasks the same way.

# Core Principles
- **Contract first** - The contract owns the task record, ordering, lifecycle, retention, and every store's schema; this solution adds only how a service uses them, and a stack realization adds only the client and its code.
- **Criticality picks the store** - A Critical task type lives in the VP-C001 store; a NonCritical one may live in either store, per the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox|VP-C003 concept]].
- **Same store, same transaction** - A task that follows a data change is enqueued by the adapter that writes that change, in the same transaction; the domain decides *that* a task is needed, never *how* it is stored.
- **Handlers are inbound adapters** - A task arrives like a request: the handler decodes the payload, calls the domain service, and maps its result to a status code — no business logic in the handler, exactly as for HTTP or gRPC.
- **Everything runs at least once** - Every handler is idempotent; the task `id` is the idempotency key it passes on.

# Boundaries
- Assumes the service already has a VP-C001 or VP-C002 store and applies its schema through its own migration tool — this solution creates neither.
- Assumes a person or an admin tool calls requeue / cancel for dead tasks; this solution exposes the operations, not a user interface for them.

# Adr
- [[./adr/one-conformance-feature-for-every-stack.md|One conformance feature for every stack]]
  - Selected variant: contract §8 as one Gherkin feature, copied verbatim into every stack and run once per store
- [[./adr/enqueue-through-the-data-port.md|Enqueue through the data port that owns the transaction]]
  - Selected variant: the data port's write method takes the tasks as plain domain values; the adapter enqueues them inside its own transaction

# Requirements
SOLUTION:
- [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - the conformance feature runs inside its `make unit-test` gate and appears in its scenario report
CONTRACT:
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox.contract|TaskBox storage contract]]
  - §1 task record, §2 ports, §3 ordering, §4 lifecycle, §5 retention, §6 per-store schema, §7 migrations, §8 conformance scenarios
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c004-httpoutbound/vp-c004-httpoutbound|VP-C004 HttpOutbound]]
  - the retry classification a handler's status code is judged by

# Template Skill Mutations
FILES:
- [[./Implementation/features/taskbox-conformance.feature.create.md|taskbox-conformance.feature]] - create - the contract's §8 as one Gherkin feature and its step vocabulary, run by every stack realization

# Workflow

## Enqueue with a data change (happy path)
1. The domain service decides that a change needs follow-up work and builds a task value: type, payload, group, `run_at`.
2. It calls the data port's write method with the change and the task values.
3. The store adapter opens one transaction, writes the change, enqueues each task through TaskBox's `enqueue(tx, …)`, and commits.
4. A worker claims the task when it is due and heads its group, and dispatches it to the handler registered for its type.
5. The handler calls the domain service and returns `2xx`; the task becomes `done` and is removed after its retention.

## A failing dependency
1. The handler's domain call fails because a dependency is unavailable; the handler returns `503` (or `504`, `429`, …) and optionally a `Retry-After`.
2. TaskBox reschedules the task after `max(backoff, Retry-After)`; its group waits.
3. After `max_attempts` retryable outcomes the task is `dead` and its group stops; a person requeues or cancels it.

## Invalid work
1. The handler finds the payload or the domain state invalid and returns `400`/`404`/`409`/`422`.
2. The task is `dead` at once — retrying cannot change the answer — and its group stops until a person resolves it.

# Rule

## MUST

### Realize the contract, never a copy of it
Implement the store structures, claim, ordering, lifecycle, and retention exactly as [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox.contract|the contract]] states them, and raise any needed deviation as a change to the contract.
- Violation: a stack adds a column, renames a status, or uses a job-queue library with its own tables.
- Risk: a service switched to another stack finds tables and pending tasks the new stack cannot read.
- Fix: propose the change to the contract as a new schema version; implement it in every stack.

### Pick the store by criticality
Store a Critical task type only in the VP-C001 store; store a NonCritical one in either store.
- Violation: a payment follow-up enqueued into Redis because it is faster.
- Risk: a Redis failover or an InMemory restart loses work the business cannot lose.
- Fix: record each task type's criticality and store beside its handler; move Critical types to VP-C001.

### Enqueue a follow-up in the data change's transaction
Enqueue a task that follows a data change through the data port that writes the change, inside the same transaction, per [[./adr/enqueue-through-the-data-port.md|adr/enqueue-through-the-data-port]].
- Violation: the domain calls `history.Record(...)` and then `tasks.Enqueue(...)` as two separate operations.
- Risk: a crash between the two writes leaves a change without its follow-up, or a follow-up for a change that never committed.
- Fix: pass the tasks to the write method (`Record(ctx, entry, tasks...)`); the adapter writes both in one transaction. Only a task with no data change behind it goes through a stand-alone enqueue.

### Map every handler result to a status code
Return `2xx` for success, a code VP-C004's retry classification calls retryable for a failure that retrying can fix, and a non-retryable `4xx` for one it cannot — never let a handler swallow a failure as `2xx`.
- Violation: a handler logs a validation error and returns success so the task "stops failing".
- Risk: work silently disappears; the dead-task list, which exists to show it, stays empty.
- Fix: map domain errors to codes in the handler (unavailable dependency → `503`, invalid payload → `400`, missing entity → `404`, conflicting state → `409`).

### Keep handlers idempotent
Write every handler so that running it twice with the same task has the effect of running it once.
- Risk: a lease expiry or a crash between the handler's success and `done` runs the task again (at-least-once) and applies its effect twice.
- Fix: key the effect on the task `id` (pass it as `Idempotency-Key` downstream, or record it with the effect and check first).

### Evolve payloads compatibly
Change a task type's payload only by adding optional fields, and make its handler accept every payload shape still stored.
- Risk: tasks enqueued before a deploy are run by the new handler (and, during a rolling deploy, new tasks by old handlers); an incompatible shape turns them into dead tasks.
- Fix: add fields as optional with defaults; for a breaking change introduce a new task type name and keep the old handler until its tasks are gone.

### Bound handlers by the lease
Configure the lease above the slowest task type's run time and make every handler honour cancellation.
- Risk: a handler outliving its lease is cancelled and its outcome discarded ([[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/taskbox-run-bounded-by-lease|lease ADR]]); one that ignores cancellation overlaps its own retry.
- Fix: pass the task's cancellation token/context to every I/O call the handler makes; raise the lease for slow task types.

### Run the conformance feature
Run [[./Implementation/features/taskbox-conformance.feature.create.md#MUST|taskbox-conformance.feature]] per its own rules in every stack realization.
- Risk: without the shared feature a stack's ordering, lease, and retention behaviour is untested against the contract.
- Fix: follow that file's MUST rules.

## SHOULD

### Name task types by the work, stably
Name a task type for the work in kebab-case (`recheck-flagged-link`), never after a class or function, and never rename it while tasks of it may be stored.

### Watch dead tasks
Expose the number of dead tasks per queue (log or metric) and alert when it is above zero — a dead task stops its whole group.

### Choose the group by the entity
Set `queue_group` to the key whose tasks must not overtake each other (the entity id, the normalized URL); leave it `null` for independent tasks so they run in parallel.

# Check list
- [ ] Every task type has a recorded criticality, and Critical types live only in the VP-C001 store.
- [ ] Every task that follows a data change is enqueued inside that change's transaction, through the data port.
- [ ] Every handler maps its results to status codes per [Map every handler result to a status code](#map-every-handler-result-to-a-status-code) and is idempotent.
- [ ] The lease exceeds the slowest handler; handlers pass their cancellation on.
- [ ] The stack realization runs `taskbox-conformance.feature` unchanged, once per supported store, green.
