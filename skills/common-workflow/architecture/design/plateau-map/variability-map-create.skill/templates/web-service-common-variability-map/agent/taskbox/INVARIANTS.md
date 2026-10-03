# TaskBox in Go — anchor (plan steps 1–3)

> **Superseded in part (2026-09-29).** The owner moved TaskBox into its own repositories (solution-create ADR `contract-vp-realized-by-libraries`): the contract, the conformance feature, and the usage rules live in `taskbox-spec` (`doc/skills/`, `doc/adr/`); code and seams in `taskbox-go` / `-dotnet` / `-python`. Here A1 and A2 are now pointer skills, the contract file is a pointer, and GW009.001 keeps a pre-release copy of the library until `taskbox-go` v0.1.0. `agent/taskbox/check.sh` checks that shape; the invariants below describe the PostgreSQL work as it was built and proven.

Anchor document per `bulk-authoring-harness` for steps 1–3 of STATUS.md's "Implementation plan — TaskBox → Outbox". Every artifact below is checked against it; the owner reviews this file instead of every file.

## 1. What is created

| # | Artifact | Where | Kind |
| --- | --- | --- | --- |
| A1 | `solution-taskbox` — stack-agnostic base, realizes VP-C003 by the contract | `skills/common-workflow/architecture/solutions/solution-taskbox.skill/` | new |
| A2 | `solution-taskbox-in-go` — Go extension of A1 | `skills/go/architecture/solutions/solution-taskbox-in-go.skill/` | new |
| A3 | Plateau `GW009.001` = GW007.001 + TaskBox (+ the goose migrations VP-C001 already allows) | `skills/go/architecture/plateau/gw009-001/plateau-gw009-001.skill/` + `structure/` | new |
| A4 | Map updates: Go `variability-map.md` VP-C003 row, Go `plateau-repository.md`, common-plateau registry row `009`, STATUS/DECISIONS | existing files | modify |

Location: `solution-create` fixes it — base in `skills/common-workflow/architecture/solutions/`, extension in `skills/{stack}/architecture/solutions/` (its ADR `multi-stack-solution-location`); the `solution-conformance-testing` pair follows the same split under `test/`.

Code: combination PostgreSQL / Redis / TaskBox Yes / GrpcOutbound Yes, everything else No, is not in the registry → new row **009**; stack VP set {VP1} → `{specific}` **001**. File form per `plateau-map-create`: `gw009-001`. Title: *persistent service with TaskBox*. Parent: `plateau-persistent-service` (GW007.001).

## 2. Invariants

1. **The contract is the source of truth.** `vp-c003-taskbox.contract.md` is linked, never restated. A1 adds only what the contract leaves to a solution. A deviation found while coding is a question to the owner (§5), never a silent change.
2. **A1 is stack-agnostic.** No Go/pgx/goose name in it, no link to A2. Its `Implementation/` holds only stack-independent elements: the **conformance feature file** (contract §8 as Gherkin, every §8 bullet covered, run unchanged by every stack). The schema DDL stays only in the contract; a stack's migration copies it.
3. **A2 owns only the client and the code** (contract §intro). PostgreSQL: `pgx/v5`, `enqueue(tx pgx.Tx, …)` never commits, group lock (`INSERT … ON CONFLICT DO UPDATE` on `taskbox_group`) before the task insert, claim = the contract's `UPDATE … WHERE seq IN (SELECT … FOR UPDATE SKIP LOCKED)`, verbatim.
4. **Schema via the service's own migrations** (contract §7): TaskBox v1 is a goose migration in the service's history (`solution-go-db-migrations`), not `CREATE TABLE IF NOT EXISTS`. A3 therefore also composes `solution-go-db-migrations` — part of VP-C001 PostgreSQL, so the code does not change.
5. **Handler outcome = HTTP status code**; classification per VP-C004 (408/429/502/503/504 and 500 retryable; any other non-2xx → `dead` at once); `max(backoff, Retry-After)`; no handler → 503.
6. **Group stops at its first `dead`**; requeue/cancel are exposed as Go functions (no HTTP admin endpoint in A3).
7. **Worker never outlives its lease**: the handler's `ctx` deadline = lease end, and every outcome write is fenced by `status = 'running' AND attempt = <claimed attempt>` so a late outcome from an expired lease is discarded (see Q1).
8. **Domain stays TaskBox-agnostic**: the domain decides *that* a task is needed; the adapter that owns the transaction writes the data and enqueues it in the same `pgx.Tx` (see Q4). A task handler is an **inbound adapter** (`internal/api/tasks/`), like HTTP/gRPC: decode payload → call the domain service → map its error to a status code.
9. **A3's running example**: a check that comes back *flagged* writes its history row **and** a delayed `recheck-flagged-link` task (group = normalized URL) in one transaction; the handler re-asks the reputation service and records the fresh result. Reputation unavailable → 503 (retried); invalid payload → 400 (dead).
10. Every artifact meets `skill-design`, `skill-content`, `skill-tags`, `solution-create`, `plateau-create-by-solutions`; every choice between variants is an ADR (`adr-create`).

