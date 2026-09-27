# TaskBox storage contract (VP-C003) — draft

> **Draft — under owner review.** Not yet referenced by any stack row.

The stack-agnostic contract every stack's TaskBox realization implements, so that the stored tasks look the same whatever language wrote them: a service rewritten in another stack keeps its tables, streams, and pending tasks. It defines the **mechanism only** — how a task is stored, claimed, retried, and dead-lettered. It knows nothing about what a task does. Concept and store rules: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map#VP-C003 TaskBox|VP-C003 TaskBox]].

A stack's solution decides only the client library and the code that writes, reads, and executes against the structures below.

## 1. Task

| Field | Type | Meaning |
| --- | --- | --- |
| `id` | UUID (text in SQLite/Redis) | Assigned at enqueue. |
| `queue` | text, default `default` | Lets a service run separate worker pools; not an ordering unit. |
| `type` | text | The task type name — the **only** key a worker dispatches on. Stable across languages: `send-order-confirmation`, not a class name. |
| `payload` | JSON text | Parameters for the handler; its shape is owned by whoever owns the task type. |
| `idempotency_key` | text, nullable | Optional; an enqueue with a key that already exists in the store is a no-op. |
| `status` | `pending` / `running` / `done` / `dead` | Lifecycle, §3. |
| `attempt` | int, starts at 0 | Number of executions started. |
| `max_attempts` | int, default 10 | After this many failures → `dead`. |
| `run_at` | timestamp (UTC) | Not claimable before this — delayed tasks and retry backoff. |
| `locked_until` | timestamp, nullable | Lease end while `running`; a task whose lease expired is claimable again. |
| `last_error` | text, nullable | Error of the latest failed attempt. |
| `created_at`, `updated_at` | timestamp (UTC) | Bookkeeping. |

Criticality is not stored: it is implied by the store the task lives in (VP-C003 concept).

## 2. Ports

- **Enqueue** — `enqueue(tx, type, payload, {queue, run_at, max_attempts, idempotency_key})`. `tx` is the caller's own unit of work in that store (SQL transaction, Redis `MULTI`, nothing for InMemory); enqueue never commits by itself when a `tx` is given.
- **Handler registry** — the service registers one handler per `type`. A handler receives `(id, payload, attempt)` and either returns (success) or raises (failure). TaskBox never inspects `payload`.
- **Worker** — claims due tasks (§4), dispatches each by `type`, and records the outcome (§3).

## 3. Lifecycle

| From | Event | To |
| --- | --- | --- |
| — | enqueue | `pending` |
| `pending` (due) | claimed | `running`, `attempt + 1`, `locked_until = now + lease` |
| `running` | handler succeeds | `done` |
| `running` | handler fails, `attempt < max_attempts` | `pending`, `run_at = now + backoff(attempt)`, `last_error` set |
| `running` | handler fails, `attempt ≥ max_attempts` | `dead` |
| `running` | lease expires (worker died) | claimable again, as if `pending` |
| `running` | no handler registered for `type` | treated as a failure — so a task reaching an older worker during a rolling deploy is retried, not lost |

- **At-least-once.** A task may run more than once (a lease expiring mid-run, a crash after the handler succeeded but before `done` was written). Every handler is idempotent.
- **No ordering guarantee.** Workers take due tasks roughly by `run_at`; nothing orders two tasks relative to each other.
- **Backoff:** `min(1s × 2^attempt, 1h)`; lease default 5 min. Both configurable per service.
- **Retention:** `done` tasks are deleted after 7 days (configurable); `dead` tasks stay until handled by a person.

## 4. Per-store realization

### PostgreSQL (schema v1)

```sql
CREATE TABLE taskbox_task (
  id              uuid PRIMARY KEY,
  queue           text        NOT NULL DEFAULT 'default',
  type            text        NOT NULL,
  payload         jsonb       NOT NULL,
  idempotency_key text        UNIQUE,
  status          text        NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','running','done','dead')),
  attempt         int         NOT NULL DEFAULT 0,
  max_attempts    int         NOT NULL DEFAULT 10,
  run_at          timestamptz NOT NULL DEFAULT now(),
  locked_until    timestamptz,
  last_error      text,
  created_at      timestamptz NOT NULL DEFAULT now(),
  updated_at      timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX taskbox_task_due ON taskbox_task (queue, run_at) WHERE status IN ('pending','running');
```

