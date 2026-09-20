---
name: solution-go-db-migrations
description: Versioned, embedded schema migrations for solution-persistent-db's store, applied by exactly one of two modes — a standalone cmd/migrate job, or a config-gated call in cmd/{service}/main.go's own startup — chosen per deployment platform, never both
whenToUse: when a module applying solution-persistent-db needs its schema versioned and changeable across releases instead of a single inline CREATE TABLE IF NOT EXISTS
domain: skill
type: architecture
version: 20260920000001
tags:
  - skill/architecture/solution
  - solution/go-db-migrations
  - stack/go
  - concern/architecture
creates:
  - "internal/infrastructure/{store}/migrations.go"
  - "internal/infrastructure/{store}/migrations/"
  - "cmd/migrate/"
extends:
  - "internal/infrastructure/{store}/"
  - "internal/infrastructure/{store}/store.go"
  - "internal/config/config.go"
  - "cmd/{service}/main.go"
depends_on:
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
built_on_plateau:
adr:
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/adr/migration-tool-choice.md|Migration tool choice]]"
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/adr/job-only-not-startup-run.md|Deploy-time job only, never startup-run (superseded)]]"
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/adr/migration-mode-per-platform.md|Two modes, chosen per deployment platform]]"
---

# Goal
- Give `solution-persistent-db`'s PostgreSQL adapter a versioned, single-source-of-truth schema
  history, applied by exactly **one** of two supported modes — never both — chosen per deployment
  platform: **Job mode** (a standalone `cmd/migrate` binary, run as a one-shot deploy-time
  job/container ahead of the app) or **MigrateOnStart mode** (`cmd/{service}/main.go` calls
  `Migrate` once, guarded by a config flag, before serving traffic) — instead of the baseline's
  inline `CREATE TABLE IF NOT EXISTS` re-run on every connect.

# Capabilities
- The store's schema has an ordered, reviewable history (`migrations/*.sql`) instead of one
  hand-maintained idempotent statement — the single place describing the schema and every change
  made to it since, regardless of which mode applies it.
- A platform whose deploy tooling can natively gate a workload on a job's completion (Kubernetes via
  Helm hooks) gets that guarantee with no extra deploy step.
- A platform where the deployed instance is known to be single — never more than one concurrent
  process attempting the same migration — can use the simpler MigrateOnStart mode without the
  concurrency risk that mode carries at scale (see this solution's own ADR).

# Core Principles
- **Exactly one mode per deployment, chosen by `MigrateOnStart` (`MIGRATE_ON_START` env var,
  default `false`), never both.** Job mode: `cmd/migrate` runs, `cmd/{service}/main.go` never calls
  `Migrate`. MigrateOnStart mode: `cmd/{service}/main.go`'s guarded call runs, `cmd/migrate` exists
  but is never wired into that deployment's pipeline. Running both against the same deployment is
  not a data-corruption risk (goose's session lock still protects it) but reintroduces the
  concurrency/masking problems documented in
  [[./adr/migration-mode-per-platform.md|adr/migration-mode-per-platform.md]] — never do it.
- **MigrateOnStart mode is safe only when at most one instance of the migrating process can ever
  run concurrently for that deployment.** The moment a deployment can run more than one replica of
  the process that would call `Migrate`, Job mode is required — see the ADR's decision matrix.
  This solution does not, and cannot, enforce that condition itself; the deploying platform's own
  config (a replica count, a scaling policy) is what makes it true or false, and
  `devops-service-deploy.skill.md`'s own "migration step" rule states the condition, not a
  platform-unconditional default.
- Depends on, and always pairs with, `solution-persistent-db` — this is how `PersistentDb` (VP7)
  manages its schema once a team wants versioning, not a Variation Point of its own;
  `variability-map.md`'s VP7 row names this solution alongside `solution-persistent-db` rather than
  adding a new row.
- SQL-only, versioned migration files are the schema's single source of truth — no ORM
  auto-migrate, no declarative-diff tool (see the tool-choice ADR's rejected Atlas variant for why).

# Boundaries
- This solution wires `Up()` only — it prescribes no rollback *process* beyond goose's own `Down()`
  capability being available; a real consumer decides its own rollback policy (roll forward with a
  fix vs. running `Down`) for itself.
- This solution does not verify that a deployment's actual replica/instance count matches its
  `MIGRATE_ON_START` setting — that correctness depends on the deploying platform's own
  configuration (a Stack `deploy.replicas`, a Kubernetes `Deployment`'s `replicas`, a scaling
  policy), which this solution has no visibility into at build time. State the chosen mode, and the
  replica-count fact that makes it safe, explicitly in that plateau's own deploy config — see
  [[skills/devops/devops-service-deploy.skill/devops-service-deploy.skill.md|devops-service-deploy]]'s
  own "migration step" rule.

# Adr
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/adr/migration-tool-choice.md|Migration tool choice]]
  - Selected variant: `github.com/pressly/goose/v3`, over a plain `*sql.DB` opened via
    `pgx.ParseConfig` + `pgx/v5/stdlib.OpenDB`, with Postgres session-level advisory locking
    (`goose.WithSessionLocker`) and a `go:embed`'d `migrations/` directory
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/adr/migration-mode-per-platform.md|Two modes, chosen per deployment platform]]
  - Selected variant: both Job mode and MigrateOnStart mode are supported, switched by one config
    flag; `devops-service-deploy.skill.md` states which platforms/topologies use which —
    **supersedes** [[skills/go/architecture/solutions/solution-go-db-migrations.skill/adr/job-only-not-startup-run.md|the earlier job-only decision]]

