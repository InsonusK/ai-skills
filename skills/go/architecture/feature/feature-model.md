# skills/go/architecture Feature Model

Built from first principles for a new Program Family — a Go web-service — with no pre-existing catalog to reconcile against. Grounded against one concrete reference implementation, `tmp/tg-bot-service` (a Telegram quiz-bot service that layers this family's base plus `GrpcApi`, `ExternalIntegration`, and a partial `CachedDb` on top), read in full rather than inferred from names. Where the reference carries product-specific concerns that are not part of this generic family, they are named and excluded — see [Out of scope](#out-of-scope).

The common baseline this model assumes (concretely, not just conceptually):
```
go.mod                              — module + versions; Go's module system already centralizes
                                       version pinning, so unlike a NuGet-based stack this family
                                       needs no separate "central package management" feature
Makefile
cmd/
  {service}/
    main.go                         — composition root: manual constructor wiring, no DI container
internal/
  domain/
    services/                       — business logic; depends on nothing outside this package
      {service}.go                    at baseline (no external system exists yet to abstract over)
      features/                     — Cucumber .feature files covering the business logic
      test/                         — godog step definitions + World
  api/
    http/
      server.go                    — stdlib net/http, thin adapter translating requests into
                                       domain-service calls and domain sentinel errors back into
                                       HTTP status codes
  config/
    config.go                      — flat struct sourced from env vars, each var also readable
                                       from a "<NAME>_FILE" path (container-secret convention)
  logging/
    logger.go                      — log/slog, one process-wide default handler set at startup
  version/
    version.go                     — build-time version var, set via -ldflags
tools/
  normalize_unittest/               — normalizes `go test -json` output into tmp/result/unit-test.json
  normalize_mutation/               — normalizes the mutation tool's report into tmp/result/mutation-test.json
  test_report/                     — assembles public/ from the normalized tmp/result/*.json files
report-template/
  index.html                       — static landing page, copied verbatim by test_report
```
No `internal/domain/interfaces/` exists at this baseline — the domain layer has no outbound port to declare until a feature that needs one (`ExternalIntegration`, `CachedDb`, `PersistentDb`) is selected; whichever is selected first creates the folder, and any of the other two extends it. No `internal/infrastructure/`, `internal/api/grpc/`, `proto/`, `buf/`, or `gen/` exists at baseline either, for the same reason.

**`Service`, the family's product itself, is grouped inside the `Common` box together with the mandatory features** — not a row in the [Features](#features) table below, and never a variability question — matching the convention every other variable feature connects to the `Common` block as a whole, not to `Service` individually.

## Feature diagram

@import "/skills/go/architecture/feature/diagrams/feature-diagram.mmd" {as="mermaid"}

No parallel `Requires` edges point at the same target in this model — the single `Requires` edge (`OutboxPattern -> PersistentDb`) has no sibling to state AND/OR logic against.

## Modeling choices specific to this family (vs. the dotnet/angular catalogs)

- **`HttpApi` is common, not a "SyncInboundApi" umbrella of independently-optional transports.** The family's own base case (stated by the family's owner) always exposes HTTP; `GrpcApi` is purely an *additional* inbound surface on top of it, not one of two alternatives where at least one is required. There is accordingly no `At least one (protocol)` group on the inbound side, unlike the dotnet catalog's `SyncInboundApi`.
- **`ExternalIntegration` is one feature, not a transport-split umbrella.** The family owner named it as a single option ("внешних интеграций"), realized in the reference implementation over gRPC (`quizclient`, calling an external quiz-agent service) but not committed to that transport — the port a consumer declares in `internal/domain/interfaces` is business-named (e.g. `QuizAgent`), and which transport backs its adapter is an implementation choice, not a further catalog split. A future second transport realization is a change to this one feature's `Realized by`, not a new sibling feature — deliberately simpler than the dotnet catalog's `SyncOutboundApi` split, because the family owner did not ask for that split here.
- **`CachedDb` and `PersistentDb` are independent, not an "at least one" pair.** Either, neither, or both may be selected — the reference implementation demonstrates only a partial `CachedDb` (a Redis-backed mapping store), with no `PersistentDb` yet. Neither requires the other.
- **`AsyncInboundApi`/`AsyncOutboundApi` keep the dotnet catalog's two-level shape** (umbrella feature + a `Mandatory` `KafkaConsumer`/`KafkaProducer` child) even though Kafka is, today, this family's only realization of each — this leaves room for a second broker later as a change to the child set, not a re-model of the umbrella, and reuses vocabulary already established in this repository.

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| DomainLogic | Business logic (`internal/domain/services`) that depends on infrastructure only through narrow, business-named outbound ports it declares itself (`internal/domain/interfaces`) — never the reverse, and never a framework/transport type leaking into it. Inbound adapters call the domain service's concrete type directly; only *outbound* dependencies are interfaces, since only they ever have more than one implementation. | true |
| HttpApi | Inbound entry point: a thin `net/http` adapter that decodes a request, calls the domain service, and translates its sentinel errors back into HTTP status codes. | true |
| AppLogging | Process-wide structured logging (`log/slog`), configured once at startup from a single log-level setting. | true |
| TestConformance | The module's build is gated by a fixed set of conformance checks — not itself a check, the parent of the four `Mandatory` ones below. | true |
| Cucumber Test | Every `.feature` scenario has a real, executing godog step-definition binding — no undefined/pending step reaches the build gate. | true |
| Code Coverage Test | The build enforces code coverage collection on every run (`go test -coverprofile`). | true |
| Mutation Test | The build runs mutation testing over the test suite via `gremlins`, catching tests that pass without exercising the behavior they claim to cover. | true |
| Test Reports | Test/coverage/mutation results are normalized and published as a build artifact (`public/`) in a consistent report format. | true |
| GrpcApi | A second inbound entry point exposing the same domain calls as `HttpApi`, over gRPC, sharing the same domain-service instance — not an alternative to `HttpApi`, an addition on top of it. | false |
| ExternalIntegration | The module calls out to another service through a narrow, business-named outbound port declared in `internal/domain/interfaces`, implemented by an `internal/infrastructure` adapter. | false |
| CachedDb | Domain depends on a narrow, business-named outbound port backed by a cache-capable store with no durability guarantee assumed by the port itself (demonstrated over Redis). | false |
| PersistentDb | Domain depends on a narrow, business-named outbound port backed by a durable system-of-record store. | false |
| AsyncInboundApi | The module reacts to asynchronous messages from other services. | false |
| KafkaConsumer | This family's only realization of `AsyncInboundApi` today: consumes messages from a Kafka topic — `Mandatory` once `AsyncInboundApi` is selected, not itself a choice. | false |
| AsyncOutboundApi | The module publishes asynchronous messages for other services to react to. | false |
| KafkaProducer | This family's baseline realization of `AsyncOutboundApi`: publishes messages to a Kafka topic directly — `Mandatory` once `AsyncOutboundApi` is selected. | false |
| OutboxPattern | Writes an outgoing message to a transactional outbox in the same transaction as the persisted business change, then relays it to Kafka, instead of publishing directly — requires `PersistentDb`. Once `PersistentDb` and `AsyncOutboundApi` are both selected *and* a given publication is triggered by the persisted change, the family owner's own rule makes this the required realization for that publication, not merely a legal option — see the forthcoming Variability Map's constraint notes for the precise trigger condition. | false |