Claim (many workers, no double claim):

```sql
UPDATE taskbox_task SET status = 'running', attempt = attempt + 1,
       locked_until = now() + $lease, updated_at = now()
WHERE id IN (
  SELECT id FROM taskbox_task
  WHERE queue = $queue AND run_at <= now()
    AND (status = 'pending' OR (status = 'running' AND locked_until < now()))
  ORDER BY run_at LIMIT $batch
  FOR UPDATE SKIP LOCKED)
RETURNING id, type, payload, attempt;
```

Enqueue with a key: `INSERT … ON CONFLICT (idempotency_key) DO NOTHING`.

### SQLite (schema v1)

Same columns: `id`, `payload`, timestamps as `TEXT` (UUID / JSON / ISO-8601 UTC); `CHECK` and the partial index as in PostgreSQL. SQLite has one writer and no `SKIP LOCKED`, so a claim is one `BEGIN IMMEDIATE` transaction running the same `UPDATE … WHERE id IN (SELECT … LIMIT $batch) RETURNING …` (SQLite ≥ 3.35). One service instance by construction (VP-C001).

### Redis

All keys of a queue share the hash tag `{taskbox:<queue>}` so they sit in one cluster slot. To enqueue atomically with Redis business data, the caller's data keys must be in that slot too — the caller's concern.

| Key | Type | Holds |
| --- | --- | --- |
| `{taskbox:<queue>}:stream` | Stream | Due tasks; entry fields = the §1 fields |
| `{taskbox:<queue>}:delayed` | Sorted set, score = `run_at` (ms) | Tasks not yet due, as the serialized entry |
| `{taskbox:<queue>}:dead` | Stream | Dead tasks |
| `{taskbox:<queue>}:key:<idempotency_key>` | String, with TTL = retention | Marks an idempotency key as already enqueued |

- **Enqueue:** inside the caller's `MULTI`: `XADD …:stream` (due now) or `ZADD …:delayed` (future `run_at`). With an idempotency key the check and the add must be one step, so the enqueue is a Lua script: `SET …:key:<k> 1 NX EX <retention>` and add only when it succeeded.
- **Consumer group:** `taskbox-workers` on `…:stream`. **Claim:** `XREADGROUP`; stale entries (lease = min-idle time) via `XAUTOCLAIM`.
- **Success:** `XACK` + `XDEL`. **Failure:** `XACK` + `XDEL` + `ZADD …:delayed` with `attempt + 1` and the backoff `run_at`, or `XADD …:dead` past `max_attempts`.
- **Due mover:** moves members of `…:delayed` with score ≤ now into `…:stream` (a Lua script, run by the worker loop).
- **Lifetime:** every entry is lost with its store (VP-C002); `…:stream` and `…:dead` are also trimmed by age with `XTRIM MINID` at the retention window.

### InMemory

A per-process queue of §1 records (a priority queue by `run_at`) drained by worker threads/goroutines, same lifecycle and handler registry; there is no `tx` — enqueue happens when called. Every task ends with the process (VP-C002's lifetime); NonCritical only.

## 5. Schema versions and migrations

The DDL above is **schema v1** of this contract. Every change adds a numbered version here (v2, …) with its DDL delta. Each stack applies these versions through its **own** migration tool (goose, EF Core migrations, …) as ordinary migrations in the service's history.

Switching a service to another stack is not designed yet — decided when a real switch happens. Known shape of the answer: the new stack's migration history starts from a baseline equal to the contract version the database is already at, instead of re-creating the tables.

## 6. Conformance scenarios

Every stack realization passes the same scenarios, one run per store it supports:
- A task enqueued in a transaction that rolls back never runs; committed, it runs exactly once when the handler succeeds.
- A failing handler is retried with growing `run_at`; after `max_attempts` failures the task is `dead` with `last_error`.
- A task claimed by a worker that dies is claimed again after its lease.
- Two workers never run the same task at the same time.
- An enqueue repeating an existing `idempotency_key` adds no task.
- A task whose `type` has no handler is retried, not dropped.
- A delayed task does not run before its `run_at`.
