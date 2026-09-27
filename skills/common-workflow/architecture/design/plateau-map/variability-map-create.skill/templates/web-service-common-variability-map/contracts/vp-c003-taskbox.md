# TaskBox storage contract (VP-C003) — draft

> **Draft — under owner review.** Not yet referenced by any stack row.

The stack-agnostic contract every stack's TaskBox realization implements, so that the stored tasks look the same whatever language wrote them: a service rewritten in another stack keeps its tables, streams, and pending tasks. It defines the **mechanism only** — how a task is stored, ordered, claimed, retried, dead-lettered, and removed. It knows nothing about what a task does. Concept and store rules: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map#VP-C003 TaskBox|VP-C003 TaskBox]].

A stack's solution decides only the client library and the code that writes, reads, and executes against the structures below.

## 1. Task

| Field | Type | Meaning |
| --- | --- | --- |
| `seq` | bigint, store-assigned, increasing | Primary key; keeps inserts appending to the index. Not an ordering guarantee by itself (§3). |
| `id` | UUIDv7 | Global identifier, assigned by the enqueuing code: logs, handlers, cross-system references. Time-ordered, so it never scatters an index. |
| `queue` | text, default `default` | Selects a worker pool. |
| `queue_group` | text, nullable | Ordering key inside a queue (like a Kafka message key). `null` = no ordering with any other task. |
| `group_seq` | bigint, nullable | Position inside `(queue, queue_group)`; set only when `queue_group` is set (§3). |
| `type` | text | Task type name — the **only** key a worker dispatches on. Stable across languages: `send-order-confirmation`, not a class name. |
| `payload` | JSON | Handler parameters; the shape is owned by whoever owns the task type. |
| `idempotency_key` | text, nullable, unique | An enqueue with a key already present in the store adds nothing. |
| `status` | `pending` / `running` / `done` / `dead` | Lifecycle, §4. |
| `attempt` | int, starts at 0 | Executions started. |
| `max_attempts` | int, default 10 | After this many failures → `dead`. |
| `run_at` | timestamp (UTC) | Not claimable before this — delayed tasks and retry backoff. |
| `locked_until` | timestamp, nullable | Lease end while `running`; an expired lease makes the task claimable again. |
| `last_error` | text, nullable | Error of the latest failed attempt. |
| `retention` | duration, nullable | How long to keep the task after it finishes; effective value = `max(service default, retention)` (§5). |
| `created_at`, `updated_at` | timestamp (UTC) | Bookkeeping. |
| `finished_at` | timestamp, nullable | Set when the task becomes `done` or `dead`; retention counts from it. |

Criticality is not stored: it is implied by the store the task lives in (VP-C003 concept).

## 2. Ports

- **Enqueue** — `enqueue(tx, type, payload, {queue, queue_group, run_at, max_attempts, idempotency_key, retention})`. `tx` is the caller's own unit of work in that store (SQL transaction, Redis `MULTI`/script, nothing for InMemory); enqueue never commits by itself when a `tx` is given.
- **Handler registry** — the service registers one handler per `type`. A handler receives `(id, payload, attempt)` and either returns (success) or raises (failure). TaskBox never inspects `payload`.
- **Worker** — claims due tasks (§6), dispatches each by `type`, records the outcome (§4).

## 3. Ordering

- **Within `(queue, queue_group)`: strict order.** Tasks of one group run one at a time, in `group_seq` order: a task is claimable only when no earlier task of its group is still `pending` or `running`.
- **`group_seq` order = commit order.** `seq` is assigned at insert, not at commit, so two concurrent transactions can commit out of `seq` order. `group_seq` is therefore taken from a per-group counter that is locked by the enqueuing transaction until it commits: a second enqueue into the same group waits, and numbers follow commit order.
- **Everything else: no order.** Tasks with `queue_group = null`, and tasks of different groups, run in parallel, roughly by `run_at`.
- **Head-of-line blocking is the price.** While the head task of a group waits for a retry, the whole group waits. What happens to a group when its head task goes `dead` is an **open question** — see §9.

## 4. Lifecycle

| From | Event | To |
| --- | --- | --- |
| — | enqueue | `pending` |
| `pending` (due, and head of its group) | claimed | `running`, `attempt + 1`, `locked_until = now + lease` |
| `running` | handler succeeds | `done`, `finished_at = now` |
| `running` | handler fails, `attempt < max_attempts` | `pending`, `run_at = now + backoff(attempt)`, `last_error` set |
| `running` | handler fails, `attempt ≥ max_attempts` | `dead`, `finished_at = now` |
| `running` | lease expires (worker died) | claimable again, as if `pending` |
| `running` | no handler registered for `type` | a failure — so a task reaching an older worker during a rolling deploy is retried, not lost |

