---
name: solution-taskbox-in-go
description: Go realization of solution-taskbox (VP-C003) — internal/taskbox with a pgx PostgreSQL store, enqueue inside the data adapter's pgx.Tx, a lease-bounded worker pool, task handlers as an inbound adapter, and the shared conformance feature run on a real PostgreSQL
whenToUse: when a Go web-service with solution-persistent-db must run work later, retry it, or keep it in order per key — in particular work enqueued atomically with a PostgreSQL data change — or when reviewing a Go TaskBox realization against the VP-C003 contract
domain: skill
type: architecture
version: 20260928000000
updated: 20260928
tags:
  - skill/architecture/solution
  - solution/taskbox-in-go
  - stack/go
  - concern/architecture
  - concern/testing
  - concern/testing/bdd
  - taskbox
  - pgx
creates:
  - "internal/taskbox/"
  - "internal/taskbox/taskbox.go"
  - "internal/taskbox/handler.go"
  - "internal/taskbox/worker.go"
  - "internal/taskbox/pgstore/"
  - "internal/taskbox/pgstore/store.go"
  - "internal/taskbox/features/taskbox-conformance.feature"
  - "internal/taskbox/test/"
  - "internal/domain/interfaces/task.go"
  - "internal/api/tasks/"
  - "internal/api/tasks/handlers.go"
  - "internal/infrastructure/{store}/migrations/{NNNNN}_taskbox_v1.sql"
extends:
  - "internal/domain/interfaces/{store-port}.go"
  - "internal/domain/services/{service}.go"
  - "internal/infrastructure/{store}/"
  - "internal/infrastructure/{store}/store.go"
  - "internal/config/config.go"
  - "cmd/{service}/main.go"
  - "go.mod"
depends_on:
  - "[[skills/common-workflow/architecture/solutions/solution-taskbox.skill/solution-taskbox.skill.md|solution-taskbox]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[skills/go/test/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]]"
built_on_plateau:
adr:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/adr/taskbox-package-outside-infrastructure.md|TaskBox package outside infrastructure]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/adr/conformance-on-a-real-database.md|Conformance on a real database]]"
---

# Goal
- `internal/taskbox` (task values, registry, worker) and `internal/taskbox/pgstore` realizing the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox.contract|TaskBox contract]] on PostgreSQL.
- TaskBox schema v1 as one more goose migration of the service.
- A data port whose write takes follow-up tasks, and a data adapter that writes both in one `pgx.Tx`.
- `internal/api/tasks` with one handler per task type, and the worker running in `main.go`'s errgroup.
- `internal/taskbox/test` running `taskbox-conformance.feature` on a real PostgreSQL, green.

# Capabilities
- Follow-up work is committed with the change that caused it, survives restarts, and runs after the change is visible.
- Retries, dead-lettering, per-group order, and retention behave exactly as in every other stack that realizes the contract.
- A pending task written by this service can be run by the same service rewritten in another stack, because the tables are the contract's.

# Core Principles
- **Base rules first** - Everything [[skills/common-workflow/architecture/solutions/solution-taskbox.skill/solution-taskbox.skill.md|solution-taskbox]] requires applies unchanged; this solution adds only the Go client and code.
- **pgx, not a job library** - `pgx/v5` is the driver `solution-persistent-db` already uses, so the data write and the enqueue share one `pgx.Tx`; River and similar libraries bring their own schema and are excluded by the contract.
- **Mechanism, not adapter** - `internal/taskbox` is a shared mechanism that data adapters import — [[./adr/taskbox-package-outside-infrastructure.md|adr/taskbox-package-outside-infrastructure]].
- **Proven by execution** - Every file under `internal/taskbox` in `Implementation/` is the code that passed the conformance feature on PostgreSQL 18, copied verbatim.
- Terms: [[./glossary/skip-locked.md|FOR UPDATE SKIP LOCKED]], [[./glossary/lease-and-fencing.md|lease and fencing token]].

# Boundaries
- Realizes the PostgreSQL store only. The contract's SQLite, Redis, and InMemory stores are not realized here; a Go service needing one is not covered by this solution yet.
- Exposes requeue / cancel as `Store` methods only; an admin endpoint or tool that calls them is not part of this solution.
- Assumes the service's CI provides a PostgreSQL for `make unit-test` ([[./adr/conformance-on-a-real-database.md|adr/conformance-on-a-real-database]]).

