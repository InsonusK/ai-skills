# skills/go/architecture build decisions log

One line per non-mechanical choice made while building this catalog. `⚠️` marks a genuine
architectural fork that needs the owner's sign-off; everything else is execution against
[[skills/go/architecture/agent/INVARIANTS]] (once written) or, before that exists, against
`feature/feature-model.md`.

## Settled before the build (from the request)

- New catalog, no pre-existing `skills/go/architecture` to reconcile against — built directly at
  `skills/go/architecture/{feature,variability-map.md,solutions,plateau,agent}/`, no `v3.1`-style
  version-prefixed staging folder (that convention exists in the dotnet/angular catalogs only to
  parallel-build against an existing older catalog; nothing to parallel here).
- Base = a Go web-service with no DB, domain-logic separation, HTTP API, tested per
  `cucmber-testing-in-go` + `solution-conformance-testing`.
- Six owner-named options become candidate variable features: gRPC alternative API,
  external integrations, Kafka publish (outbox when tied to a persisted-data change), Kafka
  consume, cached DB, persistent DB.
- **Five plateaus for the first build**, each cumulative on the last: base; +gRPC; +gRPC+external
  integration; + those +cached DB; + those +persistent DB. Kafka publish/consume and the outbox
  pattern are **not** realized by any plateau in this batch — their solutions are authored as
  skeletons (draft contract, no consumer yet), matching the dotnet catalog's VP10–VP14 precedent.
- Reference implementation for ground truth: `tmp/tg-bot-service` (read in full — see
  `agent/README.md`). Its product-specific parts (Telegram channel, access-control allow/deny list)
  are deliberately excluded from the generic catalog.
- Work happens in a dedicated worktree/branch (`go-web-service-plateau-map`) per `work-in-git-tree`,
  branched from `develop`; a PR is opened at the end.

## Stage 1 — Feature Model (this wave)

- Root named `Service`. Common: `DomainLogic`, `HttpApi`, `AppLogging`, `TestConformance` (+4
  children). Variable: `GrpcApi`, `ExternalIntegration`, `CachedDb`, `PersistentDb`,
  `AsyncInboundApi`→`KafkaConsumer`, `AsyncOutboundApi`→`KafkaProducer`+`OutboxPattern`.
- `HttpApi` modeled as common (not a dotnet-style `SyncInboundApi` "at least one of HTTP/gRPC"
  umbrella) — the owner's base always has HTTP; `GrpcApi` is a pure addition. Reasoning in
  `feature-model.md`'s "Modeling choices" section.
- `ExternalIntegration` modeled as **one** feature, not split by transport (dotnet's
  `SyncOutboundApi`→{Http,Grpc}Client shape) — the owner named it as a single option; transport is
  an adapter-level implementation choice. Flagged as a deliberate simplification, not an oversight.
- `CachedDb`/`PersistentDb` independent (no "at least one" pairing, no mutual requirement) — the
  reference has `CachedDb` (Redis) without `PersistentDb`, and the two are orthogonal by the
  owner's own option list.
- `OutboxPattern` kept as its own row (mirrors dotnet's VP14 shape: child of `AsyncOutboundApi`,
  `Requires PersistentDb`) rather than folded into `KafkaProducer`'s description, even though the
  owner's phrasing ("если... то делается через outbox") reads as a required substitution rather
  than an optional upgrade once both preconditions hold. The Feature Model records the Requires
  edge; the *strength* of the rule (required, not merely legal, once a publication is triggered by
  the persisted change) is deferred to the Variability Map's constraint prose, where dotnet's own
  equivalent nuance also lives.
- ⚠️ `AppLogging` marked common by the same judgment-call reasoning the dotnet catalog used for its
  own `AppLogging` (present throughout the one reference implementation examined; not proven
  necessary by construction). Flagged for the owner in the stage-1 summary; proceeding with
  `true` as the sensible default unless corrected.

## Stage 2 — Variability Map

- 7 VPs, numbered to match the owner's own option list (1 grpc, 2 external-integration, 3
  async-outbound/kafka-publish, 4 outbox, 5 async-inbound/kafka-consume, 6 cached-db, 7
  persistent-db) rather than diagram/topological order.
