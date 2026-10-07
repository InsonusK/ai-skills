---
name: plateau-gw009-001
description: GW009.001 — persistent service with TaskBox; plateau-persistent-service (GW007.001) plus versioned goose migrations and a PostgreSQL TaskBox (VP-C003) that re-checks every flagged link later, enqueued in the same transaction as its history record
whenToUse: when a Go web-service with PostgreSQL must run follow-up work after a data change — retried, ordered per key, and atomic with the change — or when reviewing a Go service's TaskBox wiring against the VP-C003 contract
domain: skill
type: template
version: 20260928120000
updated: 20260929
tags:
  - skill/template/plateau
  - plateau/plateau-gw009-001
  - stack/go
  - concern/architecture
parent_plateaus:
  - "[[skills/go/architecture/plateau/plateau-persistent-service/plateau-persistent-service.skill/plateau-persistent-service.skill.md|plateau-persistent-service]]"
created_by:
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
standalone: true
registry:
  - "[[skills/go/architecture/registry/cmd-service-main-go.md|cmd-service-main-go]]"
  - "[[skills/go/architecture/registry/internal-config-config-go.md|internal-config-config-go]]"
  - "[[skills/go/architecture/registry/repo-root.md|repo-root]]"
  - "[[skills/go/architecture/registry/internal-domain-services-service-go.md|internal-domain-services-service-go]]"
  - "[[skills/go/architecture/registry/internal-api-http-server-go.md|internal-api-http-server-go]]"
  - "[[skills/go/architecture/registry/internal-api-grpc-server-go.md|internal-api-grpc-server-go]]"
  - "[[skills/go/architecture/registry/internal-infrastructure-store-store-go.md|internal-infrastructure-store-store-go]]"
  - "[[skills/go/architecture/registry/internal-infrastructure-store.md|internal-infrastructure-store]]"
  - "[[skills/go/architecture/registry/internal-domain-interfaces-store-port-go.md|internal-domain-interfaces-store-port-go]]"
---

# Goal
- The GW007.001 service (HTTP + gRPC, reputation client, Redis cache, PostgreSQL history) whose schema is a versioned goose history, applied by `cmd/migrate` or at startup.
- A PostgreSQL TaskBox realizing the VP-C003 contract through the `taskbox-go` library (pre-release copy in `internal/taskbox` until v0.1.0), schema v1 as migration `00002`.
- A flagged check recorded **together with** a delayed `recheck-flagged-link` task in one transaction; the task handler re-asks the reputation service and records the fresh verdict.
- The shared TaskBox conformance feature passing on a real PostgreSQL.

Code `GW009.001`: Go web-service, common combination 009 of the [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/registry/web-service-common-plateaus|common-plateau registry]] (PostgreSQL, Redis, TaskBox, GrpcOutbound), stack VPs 001 (VP1 GrpcApi).

