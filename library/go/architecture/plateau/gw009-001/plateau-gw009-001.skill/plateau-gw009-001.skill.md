---
name: plateau-gw009-001
description: GW009.001 — persistent service with TaskBox; plateau-persistent-service (GW007.001) plus versioned goose migrations and a PostgreSQL TaskBox (VP-C003) that re-checks every flagged link later, enqueued in the same transaction as its history record
whenToUse: when a Go web-service with PostgreSQL must run follow-up work after a data change — retried, ordered per key, and atomic with the change — or when reviewing a Go service's TaskBox wiring against the VP-C003 contract
domain: skill
type: template
version: 20261012000000
updated: 20261008
tags:
  - skill/template/plateau
  - plateau/plateau-gw009-001
  - stack/go
  - concern/architecture
parent_plateaus:
  - "[[library/go/architecture/plateau/plateau-persistent-service/plateau-persistent-service.skill/plateau-persistent-service.skill.md|plateau-persistent-service]]"
created_by:
  - "[[library/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[library/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
standalone: true
registry:
  - "[[library/go/architecture/registry/cmd-service-main-go.md|cmd-service-main-go]]"
  - "[[library/go/architecture/registry/internal-config-config-go.md|internal-config-config-go]]"
  - "[[library/go/architecture/registry/repo-root.md|repo-root]]"
  - "[[library/go/architecture/registry/internal-domain-services-service-go.md|internal-domain-services-service-go]]"
  - "[[library/go/architecture/registry/internal-api-http-server-go.md|internal-api-http-server-go]]"
  - "[[library/go/architecture/registry/internal-api-grpc-server-go.md|internal-api-grpc-server-go]]"
  - "[[library/go/architecture/registry/internal-infrastructure-store-store-go.md|internal-infrastructure-store-store-go]]"
  - "[[library/go/architecture/registry/internal-infrastructure-store.md|internal-infrastructure-store]]"
  - "[[library/go/architecture/registry/internal-domain-interfaces-store-port-go.md|internal-domain-interfaces-store-port-go]]"
  - "[[library/go/architecture/registry/docs-integration-api-contract.md|docs-integration-api-contract]]"
---

# Goal
- The GW007.001 service (HTTP + gRPC, reputation client, Redis cache, PostgreSQL history) whose schema is a versioned goose history, applied by `cmd/migrate` or at startup.
- A PostgreSQL TaskBox realizing the VP-C003 contract through the `taskbox-go` library (pre-release copy in `internal/taskbox` until v0.1.0), schema v1 as migration `00002`.
- A flagged check recorded **together with** a delayed `recheck-flagged-link` task in one transaction; the task handler re-asks the reputation service and records the fresh verdict.
- The shared TaskBox conformance feature passing on a real PostgreSQL.

Code `GW009.001`: Go web-service, common combination 009 of the [[library/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/registry/web-service-common-plateaus|common-plateau registry]] (PostgreSQL, Redis, TaskBox, GrpcOutbound), stack VPs 001 (VP1 GrpcApi).

# Core Principles
- Everything [[library/go/architecture/plateau/plateau-persistent-service/plateau-persistent-service.skill/plateau-persistent-service.skill.md|plateau-persistent-service]] states still holds: ports-and-adapters, one composition root, thin inbound adapters, godog scenarios, cache failures degrade, history failures fail the request.
- **The domain decides, the adapter commits** - `LinkCheckService.Check` decides that a flagged URL needs a re-check and passes the task to `LinkHistory.Record`; `linkstore` writes the row and enqueues the task in one `pgx.Tx`.
- **Tasks are an inbound transport** - `internal/api/tasks` is as thin as `internal/api/http` and `internal/api/grpc`: decode, call the domain, map the error to a status code (`503` retried, `400` dead).
- **One schema history** - `link_checks` and the TaskBox tables are migrations `00001` and `00002` of the service; the TaskBox DDL is the contract's, verbatim.
- **One pool** - `main.go` builds one `pgxpool.Pool` for `linkstore` and `pgstore`; the TaskBox worker runs in the servers' errgroup and drains in-flight runs on shutdown.
- **Re-checks queue per URL** - the task's group is the normalized URL, so two re-checks of one link never overtake each other; different links re-check in parallel.