# Adr
- [[./adr/taskbox-package-outside-infrastructure.md|TaskBox package outside infrastructure]]
  - Selected variant: `internal/taskbox` plus one subpackage per store; adapters import it
- [[./adr/conformance-on-a-real-database.md|Conformance on a real database]]
  - Selected variant: `TEST_DATABASE_DSN` from the environment; an empty value fails the run

# Requirements
SOLUTION:
- [[skills/common-workflow/architecture/solutions/solution-taskbox.skill/solution-taskbox.skill.md|solution-taskbox]]
  - [[skills/common-workflow/architecture/solutions/solution-taskbox.skill/Implementation/features/taskbox-conformance.feature.create.md|taskbox-conformance.feature]] - copied verbatim, run by `internal/taskbox/test`
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]
  - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]] - the data adapter whose write gains the tasks
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]
  - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/infrastructure/{store}/migrations.go.create.md|migrations.go]] - `Migrate`, which applies the TaskBox migration with the service's own
- [[skills/go/test/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]]
  - godog runner conventions and the `make unit-test` gate the conformance feature runs in
GO MODULES:
- github.com/jackc/pgx/v5 v5.11
  - `pgx.Tx`, `pgx.BeginFunc` — enqueue inside the caller's transaction; `pgxpool.Pool` — claims and outcomes
- github.com/google/uuid v1.6
  - `uuid.NewV7` — task ids (contract §1)
- github.com/cucumber/godog v0.16
  - the conformance runner

# Template Skill Mutations
REPOSITORY:
- [[./Implementation/Repository.extend.md|Repository]] - extend - `google/uuid`; `TEST_DATABASE_DSN` for the test gate

PACKAGES AND FILES:
- [[./Implementation/internal/taskbox/Package.create.md|internal/taskbox]] - create - the store-independent mechanism
  - [[./Implementation/internal/taskbox/taskbox.go.create.md|taskbox.go]] - create - task values, `Store` interface
  - [[./Implementation/internal/taskbox/handler.go.create.md|handler.go]] - create - `Handler`, `Registry`, `Retryable`, `Backoff`
  - [[./Implementation/internal/taskbox/worker.go.create.md|worker.go]] - create - lease-bounded worker pool
- [[./Implementation/internal/taskbox/pgstore/Package.create.md|internal/taskbox/pgstore]] - create - PostgreSQL store
  - [[./Implementation/internal/taskbox/pgstore/store.go.create.md|store.go]] - create - enqueue, claim, fenced outcomes, requeue/cancel, cleanup
- [[./Implementation/internal/taskbox/test/Package.create.md|internal/taskbox/test]] - create - the conformance runner
  - [[./Implementation/internal/taskbox/test/runner_test.go.create.md|runner_test.go]], [[./Implementation/internal/taskbox/test/world_test.go.create.md|world_test.go]], [[./Implementation/internal/taskbox/test/steps_test.go.create.md|steps_test.go]], [[./Implementation/internal/taskbox/test/postgres_test.go.create.md|postgres_test.go]] - create
- [[./Implementation/internal/domain/interfaces/task.go.create.md|internal/domain/interfaces/task.go]] - create - the `Task` domain value
- [[./Implementation/internal/domain/interfaces/{store-port}.go.extend.md|internal/domain/interfaces/{store-port}.go]] - extend - the write takes `tasks ...Task`
- [[./Implementation/internal/domain/services/{service}.go.extend.md|internal/domain/services/{service}.go]] - extend - decide follow-ups; a method per task type
- [[./Implementation/internal/infrastructure/{store}/Package.extend.md|internal/infrastructure/{store}]] - extend - `{NNNNN}_taskbox_v1.sql` migration
- [[./Implementation/internal/infrastructure/{store}/store.go.extend.md|internal/infrastructure/{store}/store.go]] - extend - data and tasks in one `pgx.Tx`
- [[./Implementation/internal/api/tasks/Package.create.md|internal/api/tasks]] - create - the task handlers' inbound adapter
  - [[./Implementation/internal/api/tasks/handlers.go.create.md|handlers.go]] - create - one handler per task type
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - worker settings
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - one pool, handlers registered, worker in the errgroup