- `AsyncInboundApi`/`AsyncOutboundApi` kept as single VPs with a `Mandatory` `KafkaConsumer`/
  `KafkaProducer` realization, mirroring dotnet's VP12/VP13 — each is 1:1 with today's only
  realization, not an "at least one of N" group.
- `OutboxPattern` (VP4) kept as its own VP (not folded into VP3's description), Constraint `VP3=Yes
  AND VP7=Yes`, mirroring dotnet's VP14 shape exactly. The owner's stronger "required, not merely
  legal, once triggered by a persisted change" framing is recorded in the VP4 note, not encoded as
  a structural constraint (the map records legality; strength-of-rule is the realizing solution's
  own Boundaries).
- No categorical (multi-variant) VP — every row is boolean; noted explicitly in Out of scope in
  case a second broker/transport realization later needs one.

## Stage 3 — solutions + delta-conflict-detection

- **No `templates/go/` existed for `solution-create` or `plateau-create-by-solutions`** — built one
  for each, adapted from `templates/python/` (closest analog: simpler baseline than dotnet, no
  per-project ceremony) cross-checked against `templates/dotnet/` for the parts python's *source*
  tree is missing (confirmed by direct `find` against source — **not** trusted from the stale
  `.claude/skills/` sync mirror, which still shows an `adr/` subfolder under
  `solution-{Solution}.skill.template/` that no current stack (dotnet/python/typescript source)
  actually has; ADRs are created ad hoc from `adr-create.skill.md`'s own single shared template,
  not a per-stack one — do not recreate that stale `adr/` folder for Go either).
  - `solution-create.skill/templates/go/`: 4 Implementation-file kinds — `Repository` (repo root),
    `Package` (a Go package directory, mirrors python's package / dotnet's Project), `Struct` (a
    file organized around one struct + methods, mirrors dotnet/python's `Class`), `Functions` (a
    file of standalone functions/interfaces/vars — needed because the reference's own
    `domain/interfaces` and `config`/`logging` files are exactly this shape, not struct-shaped).
    No `init` kind (Python-specific `__init__.py` machinery; no Go analog).
  - `plateau-create-by-solutions.skill/templates/go/`: `plateau-{name}`, `repo-{name}`,
    `package-{name}`, and one merged `file-{name}` (aggregates both `Struct`/`Functions` outputs,
    mirroring how python's single `module-{name}` aggregates `Class`/`functions`/`init`).
  - Registered Go in `plateau-create-by-solutions.skill.md` itself (Scope line, `{stack}` example
    list, the stack-detection template-folder list, a new Go `Implementation/` file-pattern table,
    and Go package/file naming-normalization rows) — the same kind of extension the Angular build
    made to this skill (see `[[v31-angular-plateau-build]]` memory, commit `0e2be8cd`).
- **Planned solutions (12), not yet all authored** — common baseline: `solution-go-repository-structure`,
  `solution-go-domain-logic`, `solution-go-http-api`, `solution-go-app-logging`,
  `solution-conformance-testing-in-go` (extends the shared
  `skills/common-workflow/test/solution-conformance-testing.skill`, mirroring the
  `ts`/`python`/`dotnet` per-stack extensions). VP-realizing: `solution-grpc-api` (VP1),
  `solution-external-integration` (VP2), `solution-go-messaging-infrastructure` +
  `solution-go-kafka-producer` (VP3, skeleton), `solution-go-transactional-outbox` (VP4, skeleton),
  `solution-go-kafka-consumer` (VP5, skeleton), `solution-cached-db` (VP6), `solution-persistent-db`
  (VP7). Expect most intersections to classify `-N-` (disjoint `internal/` packages per solution) —
  confirm once actually authored, and iterate delta-conflict-detection's grouping pass for real
  before treating any VP's `Realized by` cell as final.
- ⚠️ **Mutation-tool-per-stack ADR to be updated** (`skills/common-workflow/test/
  solution-conformance-testing.skill/adr/mutation-tool-per-stack.md`) to add Go: **gremlins**
  (`github.com/go-gremlins/gremlins`) — confirmed from `tmp/tg-bot-service`'s own `Makefile`
  (`mutation-test` target), not invented. This is a shared cross-stack file outside
  `skills/go/architecture/` — the same kind of carve-out INVARIANTS.md will record (mirrors
  dotnet's INVARIANTS.md §4 carve-out for its own two allowed external `depends_on` targets).

**All 14 solutions authored** (commits `d3365f5b`, `45de25c1`, `d2784c06`, `00f6127b`): the 5
common-baseline + `solution-go-domain-ports` (shared prereq) + `solution-grpc-api` (VP1) +
`solution-external-integration` (VP2) + `solution-cached-db` (VP6) + `solution-persistent-db`
(VP7) full; `solution-go-messaging-infrastructure` + `solution-go-kafka-producer` (VP3) +
`solution-go-transactional-outbox` (VP4) + `solution-go-kafka-consumer` (VP5) skeletons. Every
`variability-map.md` VP row's `Realized by` cell now points at a real solution.

**Delta-conflict-detection classification pass DONE** — grouped every `Implementation/` file by its
`element/*` tag (44 files, mechanical `grep` pass, not eyeballed). Four real (2+ solution) groups,
all **canonical FMN** (no constraint between the intersecting VPs, independent additive code
changes) — **no resolver solutions needed anywhere in this catalog**:
- `cmd-service-main-go` (N=7: go-app-logging, go-http-api, grpc-api, external-integration,
  cached-db, persistent-db, kafka-consumer) — each solution appends its own construct/wire/defer
  block to `run()`; the final `services.New{Service}(...)` call must combine every applied port
  solution's argument, which is assembly-time judgment (matching dotnet's `pipelineregistration-cs`
  N≥5 canonical precedent), not a mechanical conflict.
- `internal-config-config-go` (N=7, same solutions) — each appends its own `Config` field(s); field
  order in a Go struct literal is not order-sensitive, so this is the easiest of the four groups.
- `internal-domain-services-service-go` (N=4: go-domain-logic[create] + external-integration +
  cached-db + persistent-db) — each appends a constructor parameter; already explicitly
  cross-referenced between the three `.extend.md` files' own MUST rules at authoring time.
- `repo-root` (N=4: go-repository-structure[create] + go-conformance-testing + grpc-api +
  external-integration) — mostly disjoint additions (proto/buf dirs vs. Makefile testing targets),
  except `Makefile`'s `proto-gen` target specifically, which `solution-external-integration`'s own
  Rule already handles ("if the target already exists, extend its recipe body, never redeclare it").
- Every other `element/*` tag has exactly one solution touching it (`-N-`, no shared artifact).
- **No `.create` double-authors any element** — checked as part of the same pass.
- Per [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]]'s own placement rule, **Registry entries are not written yet** — a Registry entry lives at "the shallowest plateau where every intersecting solution is simultaneously present," and no plateau exists yet. All four groups reach N≥3, so each will carry the architectural-signal note once written. Write them during Stage 4, per solution combination, not here.

**Stage 3 COMPLETE.** Next: `agent/INVARIANTS.md` + `check.sh` (the harness anchor — the VP→
solution mapping is now final), then Stage 4 (5 plateaus).

**Stage 4 COMPLETE — all 5 plateaus built, each ground-truth verified, one Go module
(`github.com/example/linkcheck-service`) grown incrementally rather than rewritten per plateau**:
- `plateau-http-service` (base, commit `de69fd2a`), `plateau-dual-api-service` (+VP1, `ed8557f3`),
  `plateau-integrated-service` (+VP2, `3f3f79e9`), `plateau-cached-service` (+VP6, `cb4a93df`),
  `plateau-persistent-service` (+VP7, `93640fa1`) — matching the owner's own specified sequence
  exactly ("база + 1 → +1+2 → +1+2+5 → +1+2+5+6"), Kafka (VP3/VP4/VP5) deliberately excluded from
  this batch per the owner's own scoping message.
- Real bugs found and fixed **upstream in the catalog**, not just worked around locally, while
  ground-truth building (every one required actually running `go build`/`go vet`/`make unit-test`/
  `make mutation-test`/a real network smoke test — none would have surfaced from reading the
  solution skills alone):
  1. `godog.Options` needs an explicit `Format: "pretty"` — fixed in
     `skills/go/testing/cucmber-testing-in-go.skill.md` itself (a pre-existing skill, not authored
     in this build), since its own documented example carried the same latent bug.
  2. `solution-go-repository-structure`'s `main.go` was bundled into the repo-tier
     `Repository.create.md` while every solution that extends it expects a file-tier
     `Functions.extend.md` target — split `main.go` into its own file-tier
     `Implementation/cmd/{service}/main.go.create.md`.
  3. A `proto/{service}/v1/{service}.proto` path combined with a flat `go_package` breaks
     `buf generate`'s import path — fixed by keeping the proto flat in both
     `solution-grpc-api`/`solution-external-integration`, and switched `buf.gen.yaml` to `local:`
     plugins for no network dependency.
  4. `solution-external-integration` extended the domain result type but never told an agent to
     extend the HTTP/gRPC adapters to surface the new fields — added both
     `internal/api/{http,grpc}/server.go.extend.md` files.
  5. The exact same class of gap as #4, found again in `solution-persistent-db` while building
     `plateau-persistent-service`: no adapter-extension files at all, and `{service}.go.extend.md`
     never showed the `RecentChecks` read method its own prose already promised. Fixed the same way
     — added both adapter extend files and the missing domain-service method.
  6. A catalog-wide `grep` for every `element/*` tag (done while building plateau 5, prompted by
     adding persistent-db's new adapter-extension files and wanting to place its registry entry
     correctly) found `internal-api-http-server-go`/`internal-api-grpc-server-go` had already
     reached N=2 (create + `external-integration` extend) back at `plateau-integrated-service` with
     **no registry entry at all** — a real miss in this build's own earlier Stage-4 work, not a new
     regression. Fixed retroactively: two registry entries added at their real shallowest plateau
     (`plateau-integrated-service`), that plateau's root skill updated (`registry:` property +
     "# Registry" section, "Four intersections" → "Six"), before writing plateau 5's own N=3
     versions of the same two entries.
- Registry findings worth keeping past this build: `{external-integration, cached-db}` on
  `internal-domain-services-service-go` is a borderline `FMC` defused by explicit cross-solution
  documentation rather than a `depends_on` edge (recorded at `plateau-cached-service`); the same
  element's `{*, persistent-db}` pairing was **confirmed, not assumed**, to stay plain `FMN` by
  reading `Check`'s actual body — `history.Record` runs strictly after the cache-aside call
  returns, a pure append, never a wrap. General rule extracted for future solutions touching this
  element: a solution that **reads an already-computed value and appends independent work** stays
  `FMN` regardless of how many accumulate; only a solution that **wraps or relocates** an existing
  call risks the borderline case.
- `skills/go/architecture/plateau/plateau-repository.md` (Stage 5, `plateau-map-create`) built from
  the finished `variability-map.md` and all 5 plateaus' `created_by`/`parent_plateaus`: 5×7 matrix,
  all `standalone: true`, one linear lineage chain. Constraint check clean (VP4's `requires VP3 AND
  VP7` is vacuously satisfied — no plateau realizes VP4). Recorded three concrete
  legal-but-unbuilt combinations for future work: any VP3/VP4/VP5 (Kafka, out of scope this batch),
  `VP1=No` with any of VP2/VP6/VP7 (no plateau branches off `plateau-http-service` directly), and
  `VP7` without `VP6` (this catalog's chain only ever adds `PersistentDb` on top of `CachedDb`,
  never alone — unlike the reference `tmp/tg-bot-service`, which the owner described as having a DB
  that is cached **and** persistent simultaneously, matching `plateau-persistent-service` exactly).
- No top-level `skills/go/architecture/README.md` added — checked the dotnet catalog for precedent
  first; it has no catalog-root README either, only `agent/README.md` (the harness's own
  scaffolding readme), which this catalog already has.

**Stages 4–5 COMPLETE. Pipeline finished for this build's scope** (base + VP1 + VP2 + VP6 + VP7,
5 plateaus). Remaining, deliberately out of scope for this batch per the owner's own message:
VP3/VP4/VP5 (Kafka publish/consume + outbox) stay skeleton solutions with no realizing plateau.

## Follow-up — solution-go-db-migrations (closes solution-persistent-db's own Boundaries gap)

Added after the build above, in a separate worktree (`.ai-worktree/go-db-migration-solution`),
triggered by the owner asking why `solution-persistent-db`'s adapter uses a bare
`CREATE TABLE IF NOT EXISTS` instead of a migration library, then asking for a real solution: a
tool that supports running once at service startup *or* once as a separate deploy-time job, with one
versioned source of truth for the schema.

**Process correction, recorded because it is worth not repeating:** the first pass modeled this as a
new "VP8" row and edited `feature-model.md`/`diagrams/feature-diagram.mmd`/`variability-map.md`/
`plateau/plateau-repository.md` before the owner had confirmed the migration tool — the owner
stopped this, reverted those edits (`git checkout --`), and corrected two things: (1) the tool choice
must be agreed first, not decided and then presented; (2) this is **part of VP7**, not a new VP — a
team never chooses "migrations: yes/no" independently of `PersistentDb` itself. Both are now the
standing pattern for any future addition to this catalog that extends an existing VP's realization.

**Tool choice, verified rather than assumed:** a first internal pass leaned `golang-migrate`; the
owner pushed back that it might be unmaintained ("заморожен"). Checked for real via the Go module
proxy (`@latest`/`@v/list`) and the GitHub API rather than trusting either direction — neither
project is stalled (`golang-migrate` v4.20.1 released 2026-09-09, `goose` v3.28.0 released
2026-09-02, both un-archived). Both were also fetched into scratch modules and compiled against this
solution's actual shape (pgx, `go:embed`, Postgres locking). Real, measured differences favored
**goose**: lighter `go.sum` footprint (27 vs. 87 lines in a minimal scratch module), and its session
locker (`goose.WithSessionLocker(lock.NewPostgresSessionLocker())`, opt-in) matches
`golang-migrate`'s default-on advisory lock once explicitly enabled — recorded with the full
comparison table in `solution-go-db-migrations.skill/adr/migration-tool-choice.md`. Checked whether
Google publishes any recommendation between the two (Cloud SQL docs, Go style guide, golang/go
wiki) — found none; "Database Migration Service" on `cloud.google.com` is an unrelated
heterogeneous-engine migration product, not a schema-versioning library recommendation.

**Structure:** kept as a second solution skill (`solution-go-db-migrations`, `depends_on`
`solution-persistent-db`) rather than merged into `solution-persistent-db` directly — owner's
explicit choice, mirroring VP3's existing two-solutions-one-VP precedent
(`solution-go-messaging-infrastructure` + `solution-go-kafka-producer`). Unlike VP3's pairing it is
**not** mandatory whenever VP7 is `Yes`: `plateau-persistent-service` (built before this solution
existed) composes `solution-persistent-db` alone and stays a legal VP7 realization; see
`variability-map.md`'s own "`solution-go-db-migrations` is part of VP7, not a new VP" note for why
mandatory pairing was rejected (it would make the map disagree with that plateau's own already-
verified `created_by`).

**Ground truth:** `Migrate`'s exact code (goose provider + session locker + `pgx/v5/stdlib` bridge +
`go:embed`) was compiled and `go vet`-ed against the real `github.com/pressly/goose/v3 v3.28.0`
release in a throwaway scratch module before being written into the solution's Implementation files
— not assumed from documentation. No plateau retrofitted to compose it (see `variability-map.md`'s
own note); that remains future work, same as VP3–VP5.

### Second follow-up — dropped the startup-run call site; migration-job topology moved to devops-service-deploy

The owner pointed out the first version of this solution offered two call sites for `Migrate`
(`cmd/{service}/main.go`'s own startup path, or a separate `cmd/migrate` job) with no stated
criteria for choosing between them, then proposed dropping startup-run entirely: migrations always
run as a separate job, gated so the app container cannot start until the job completes — better
scaling under load, no concurrent-migration races to reason about, and ~zero ongoing cost once the
job has completed. Recorded as its own ADR,
`solution-go-db-migrations.skill/adr/job-only-not-startup-run.md` (searched: job-only vs. the
original team's-choice design vs. startup-run-only — job-only selected).

**Real per-platform mechanics, verified via web search before committing to the design** (the
owner's proposal is sound, but "the app waits for the job" does not work identically everywhere):
Docker Compose supports it natively (`depends_on: condition: service_completed_successfully`,
Compose v2.20+/Engine 25+). Kubernetes supports it via a `Job` + `kubectl wait
--for=condition=complete`, or automatically via a Helm `pre-install,pre-upgrade` hook. **Docker
Stack (Swarm) does not** — `docker stack deploy` silently ignores `depends_on` entirely (confirmed:
a long-standing, still-open Docker Compose/Moby issue), even though Swarm's own `deploy.mode:
replicated-job` (added specifically for one-shot tasks like migrations) exists — so on Stack the
"wait" has to be a scripted two-step deploy (job stack deployed and polled via `docker service ps`
until `Complete`, then the app stack), not a manifest-only dependency.

**Where this landed:** the actual "gate the app on the job" rule was added to
`skills/devops/devops-service-deploy.skill/devops-service-deploy.skill.md` (a new MUST rule,
mirrored into its `service-deploy-skill-template.md` skeleton, plus new
`docker-stack-migrate.example.yml`/`k8s-migrate-job.example.yml` templates and wait-step updates to
all three deploy guides) — **not** duplicated inside this Go catalog, since it is a cross-stack
deployment-topology concern every service repository needs, not something specific to Go or to
`PersistentDb`. `solution-go-db-migrations` now only provides `cmd/migrate` (the binary that
skill's manifests invoke) and references it in prose; `INVARIANTS.md`'s external-reference carve-out
list (section 4) was extended to name this one addition explicitly. `cmd/{service}/main.go.extend.md`
was deleted from this solution's `Implementation/` — the solution no longer touches `main.go` at
all. goose's session lock is kept, reframed as defense-in-depth against a retried/duplicated job
run rather than the primary safety mechanism (that is now the deploy pipeline itself, which never
starts the app while the job is still running).

### Third follow-up — reinstated MigrateOnStart as a second, conditionally-safe mode

The owner's own real deployment topology made job-only stricter than necessary: their Docker Stack
deployments are always single-replica (the concurrency risk job-only exists to prevent cannot occur
there), and their Kubernetes deployments always go through a Helm chart (a `pre-install,pre-upgrade`
hook Job blocks the release natively, so the two-command sequence the plain-manifest path needs is
never actually paid). The owner also explicitly does not want a two-command deploy anywhere it can
be avoided.

**Before landing on the final design, the owner asked whether wiring both modes at once (Job *and*
startup-run, as a "safety net") would create problems.** Answered with five concrete mechanisms, not
just "it's not best practice": every process restart (crash/OOM/eviction/HPA scale-out), not only
deploys, would touch the database; it directly undermines autoscaling; it can introduce a new
pod-readiness stall if an unrelated restart races a still-running Job holding the lock; it silently
masks a skipped/misconfigured Job step instead of failing loudly; and it re-couples migration
failure to the app's crash-loop for that path. A lighter alternative (a read-only version-mismatch
check at startup, fail loudly on mismatch, no mutation) was named as the right tool for "catch a
skipped Job step" instead of a full migrate-on-start fallback — documented as a future option, not
built today.

**Then verified two best-practice claims before finalizing** (per this build's own "ground truth,
not assumed" pattern): the Twelve-Factor App methodology's Admin Processes factor (XI) independently
corroborates "separate process, same codebase" as the default shape; Kubernetes-specific
practitioner sources independently name startup-run as the anti-pattern for the owner's own original
reason (N replicas → N concurrent attempts) and explicitly reject init containers for the identical
reason (per-pod, same problem moved earlier); Helm's own documentation confirms `pre-install,
pre-upgrade` hooks genuinely block the release on the hook resource's completion, natively.

**Final design:** `Config.MigrateOnStart` (`MIGRATE_ON_START`, default `false`) is the one switch.
`cmd/{service}/main.go.extend.md` was rewritten (recreated after being deleted in the prior
follow-up) to call `Migrate` only inside `if cfg.MigrateOnStart`. The condition for choosing
MigrateOnStart is stated generically in `devops-service-deploy.skill.md` — safe only when at most
one instance of the migrating process can ever run concurrently — **not** as an unconditional
"Docker Stack = MigrateOnStart" mapping, because that skill is shared by every service in this
repository, not only ones matching this owner's own topology; the shared Stack template's own
default (`replicas: 2`) is left requiring Job mode unless a specific service's own copy documents
single-replica and switches it. For this owner's own stated topology the condition resolves to
exactly what they asked for (Stack → MigrateOnStart, Kubernetes-via-Helm → Job,
`MIGRATE_ON_START=false` explicit). This distinction (generic conditional rule vs. this owner's own
resolved instance of it) was raised proactively, per this repository's `AGENTS.md` "Role stance"
instruction to push back on an incomplete/inconsistent ask rather than defer silently — the owner's
underlying goal was still fully honored.

Recorded as `solution-go-db-migrations.skill/adr/migration-mode-per-platform.md`, written bilingually
(English + Russian, mirrored structure) at the owner's explicit request given the topic's
complexity — it **supersedes** `adr/job-only-not-startup-run.md`, which was kept unedited with a
superseded-by pointer rather than deleted or rewritten, per `adr-create`'s "record the rejected
options" principle (its reasoning wasn't wrong, the owner's topology just made a second mode safe in
addition to it, not a replacement for it). `devops-service-deploy.skill.md`'s "migration step" rule
and its mirror in `service-deploy-skill-template.md` were rewritten to the same generic,
condition-based shape.

### Fourth follow-up — gave Helm its own dedicated section instead of a footnote inside plain-k8s

The owner pointed out that Helm — their actual, only Kubernetes deployment path — was buried as a
`MAY`-level bonus and a commented-out annotation snippet inside the plain-manifest `k8s/` materials,
not a real, standalone, worked path. Built a full, real chart under
`templates/helm/chart-example/` (`Chart.yaml`, `values.yaml`, `.helmignore`,
`templates/_helpers.tpl`/`deployment.yaml`/`service.yaml`/`configmap.yaml`/`secret.yaml`/
`ingress.yaml`/`migrate-job.yaml`) plus its own `helm-deploy.example.md` deploy guide
(install/upgrade/rollback/uninstall).

**Verified for real, not just written**: installed `helm` (v3.22.0, via the official
`get-helm-3` script — not present in this environment before) and ran `helm lint` (clean, one
informational note only) and `helm template` in both `migrate.mode=job` and `migrate.mode=onStart`
against the actual in-place chart files, confirming the migrate Job renders only in `job` mode and
`MIGRATE_ON_START` flips to `"true"` only in `onStart` mode — never both, by construction of the
template's own `{{- if eq .Values.migrate.mode "job" }}` condition and the Deployment's env var
being derived from that same values key, not a second independently-settable one. This closes the
gap the ADR's own Boundary named (`solution-go-db-migrations` "does not verify... relies on the
deploying team reading and honoring it") for the Helm path specifically: the chart makes "both modes
at once" structurally impossible to reach via a values override, not merely documented against.

`devops-service-deploy.skill.md` restructured accordingly: "Kubernetes coverage" now explicitly
names plain-manifests and Helm as two alternative, equally first-class paths (pick one, don't
maintain both as the steady state); a new "Helm chart coverage" rule states the single-source-of-
-truth requirement above; `templates/helm/` joins `templates/docker/`/`templates/k8s/` in the
required grouping layout; Example/Check-list sections and the `service-deploy-skill-template.md`
mirror all updated to match. `MAY: Provide a Helm chart in addition to plain manifests` was removed
— that framing was already inconsistent with the pre-existing `Kubernetes coverage` MUST rule, which
had always accepted "manifests **or** Helm chart" as alternatives satisfying the same requirement,
not one as an addition on top of the other.