# Core Principles
- Everything [[skills/go/architecture/plateau/plateau-persistent-service/plateau-persistent-service.skill/plateau-persistent-service.skill.md|plateau-persistent-service]] states still holds: ports-and-adapters, one composition root, thin inbound adapters, godog scenarios, cache failures degrade, history failures fail the request.
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
Job mode: `go run ./cmd/migrate` (or its container) runs before the new service version; the service never migrates. Single instance: `MIGRATE_ON_START=true` instead — never both ([[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]).

# Structure
See `structure/` — this plateau's own copy of every file skill.
- New: [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-domain-interfaces-task.skill.md|file-domain-interfaces-task]], [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-api-tasks.skill.md|package-api-tasks]], [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-api-tasks-handlers.skill.md|file-api-tasks-handlers]], [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-infrastructure-linkstore-migrations.skill.md|file-infrastructure-linkstore-migrations]], [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-cmd-migrate.skill.md|package-cmd-migrate]], [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-cmd-migrate-main.skill.md|file-cmd-migrate-main]].
- Extended: [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-infrastructure-linkstore-store.skill.md|file-infrastructure-linkstore-store]] (pool injected, data + tasks in one tx), [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-infrastructure-linkstore.skill.md|package-infrastructure-linkstore]] (migrations), [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-domain-interfaces-link-history.skill.md|file-domain-interfaces-link-history]] (`Record` takes tasks), [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--package-domain-interfaces.skill.md|package-domain-interfaces]] (`task.go`), [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-domain-services-linkcheck.skill.md|file-domain-services-linkcheck]] (re-check scheduling, `Recheck`), [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-config-config.skill.md|file-config-config]], [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-cmd-service-main.skill.md|file-cmd-service-main]], [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--repo-gw009-001.skill.md|repo-gw009-001]].
- Not described here: `internal/taskbox` (with `pgstore/`, `features/`, `test/`) is the **pre-release copy of the `taskbox-go` library** — its source of truth is that repository ([[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]); at `taskbox-go` v0.1.0 the example drops the copy and depends on the module.
- Unchanged since GW007.001: the HTTP/gRPC adapters and their packages, the reputation client and cache, `reputation.go`, `reputation_cache.go`, `package-domain-services`, logging, version.

# Registry
Element intersections this plateau adds or grows — see each entry's growth history:
- [[skills/go/architecture/registry/internal-infrastructure-store-store-go.md|internal-infrastructure-store-store-go]] (new, N=3: persistent-db, go-db-migrations, taskbox-in-go)
- [[skills/go/architecture/registry/internal-infrastructure-store.md|internal-infrastructure-store]] (new, N=3)
- [[skills/go/architecture/registry/internal-domain-interfaces-store-port-go.md|internal-domain-interfaces-store-port-go]] (new, N=2)
- [[skills/go/architecture/registry/cmd-service-main-go.md|cmd-service-main-go]] (N=9), [[skills/go/architecture/registry/internal-config-config-go.md|internal-config-config-go]] (N=9), [[skills/go/architecture/registry/repo-root.md|repo-root]] (N=5), [[skills/go/architecture/registry/internal-domain-services-service-go.md|internal-domain-services-service-go]] (N=5)
- Unchanged: [[skills/go/architecture/registry/internal-api-http-server-go.md|internal-api-http-server-go]], [[skills/go/architecture/registry/internal-api-grpc-server-go.md|internal-api-grpc-server-go]] — TaskBox adds no field to any HTTP/gRPC response.

# Ground truth
`example/` evolved from GW007.001's, verified on 2026-09-28:
- `go build ./...`, `go vet ./...` — clean.
- `TEST_DATABASE_DSN=postgres://…/taskbox_test make test-kind-unit TEST_RUN_PURPOSE=report` — 48/48 tests: 15 domain scenarios (5 new: a flagged check carries a re-check task, a clean check carries none, `Recheck` records a fresh verdict, fails while the reputation service is down, rejects a non-http(s) URL) and 30 TaskBox conformance scenarios on PostgreSQL 18. The `@store-transient` scenario is `missing` in the report — no VP-C002 TaskBox store yet.
- Targeted mutations: group lock removed → the commit-order scenario fails 10/10; `attempt` fence removed → the lease scenario fails 3/3.
- `make test-kind-mutation` runs but is not evidence here: gremlins runs only the mutated package's own tests, and the godog runners live in `test/` subpackages (catalog-wide limitation, recorded in the common map's `agent/DECISIONS.md`).
- Runtime smoke test (real PostgreSQL, Redis, a throwaway fake reputation gRPC server, `RECHECK_AFTER=2s`): `cmd/migrate` applied versions 1–2; a flagged check wrote its history row and a pending task (group = URL, delay 2 s) in one transaction, a clean check none; after 2 s the re-check got `503` and was retried 2 s later (1 s × 2¹), then succeeded — the fresh verdict appeared in `link_checks`, `GET /v1/links/recent`, and the Redis cache. A task still pending when the service was killed ran after the restart.

To run it yourself: `cd example && go mod tidy && make build && DATABASE_DSN=… REPUTATION_ADDR=… go run ./cmd/migrate && TEST_DATABASE_DSN=… make test-kind-unit`.
