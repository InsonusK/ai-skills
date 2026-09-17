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
  `solution-go-conformance-testing` (extends the shared
  `skills/common-workflow/test/solution-conformance-testing.skill`, mirroring the
  `ts`/`python`/`dotnet` per-stack extensions). VP-realizing: `solution-grpc-api` (VP1),
  `solution-external-integration` (VP2), `solution-messaging-infrastructure` +
  `solution-kafka-producer` (VP3, skeleton), `solution-transactional-outbox` (VP4, skeleton),
  `solution-kafka-consumer` (VP5, skeleton), `solution-cached-db` (VP6), `solution-persistent-db`
  (VP7). Expect most intersections to classify `-N-` (disjoint `internal/` packages per solution) —
  confirm once actually authored, and iterate delta-conflict-detection's grouping pass for real
  before treating any VP's `Realized by` cell as final.
- ⚠️ **Mutation-tool-per-stack ADR to be updated** (`skills/common-workflow/test/
  solution-conformance-testing.skill/adr/mutation-tool-per-stack.md`) to add Go: **gremlins**
  (`github.com/go-gremlins/gremlins`) — confirmed from `tmp/tg-bot-service`'s own `Makefile`
  (`mutation-test` target), not invented. This is a shared cross-stack file outside
  `skills/go/architecture/` — the same kind of carve-out INVARIANTS.md will record (mirrors
  dotnet's INVARIANTS.md §4 carve-out for its own two allowed external `depends_on` targets).