Dependency solutions: `solution-persistent-db` and `solution-go-db-migrations` are applied (the data adapter and the migration runner this solution extends); `solution-cached-db` is not needed and not touched.

# Workflow

## Write with a follow-up (happy path)
1. `{Service}.{Method}` computes its result and decides a follow-up is needed; it builds an `interfaces.Task` (type, payload, group, `RunAt`).
2. It calls `{Port}.{Write}(ctx, entry, task)`.
3. `{store}.Store.{Write}` opens a `pgx.Tx`, inserts the entry, calls `pgstore.Enqueue(ctx, tx, …)` (group lock, then insert), and commits.
4. A worker's claim takes the task once `run_at` has passed and it heads its group.
5. `Handlers.{followUp}` decodes the payload and calls `{Service}.{FollowUpMethod}`; `200` → the task is `done`.

## Dependency down
1. The follow-up method returns `ErrUnavailable`; the handler answers `503`.
2. `pgstore.Finish` sets `pending` with `run_at = now + max(backoff, Retry-After)`; the group waits.
3. After `max_attempts` the task is `dead`; `Store.Requeue` or `Store.Cancel` resumes the group.

## Worker crash
1. A worker dies while its task is `running`.
2. After `locked_until`, another worker's claim takes it again with `attempt + 1`; a late outcome of the dead run is fenced out.

# Rule

## MUST

### Apply the base solution's rules
Apply every MUST of [[skills/common-workflow/architecture/solutions/solution-taskbox.skill/solution-taskbox.skill.md#MUST|solution-taskbox]].
- Risk: the Go code conforms to the contract while the service misuses it (Critical task in the wrong store, a handler that swallows failures).
- Fix: check the service's task types against the base solution's check list.

### Keep the proven code verbatim
Create every `internal/taskbox` file exactly as its Implementation file shows, changing only `{module-path}` and `{store}`.
- Violation: "simplifying" the claim, dropping the `attempt` fence, or skipping the group lock for a first version.
- Risk: the conformance scenarios that proved this code were written to catch exactly those changes; a modified copy is unproven.
- Fix: change the code here first, rerun the conformance feature, then apply it.

### Apply the Implementation files' rules
Apply every MUST of the Implementation files below.
- [[./Implementation/internal/taskbox/pgstore/store.go.create.md#MUST|pgstore/store.go]]
- [[./Implementation/internal/taskbox/worker.go.create.md#MUST|worker.go]]
- [[./Implementation/internal/taskbox/Package.create.md#MUST|internal/taskbox]]
- [[./Implementation/internal/taskbox/test/runner_test.go.create.md#MUST|runner_test.go]]
- [[./Implementation/internal/taskbox/test/Package.create.md#MUST|internal/taskbox/test]]
- [[./Implementation/internal/domain/interfaces/{store-port}.go.extend.md#MUST|{store-port}.go]]
- [[./Implementation/internal/domain/services/{service}.go.extend.md#MUST|{service}.go]]
- [[./Implementation/internal/infrastructure/{store}/store.go.extend.md#MUST|{store}/store.go]]
- [[./Implementation/internal/infrastructure/{store}/Package.extend.md#MUST|{store} migrations]]
- [[./Implementation/internal/api/tasks/handlers.go.create.md#MUST|handlers.go]]
- [[./Implementation/cmd/{service}/main.go.extend.md#MUST|main.go]]
- [[./Implementation/Repository.extend.md#MUST|Repository]]

# Check list
- [ ] Every `internal/taskbox` file matches its Implementation file (module path and `{store}` aside).
- [ ] `{NNNNN}_taskbox_v1.sql` holds the contract's schema v1 DDL verbatim; `Migrate` applies it.
- [ ] The data port's write takes `tasks ...Task`; the adapter writes data and tasks in one `pgx.Tx`.
- [ ] Every task type has one handler in `internal/api/tasks`, registered before `worker.Run`.
- [ ] `TEST_DATABASE_DSN=… make unit-test` runs `taskbox-conformance.feature` (identical to solution-taskbox's) and every PostgreSQL scenario passes.