## 3. How it is verified

- **Ground truth (A3):** `go build ./...`, `go vet ./...`, `make unit-test` green, with the §8 feature file run **against a real local PostgreSQL 18** (`TEST_DATABASE_DSN`); without the DSN those scenarios fail, never skip. Plus a runtime smoke test: service + worker, a flagged check → task row → re-check recorded.
- **Order-under-concurrency proof:** the "concurrent enqueue into one group" scenario is mutation-tested once by removing the group lock — it must fail, otherwise the scenario proves nothing.
- **Mechanical:** the common map's `agent/check.sh` exits 0 before every commit; a small `agent/taskbox/check.sh` checks A1–A3 links, frontmatter (`creates`/`extends`/`depends_on`/`adr`) resolve, tags present, no template `hint`/`example` blocks, every §8 bullet has a scenario.
- **Fresh-eyes audit** per wave (separate pass against this file + the design skills).

## 4. Waves (one commit each)

| Wave | Content |
| --- | --- |
| W1 | this anchor + `check.sh` skeleton |
| W2 | A1 `solution-taskbox` (+ conformance feature, ADRs) |
| W3 | A2 PostgreSQL part + A3 plateau built and verified together (code is proven in the example, then written into A2's `Implementation/`) |
| W4 | A4 map/registry/repository updates, STATUS/DECISIONS |
| W5 | A2 Redis part, verified by the §8 feature against the plateau's Redis |
| — | PR into `develop` |

## 5. Questions for the owner — answered 2026-09-28

All four recommendations accepted (Q2 "for now, to be revisited"). Q1 and Q2 are applied to the contract with ADRs `taskbox-run-bounded-by-lease` and `taskbox-redis-task-hash`; Q2 (c) was refined while writing it — see DECISIONS.md. Q3: A2 covers PostgreSQL and Redis; SQLite/InMemory stay `planned`. The original questions:

**Q1 — contract clarification (PostgreSQL, all stacks).** §8 says "two workers never run the same task at the same time", but a handler that outlives its lease is re-claimed while still running. Proposal: add to §4 "a handler runs at most until its lease ends (cancelled after); an outcome written after the task was re-claimed is discarded (fenced by `attempt`)". Also editorial: the line "Every attempt records its outcome in `last_status`." sits inside the §4 table and cuts off the requeue/cancel rows — move it below the table.

**Q2 — Redis section is incomplete; proposed amendments before any Redis code:**
- (a) Stream entries are immutable, so a retrying head task has nowhere to keep `attempt`, `run_at`, `last_status`, `last_error`. Moreover, nothing keeps a *finished* task, so Inbox's `GET …/tasks/<status_key>` cannot be answered from Redis. Proposal: the task's §1 fields live in a hash `{taskbox:<q>}:task:<id>` (TTL = effective retention once finished) plus `{taskbox:<q>}:status:<status_key>` → id; streams/lists carry only ids.
- (b) A grouped task with a future `run_at` goes to `…:delayed` and enters its partition only when due. A later task of the same group that is due now overtakes it, which breaks §3. In SQL the delayed head holds its group. Proposal: a grouped task always enters its partition at enqueue, and its partition waits for the head's `run_at`, as it does for a retrying head. Only ungrouped tasks use `…:delayed`.
- (c) Order race: a task enqueued into a stopped group lands in the partition. A requeue that runs before the worker parks that task appends the dead task and the parked tasks *after* it. Proposal: enqueue is always a Lua script that pushes to `…:parked:<group>` when the group is in `…:stopped`. Requeue/cancel also sweep that group's entries still left in the partition.
- (d) `hash(queue_group) mod partitions` names no hash function, so Go and dotnet would route one group to different partitions. Proposal: CRC32 (IEEE) of the UTF-8 group; the partition count is stored in `{taskbox:<q>}:meta` and never changed while tasks exist.

**Q3 — scope of SQLite / Redis / InMemory in this task.** No Go plateau has SQLite or InMemory (both are `planned` Variants of VP-C001/VP-C002), and "no throwaway builds" says a Variant is verified by a plateau. *Recommendation:* in this task, PostgreSQL + Redis (GW009.001 has Redis, so the §8 feature runs against the plateau's own Redis, and the example gets one NonCritical task type there). SQLite and InMemory stay `planned` in the VP-C003 cell until a plateau with that store exists. *Alternative:* write all four now; SQLite/InMemory are then verified only by the feature file in the GW009.001 example, where the service does not use them.

**Q4 — how the domain asks for a task in the same transaction.** *Recommendation:* the data port takes the tasks alongside the write, for example `LinkHistory.Record(ctx, entry, tasks ...interfaces.Task)`. `interfaces.Task` is a plain domain value `{Type, Payload, Group, RunAt}`. The store enqueues inside its own `pgx.Tx`. TaskBox itself lives in `internal/taskbox/` (a shared mechanism, not an adapter), so adapters may import it without breaking "no sibling infrastructure imports". *Alternative:* a unit-of-work port whose transaction is carried in `ctx` — less explicit, and a task could be enqueued outside any transaction without anyone noticing.