No pure selector and no mandatory companion was excluded from this table — every feature above has an independent identity worth its own row (unlike, e.g., the dotnet catalog's Cecil-architecture-tests companion, which has no analog here).

## Out of scope

- **Fixed infrastructure excluded, and why.** Module/version pinning is not a feature — Go's module system (`go.mod`) already centralizes it, unlike a NuGet-based stack. Composition-root wiring (`cmd/{service}/main.go`), config loading, and the `tools/`/`report-template/` test-reporting scaffolding are treated as baseline prose above, never as feature-table rows, matching this repository's existing dotnet catalog's convention (`solution-sln-structure`-equivalent structure is realized by a solution but is not itself a Feature Model row).
- **Plateau Components are a different mechanism entirely** — an optional, cross-cutting capability attached at a plateau's composition root (see `plateau-component-create`) is intentionally excluded from this model.
- **This model targets the intended generic Program Family, not a literal copy of `tmp/tg-bot-service`.** That reference is one concrete product built on this family (base + `GrpcApi` + `ExternalIntegration` + partial `CachedDb`), and it carries product-specific concerns deliberately excluded here because they are not this family's own variability:
  - **The Telegram channel itself** — in the reference it is simultaneously an inbound adapter (bot commands) and an outbound `ExternalIntegration`-port implementation (pushing messages), a genuinely dual-natured shape. This family does not model "Telegram" as a feature; a consumer of this catalog that happens to be a Telegram bot would realize its own inbound channel and its own `ExternalIntegration` port the same way any other product-specific channel would, outside this catalog.
  - **Access control** (the reference's YAML allow/deny list) plugs in only at that one Telegram-inbound middleware point, has no domain-level port, and is not applied to `HttpApi`/`GrpcApi` at all in the reference. It is product policy, not a catalog feature.
- **Unverified constraints.** The one `Requires` edge (`OutboxPattern -> PersistentDb`) rests on straightforward reasoning (an outbox table lives in the persistent store an outbox relays from) and is not flagged as provisional. The *strength* of the rule for `AsyncOutboundApi` + `PersistentDb` combined — outbox becomes required, not optional, once a publication is triggered by the persisted change — is the family owner's own explicitly stated business rule, not this skill's inference; it is recorded here and will carry into the Variability Map's constraint column with its precise trigger condition.
- **`IsCommon` verdicts are judgment calls, not proofs**, in particular `AppLogging` — present and structured the same way throughout the one reference implementation examined, treated as common by the same reasoning the dotnet catalog used for its own `AppLogging`, but not proven necessary by construction the way `DomainLogic`/`HttpApi` are. Flagged for the family owner's confirmation.
- **`AsyncInboundApi`, `KafkaConsumer`, `AsyncOutboundApi`, `KafkaProducer`, and `OutboxPattern` are aspirational for the first build of this catalog.** None of the five plateaus planned for the initial build (base; +`GrpcApi`; +`GrpcApi`+`ExternalIntegration`; + those +`CachedDb`; + those +`PersistentDb`) realizes them. Per this repository's established pattern (see the dotnet catalog's `v3.1` build), their solutions will still be authored — as skeletons with a draft-contract marker — so the Variability Map's `Realized by` column has nowhere left empty, with full authoring deferred until a real consumer (a sixth plateau) exists.