# Capabilities
- api — unchanged from GW007.001: `POST /v1/links/check`, `GET /v1/links/recent`, gRPC `Check` / `RecentChecks`, `GET /health`.
- domain
  - `Check` additionally schedules `recheck-flagged-link` (payload `{"url": …}`, group = normalized URL, run at now + `RECHECK_AFTER`) for a flagged verdict.
  - `Recheck(ctx, url)` bypasses the cache, refreshes it, and records the fresh verdict.
- deferred work (VP-C003)
  - `internal/taskbox`: task values, handler registry, VP-C004 classification, worker pool with lease-bounded handlers and attempt fencing.
  - `internal/taskbox/pgstore`: contract schema v1 — enqueue in the caller's `pgx.Tx` with the group lock, `SKIP LOCKED` claim, requeue/cancel, retention cleanup.
  - Settings: `TASKBOX_WORKERS` (2), `TASKBOX_LEASE` (5m), `TASKBOX_POLL_INTERVAL` (1s), `RECHECK_AFTER` (1h).
- schema — `linkstore.Migrate` (goose, session-locked); `cmd/migrate` for Job mode, `MIGRATE_ON_START=true` for single-instance deployments.
- testing — `make test-kind-unit` runs the domain scenarios and the TaskBox conformance feature; the latter needs `TEST_DATABASE_DSN`.

# Usecases

## A flagged link is re-checked later
[[./diagrams/recheck-flagged-link.mmd|diagrams/recheck-flagged-link.mmd]] — sequence: `Check` → cache miss → reputation flags → `Record(entry, task)` in one transaction → after `RECHECK_AFTER` a worker claims the task → handler → `Recheck` → reputation (`503` → retried after backoff) → fresh verdict recorded.

## Reputation service down during a re-check
The handler maps `ErrUnavailable` to `503`; TaskBox reschedules after `max(1s × 2^attempt, Retry-After)`; the URL's group waits; after 10 attempts the task is `dead` and the group stops until `pgstore.Store.Requeue`/`Cancel` is called.