- **At-least-once.** A task may run more than once (lease expiry mid-run, a crash between handler success and writing `done`). Every handler is idempotent.
- **Backoff** `min(1s × 2^attempt, 1h)`; **lease** default 5 min — both configurable per service.

## 5. Retention and idempotency window

- A `done` task is deleted once `now > finished_at + max(service default, retention)`; the service default is 7 days, configurable. A task can extend its own retention, never shorten it below the default.
- A `dead` task in a VP-C001 store is never deleted automatically — it waits for a person. In a VP-C002 store it keeps a lifetime like every entry there (the store's concept wins).
- The effective retention is also the **idempotency window**: an `idempotency_key` is remembered as long as its task row (or Redis marker) exists. A task type that must reject duplicates for longer sets a longer `retention`.

## 6. Per-store realization

### PostgreSQL (schema v1)

```sql
CREATE TABLE taskbox_task (
  seq             bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  id              uuid        NOT NULL UNIQUE,
  queue           text        NOT NULL DEFAULT 'default',
  queue_group     text,
  group_seq       bigint,
  type            text        NOT NULL,
  payload         jsonb       NOT NULL,
  idempotency_key text        UNIQUE,
  status          text        NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','running','done','dead')),
  attempt         int         NOT NULL DEFAULT 0,
  max_attempts    int         NOT NULL DEFAULT 10,
  run_at          timestamptz NOT NULL DEFAULT now(),
  locked_until    timestamptz,
  last_error      text,
  retention       interval,
  created_at      timestamptz NOT NULL DEFAULT now(),
  updated_at      timestamptz NOT NULL DEFAULT now(),
  finished_at     timestamptz,
  CHECK ((queue_group IS NULL) = (group_seq IS NULL)),
  UNIQUE (queue, queue_group, group_seq)
);
CREATE INDEX taskbox_task_due   ON taskbox_task (queue, run_at)                  WHERE status IN ('pending','running');
CREATE INDEX taskbox_task_group ON taskbox_task (queue, queue_group, group_seq)  WHERE status IN ('pending','running');
CREATE INDEX taskbox_task_done  ON taskbox_task (finished_at)                    WHERE status = 'done';

CREATE TABLE taskbox_group (
  queue       text   NOT NULL,
  queue_group text   NOT NULL,
  last_seq    bigint NOT NULL,
  PRIMARY KEY (queue, queue_group)
);
```

Group number, inside the enqueuing transaction (the row lock serializes the group until commit):

```sql
INSERT INTO taskbox_group (queue, queue_group, last_seq) VALUES ($queue, $group, 1)
ON CONFLICT (queue, queue_group) DO UPDATE SET last_seq = taskbox_group.last_seq + 1
RETURNING last_seq;
```

Claim — due tasks that are the head of their group (or ungrouped), without double claims:

```sql
UPDATE taskbox_task SET status = 'running', attempt = attempt + 1,
       locked_until = now() + $lease, updated_at = now()
WHERE seq IN (
  SELECT t.seq FROM taskbox_task t
  WHERE t.queue = $queue AND t.run_at <= now()
    AND (t.status = 'pending' OR (t.status = 'running' AND t.locked_until < now()))
    AND (t.queue_group IS NULL OR NOT EXISTS (
      SELECT 1 FROM taskbox_task e
      WHERE e.queue = t.queue AND e.queue_group = t.queue_group AND e.group_seq < t.group_seq
        AND e.status IN ('pending','running')))
  ORDER BY t.run_at LIMIT $batch
  FOR UPDATE SKIP LOCKED)
RETURNING seq, id, type, payload, attempt;
```

Enqueue with a key: `INSERT … ON CONFLICT (idempotency_key) DO NOTHING` (the group counter is taken only when the insert happens).

### SQLite (schema v1)

Same tables and columns; `seq` is `INTEGER PRIMARY KEY`; UUID, JSON, timestamps as `TEXT` (ISO-8601 UTC); `retention` as integer seconds. SQLite has one writer, so every write transaction is already serialized: `group_seq` = `last_seq + 1` from `taskbox_group` in the enqueuing transaction, and a claim is one `BEGIN IMMEDIATE` transaction running the same `UPDATE … RETURNING` (SQLite ≥ 3.35) without `SKIP LOCKED`. One service instance by construction (VP-C001).

### Redis

Ordering in Redis follows Kafka: a queue has a fixed number of **partitions**; a task's partition is `hash(queue_group) mod partitions` (ungrouped tasks: any partition); each partition is drained by **one worker at a time**, which gives per-group order. All keys of a queue share the hash tag `{taskbox:<queue>}` so they sit in one cluster slot; to enqueue atomically with Redis business data, the caller's data keys must be in that slot too — the caller's concern.

| Key | Type | Holds |
| --- | --- | --- |
| `{taskbox:<queue>}:p:<n>` | Stream | Due tasks of partition `n`; entry fields = the §1 fields |
| `{taskbox:<queue>}:p:<n>:lease` | String, `SET NX PX <lease>` | Which worker owns partition `n` right now |
| `{taskbox:<queue>}:delayed` | Sorted set, score = `run_at` (ms) | Tasks not yet due, as the serialized entry |
| `{taskbox:<queue>}:dead` | Stream | Dead tasks |
| `{taskbox:<queue>}:key:<idempotency_key>` | String, TTL = effective retention | Marks an idempotency key as already enqueued |

- **Enqueue:** inside the caller's `MULTI`: `XADD …:p:<n>` (due now) or `ZADD …:delayed` (future `run_at`). With an idempotency key, the check and the add are one Lua script (`SET …:key:<k> 1 NX EX <retention>`, add only on success).
- **Claim:** a worker takes a partition lease, then reads that partition's stream in order and keeps renewing the lease; a partition whose lease expired is taken over by another worker, which continues from the first unacknowledged entry.
- **Retry keeps order:** a failed head task stays at the head of its partition and is retried after its backoff — the partition waits, exactly as a group does in SQL (§3).
- **Success:** `XACK`/`XDEL`. **Dead:** `XADD …:dead`, then `XDEL` (subject to §9).
- **Due mover:** moves members of `…:delayed` with score ≤ now into their partition stream (a Lua script run by the worker loop).
- **Lifetime:** every entry is lost with its store (VP-C002); `…:dead` is trimmed by age with `XTRIM MINID` at the retention window.

### InMemory

Per-process FIFO per `(queue, queue_group)` plus one queue for ungrouped tasks, a priority queue by `run_at` for delayed ones, and worker threads/goroutines that run at most one task per group at a time. Same lifecycle and handler registry; no `tx` — enqueue happens when called. Every task ends with the process (VP-C002 lifetime); NonCritical only.

## 7. Schema versions and migrations

The DDL above is **schema v1** of this contract. Every change adds a numbered version here (v2, …) with its DDL delta. Each stack applies these versions through its **own** migration tool (goose, EF Core migrations, …) as ordinary migrations in the service's history.

Switching a service to another stack is not designed yet — decided when a real switch happens. Known shape of the answer: the new stack's migration history starts from a baseline equal to the contract version the database is already at, instead of re-creating the tables.

## 8. Conformance scenarios

Every stack realization passes the same scenarios, one run per store it supports:
- A task enqueued in a transaction that rolls back never runs; committed, it runs exactly once when the handler succeeds.
- A failing handler is retried with growing `run_at`; after `max_attempts` failures the task is `dead` with `last_error` and `finished_at`.
- A task claimed by a worker that dies is claimed again after its lease.
- Two workers never run the same task at the same time.
- Tasks of one `queue_group` run one at a time in `group_seq` order, even when they were enqueued by concurrent transactions; tasks of different groups run in parallel.
- A retrying head task holds back the rest of its group until it succeeds.
- An enqueue repeating an existing `idempotency_key` adds no task, for as long as the effective retention keeps the first one.
- A `done` task is removed after `max(default, retention)`; a `dead` task is not removed.
- A task whose `type` has no handler is retried, not dropped.
- A delayed task does not run before its `run_at`.

## 9. Open questions

- **A dead head task and its group.** Either (a) the group stays blocked until a person resolves the dead task — strict order, one poisoned task stops the group; or (b) the group moves on and the dead task waits in the dead-letter store — the group keeps flowing, but later tasks run without the earlier one. Kafka consumers are usually built as (b) with a DLQ. The SQL claim in §6 implements (b) as written — the head check looks only at `pending`/`running` tasks; (a) adds `dead` to that check.
