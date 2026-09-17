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