## Deploy with migrations
Job mode: `go run ./cmd/migrate` (or its container) runs before the new service version; the service never migrates. Single instance: `MIGRATE_ON_START=true` instead — never both ([[library/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]).

# Structure
See `structure/` — this plateau's own copy of every file skill.
- New: [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-domain-interfaces-task.skill.md|file-domain-interfaces-task]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-api-tasks.skill.md|package-api-tasks]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-api-tasks-handlers.skill.md|file-api-tasks-handlers]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-infrastructure-linkstore-migrations.skill.md|file-infrastructure-linkstore-migrations]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-cmd-migrate.skill.md|package-cmd-migrate]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-cmd-migrate-main.skill.md|file-cmd-migrate-main]].
- Extended: [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-infrastructure-linkstore-store.skill.md|file-infrastructure-linkstore-store]] (pool injected, data + tasks in one tx), [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-infrastructure-linkstore.skill.md|package-infrastructure-linkstore]] (migrations), [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-domain-interfaces-link-history.skill.md|file-domain-interfaces-link-history]] (`Record` takes tasks), [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-domain-interfaces.skill.md|package-domain-interfaces]] (`task.go`), [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-domain-services-linkcheck.skill.md|file-domain-services-linkcheck]] (re-check scheduling, `Recheck`), [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-config-config.skill.md|file-config-config]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-cmd-service-main.skill.md|file-cmd-service-main]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--repo-gw009-001.skill.md|repo-gw009-001]].
- Not described here: `internal/taskbox` (with `pgstore/`, `features/`, `test/`) is the **pre-release copy of the `taskbox-go` library** — its source of truth is that repository ([[library/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]); at `taskbox-go` v0.1.0 the example drops the copy and depends on the module.
- Changed on 2026-10-12 with GW007.001 (issue #206, the catalog follows `api-develop`): [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-api-http-server.skill.md|file-api-http-server]] and [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-api-http.skill.md|package-api-http]] (the HTTP form over the contract's generated messages), new [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-api-http-docs.skill.md|file-api-http-docs]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-api-http-protojson.skill.md|file-api-http-protojson]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-docs-integration-api.skill.md|package-docs-integration-api]], [[library/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-docs-integration-client.skill.md|package-docs-integration-client]]; the repo layout and `file-config-config` (`HTTP_API_DOCS_ENABLED`, `HTTP_BASE_PATH`).
- Unchanged since GW007.001 otherwise: the gRPC adapter and its package, the reputation client and cache, `reputation.go`, `reputation_cache.go`, `package-domain-services`, logging, version.

# Registry
Element intersections this plateau adds or grows — see each entry's growth history:
- [[library/go/architecture/registry/internal-infrastructure-store-store-go.md|internal-infrastructure-store-store-go]] (new, N=3: persistent-db, go-db-migrations, taskbox-in-go)
- [[library/go/architecture/registry/internal-infrastructure-store.md|internal-infrastructure-store]] (new, N=3)
- [[library/go/architecture/registry/internal-domain-interfaces-store-port-go.md|internal-domain-interfaces-store-port-go]] (new, N=2)
- [[library/go/architecture/registry/cmd-service-main-go.md|cmd-service-main-go]] (N=9), [[library/go/architecture/registry/internal-config-config-go.md|internal-config-config-go]] (N=9), [[library/go/architecture/registry/repo-root.md|repo-root]] (N=5), [[library/go/architecture/registry/internal-domain-services-service-go.md|internal-domain-services-service-go]] (N=5)
- Unchanged: [[library/go/architecture/registry/internal-api-http-server-go.md|internal-api-http-server-go]], [[library/go/architecture/registry/internal-api-grpc-server-go.md|internal-api-grpc-server-go]] — TaskBox adds no field to any HTTP/gRPC response.
- [[library/go/architecture/registry/docs-integration-api-contract.md|docs-integration-api-contract]] (N=2: `solution-grpc-api` replaces the baseline's hand-written spec by the proto file)
- [[library/go/architecture/registry/internal-api-http-server-go.md|internal-api-http-server-go]] gained `solution-grpc-api` on 2026-10-12 — its two `FMC` pairings are resolved inside the extending solutions, see that entry.

# Ground truth
`examples/` evolved from GW007.001's, verified on 2026-09-28:
- `go build ./...`, `go vet ./...` — clean.
- `TEST_DATABASE_DSN=postgres://…/taskbox_test make test-kind-unit TEST_RUN_PURPOSE=report` — 48/48 tests: 15 domain scenarios (5 new: a flagged check carries a re-check task, a clean check carries none, `Recheck` records a fresh verdict, fails while the reputation service is down, rejects a non-http(s) URL) and 30 TaskBox conformance scenarios on PostgreSQL 18. The `@store-transient` scenario is `missing` in the report — no VP-C002 TaskBox store yet.
- Targeted mutations: group lock removed → the commit-order scenario fails 10/10; `attempt` fence removed → the lease scenario fails 3/3.
- `make test-kind-mutation` (re-run 2026-10-08, after the mutation kind got `gremlins --integration`): 79 mutants killed, none lived, 2 timed out, 60 in code no scenario reaches — score 56%, about ten minutes with PostgreSQL. Before that change gremlins ran only the mutated package's own tests while the godog runners live in `test/` subpackages, and the run proved nothing (3 killed, 78 lived).
- Runtime smoke test (real PostgreSQL, Redis, a throwaway fake reputation gRPC server, `RECHECK_AFTER=2s`): `cmd/migrate` applied versions 1–2; a flagged check wrote its history row and a pending task (group = URL, delay 2 s) in one transaction, a clean check none; after 2 s the re-check got `503` and was retried 2 s later (1 s × 2¹), then succeeded — the fresh verdict appeared in `link_checks`, `GET /v1/links/recent`, and the Redis cache. A task still pending when the service was killed ran after the restart.

API contract, verified 2026-10-12 after the move onto `api-develop` (issue #206):
- `TEST_DATABASE_DSN=… make test-and-report` against a real PostgreSQL 17 — green: 66 tests, 48 scenarios; the `generated` kind up to date; mutation 96 killed / 0 lived / 2 timed out / 56 not covered.
- The HTTP feature stubs the reputation port, the cache, and the history store (whose `Record` takes the tasks and drops them).
- Not re-run after the move: the runtime smoke test with Redis and a fake reputation server above.

To run it yourself: `cd examples && go mod tidy && make build && DATABASE_DSN=… REPUTATION_ADDR=… go run ./cmd/migrate && TEST_DATABASE_DSN=… make test-kind-unit`.