# Requirements
SOLUTION:
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]
  - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/infrastructure/{store}/store.go.create.md|store.go]] - the file this solution's `store.go.extend.md` extends
  - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/cmd/{service}/main.go.extend.md|main.go]] - the file this solution's own `main.go.extend.md` further extends
GO MODULES:
- github.com/pressly/goose/v3 v3.28
  - `goose.NewProvider`, `(*Provider).Up` — the migration runner
  - `goose.WithSessionLocker` + `lock.NewPostgresSessionLocker` — concurrency-safe locking (opt-in;
    see the tool-choice ADR) — defense in depth against a retried/duplicated run in either mode, not
    the primary safety mechanism (that is choosing the correct mode for the deployment's topology)
- github.com/jackc/pgx/v5/stdlib
  - `stdlib.OpenDB` — bridges a `pgx.ConnConfig` into the `*sql.DB` `goose.NewProvider` requires,
    without pulling in `database/sql`'s `lib/pq` driver

# Template Skill Mutations
FILES:
- [[./Implementation/internal/infrastructure/{store}/Package.extend.md|internal/infrastructure/{store}]] - extend - schema ownership moves from inline DDL to versioned migrations
- [[./Implementation/internal/infrastructure/{store}/migrations.go.create.md|migrations.go]] - create - `Migrate` + the embedded `migrations/` directory
- [[./Implementation/internal/infrastructure/{store}/store.go.extend.md|store.go]] - extend - `New` stops creating the table inline
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - add `MigrateOnStart`, the one flag selecting the mode
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - `MigrateOnStart`-guarded call — this solution's MigrateOnStart-mode call site
- [[./Implementation/cmd/migrate/Package.create.md|cmd/migrate]] - create - this solution's Job-mode call site's package
- [[./Implementation/cmd/migrate/main.go.create.md|main.go]] - create - the Job-mode binary's `main`

# Workflow

## Job mode
1. The deploy pipeline runs `cmd/migrate` as a one-shot job/container, per
   `devops-service-deploy.skill.md`'s own manifests for the platform in use — typically a
   Kubernetes `Job` via a Helm `pre-install,pre-upgrade` hook (see that skill's
   [`templates/helm/chart-example/templates/migrate-job.yaml`](skills/devops/devops-service-deploy.skill/templates/helm/chart-example/templates/migrate-job.yaml)),
   or a plain-manifest `kubectl apply` + `kubectl wait` step.
2. `cmd/migrate`'s `run()` calls `{store}.Migrate(ctx, dsn)`, which applies every migration newer
   than the database's recorded version, or no-ops if already current.
3. The deploy pipeline proceeds to (re)start the app only if the job/container exits `0`.
4. `MIGRATE_ON_START` is `false` (the default) for this deployment — `cmd/{service}/main.go` never
   calls `Migrate`; `{store}.New` simply connects to an already-migrated schema.

## MigrateOnStart mode
1. `MIGRATE_ON_START=true` is set for this deployment — a topology where this solution's own ADR
   confirms at most one instance of `cmd/{service}` ever runs concurrently.
2. `cmd/{service}/main.go`'s `run()` calls `{store}.Migrate(ctx, cfg.DatabaseDSN)` before
   constructing `Store`, exactly once per process start.
3. `run()` constructs the `Store` only after `Migrate` succeeds.
4. `cmd/migrate` is never invoked for this deployment — it exists in the repository (available if
   the deployment's topology ever changes and needs Job mode instead) but nothing wires it in.

## Defense in depth: a retried or duplicated run within one mode
1. Something retries a migration run while another is still in flight — in Job mode, a deploy
   pipeline's own retry logic; in MigrateOnStart mode, an unusually fast process restart racing a
   still-in-flight previous start. A pipeline/operational accident this solution does not assume
   away, not a sign the wrong mode was chosen.
2. goose's session-level advisory lock (`lock.NewPostgresSessionLocker`, wired via
   `goose.WithSessionLocker`) blocks the second caller before it runs.
3. The second caller resumes once the first's `Up()` completes, finds no pending migrations left,
   and returns with nothing applied.
4. No concurrent run observes a partially-applied migration or races another run on goose's own
   migration-tracking table.

# Rules

## MUST
- [[./Implementation/internal/infrastructure/{store}/migrations.go.create.md#MUST|migrations.go]]
- [[./Implementation/internal/infrastructure/{store}/store.go.extend.md#MUST|store.go]]
- [[./Implementation/internal/config/config.go.extend.md#MUST|internal/config/config.go]]
- [[./Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]
- [[./Implementation/cmd/migrate/main.go.create.md#MUST|cmd/migrate/main.go]]

# Check list
- [ ] `{store}`'s schema exists only via `migrations/` — no inline `CREATE TABLE` remains in
      `store.go`.
- [ ] Exactly one mode is wired for a given deployment: either `cmd/migrate` is invoked by the
      deploy pipeline with `MIGRATE_ON_START` left `false`, or `MIGRATE_ON_START=true` is set and
      `cmd/migrate` is never invoked for that deployment — never both, never neither.
- [ ] Wherever `MIGRATE_ON_START=true` is set, that deployment's own config states, explicitly, that
      at most one instance of `cmd/{service}` can run concurrently.
- [ ] `github.com/pressly/goose/v3` (and its subpackages) is imported only from
      `internal/infrastructure/{store}/migrations.go`.
