# Feature Model

Derived from the existing `skills/dotnet/architecture/v3`/`draft` catalog (22 solutions across 6 plateaus) plus two features not present there yet (`AppLogging`, `ExceptionHandlingPipeline` as an explicit feature rather than assumed infra) — this describes the architectural capability space this Program Family is meant to cover, not only what the old catalog already implements. Two granularities are mixed in one tree, per FODA: most features are answered once per **module**; three (nested under `Persistence`) are answered once per **entity** inside a module that has `Persistence`.

The common baseline this model assumes (concretely, not just conceptually):
```
App/
  App.Host (csproj)
Modules/
  {ModuleName}/
    {ModuleName}.Application (csproj) + .Application.Test
    {ModuleName}.Interfaces (csproj) + .Interfaces.Test
Shared (csproj)
BuildingBlocks (csproj)
  MediatR/
    ExceptionHandlingBehaviour.cs
    ValidationBehavior.cs
```
No `{ModuleName}.Domain` project exists at this baseline — it only appears once `DomainLogic` is selected, which is exactly why entity-level features cannot be common.

**`Module`, the family's product itself, is grouped inside the `Common` box together with the six mandatory features** — not because it is one of them (it is still not a row in the [Features](#features) table below, and never a variability question), but because every optional feature in the diagram hangs off the `Common` block as a whole rather than off `Module` individually, which is what actually reduced the clutter of one node fanning out into fourteen separate arrows.

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

Two cross-tree `Requires` edges, both single-source (no boolean logic to spell out): `ValueObjects` requires `DomainLogic` (the strict `{ValueObject}` type lives in `{Module}.Domain`), and `Persistence` requires `DomainLogic` (persisted entities live in `{Module}.Domain`, which only `DomainLogic` introduces). An earlier draft had `ValueObjects` requiring `DomainLogic` **or** `Persistence`; once `Persistence -> DomainLogic` was made explicit, the `ValueObjects -> Persistence` edge became redundant and was dropped — see the [v3.1 Variability Map](../variability-map.md#vp2s-constraint--persistence-requires-domainlogic).

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| SoftValueObjects | Boundary-level invariants (on a DTO/Command) enforced through a lenient, collect-all validator instead of bare primitives | true |
| ValidationPipeline | Every request passes a MediatR pipeline behavior that validates it before its handler runs | true |
| ExceptionHandlingPipeline | Every unhandled exception is caught by a MediatR pipeline behavior and mapped to a consistent result shape | true |
| CrossModuleValidation | A Soft Value Object or DTO crossing into another module is validated there too, via shared validators | true |
| MediatorModuleIntegration | Modules and `App.Host` never call each other directly — every interaction is a MediatR Command (request/response) or Notification (pub/sub) dispatch | true |
| AppLogging | Basic structured logging to the console, meant to be easily extended to file persistence later (reading logs back from a file is out of scope of this Program Family) | true |
| TestConformance | The module's build is gated by a fixed set of conformance checks — not itself a check, the parent of the four `Mandatory` ones below | true |
| Cucumber Test | Every `.feature` scenario has a real, executing step-definition binding — no undefined/skipped step reaches the build gate | true |
| Code Coverage Test | The build enforces a minimum code coverage threshold | true |
| Mutation Test | The build runs mutation testing over the test suite, catching tests that pass without actually exercising the behavior they claim to cover | true |
| Test Reports | Test/coverage/mutation results are published as a build artifact in a consistent report format | true |
| DomainLogic | The module has a real Domain layer — entities whose state-transition methods guard themselves with a locally-owned invariant check before mutating | false |
| ValueObjects | Domain invariants enforced through a fail-fast Value Object type embedded in an entity, not just at the boundary — requires `DomainLogic` (directly, or transitively via `Persistence`) | false |
| Persistence | The module durably stores and reloads entities (`DbContext`, generic `Repository<T>`, atomic commit via `UnitOfWork`) — requires `DomainLogic` (persisted entities live in `{Module}.Domain`, which only `DomainLogic` introduces) | false |
| CentralizedRules | A validation condition duplicated across two or more consumers gets exactly one centralized declaration, reused everywhere | false |
| EntityConcurrencyControl | A specific persisted entity uses optimistic concurrency (version-checked updates) — only meaningful once `Persistence` is selected, and only for a mutable entity | false |
| ExternalIdentity | A specific persisted entity's identity is client-generated and its creation is idempotent — only meaningful once `Persistence` is selected, and only for an externally-created entity | false |
| AuditTimestamps | A specific persisted entity tracks user/server creation and update timestamps — only meaningful once `Persistence` is selected, and only for a user-initiated entity | false |
| SyncInboundApi | The module answers synchronous, request-response calls from external callers | false |
| HttpApiController | Inbound sync entry point: a REST API (Controllers + Minimal API), thin adapter over MediatR — one of an `Optional (at least one)` group under `SyncInboundApi` — at least one of them is required once `SyncInboundApi` is selected | false |
| GrpcApiController | Inbound sync entry point: a gRPC service, thin adapter over MediatR — the other member of that same `Optional (at least one)` group | false |
| SyncOutboundApi | The module makes synchronous, request-response calls to another service | false |
| HttpApiClient | Outbound sync call: this module calls another service over HTTP — one of an `Optional (at least one)` group under `SyncOutboundApi` — at least one of them is required once `SyncOutboundApi` is selected | false |
| GrpcApiClient | Outbound sync call: this module calls another service over gRPC — the other member of that same `Optional (at least one)` group | false |
| AsyncInboundApi | The module reacts to asynchronous messages from other services | false |
| KafkaConsumer | This module's only realization of `AsyncInboundApi` today: consumes messages from a Kafka topic — `Mandatory` once `AsyncInboundApi` is selected, not a choice | false |
| AsyncOutboundApi | The module publishes asynchronous messages for other services to react to | false |
| KafkaProducer | This module's baseline realization of `AsyncOutboundApi`: publishes messages to a Kafka topic — `Mandatory` once `AsyncOutboundApi` is selected | false |
| OutboxPattern | Writes an outgoing message to a transactional outbox table in the same transaction as the business change, then relays it to Kafka — an `Optional` reliability upgrade over publishing directly | false |

Two things this catalog contains are deliberately **not** rows above:
- **Entity classification** (the four-way Internal/External × Immutable/Mutable decision) is not itself a feature — it is the *consequence* of the `EntityConcurrencyControl` × `ExternalIdentity` combination per entity (Mutable = concurrency-control Yes, External = external-identity Yes → one of 4 named states). It has no independent Yes/No of its own; the `solution-entity-classification` skill is a combination-resolver that spells out what each pairing produces.
- **Cecil architecture tests** (the structural build-time guarantees over the rule mechanism) is a mandatory companion of `CentralizedRules`, not an independently optional feature — nothing in the catalog applies one without the other.

## Out of scope
Limitations of this model, stated explicitly rather than left implicit:
- **Reading logs back from a file** is explicitly out of scope of `AppLogging` and of this Program Family entirely — `AppLogging` only covers writing structured logs, extensibly to file, never querying them back.
- **Fixed project infrastructure beyond what's listed as a feature is not modeled** — repository/solution scaffolding conventions and build/conformance test gates are treated the way FODA's Context Analysis treats external, non-varying infrastructure: always present, never a choice a team makes.
- **Plateau Components are a different mechanism entirely** — an optional, cross-cutting capability like tracing or caching (see `plateau-component-create`) attaches at the composition root independently of this feature model and is intentionally excluded from it.
- **This model targets the intended Program Family, not only today's catalog** — `AppLogging` and `ExceptionHandlingPipeline` (as an explicit feature) do not map to an existing solution yet; building them is future work implied by this model, not something already available to point `Realized by` at.
- **Two cross-tree constraints are identified** — `ValueObjects` requires `DomainLogic`, and `Persistence` requires `DomainLogic`. Both rest on Domain-project provenance (`{Module}.Domain` exists only with `DomainLogic`) plus v3's plateau staging, not on a `depends_on` edge yet — the v3 `solution-value-objects` has an empty `depends_on`. The absence of further constraints reflects what was checked in this pass, not a proof that no others exist.
- **`IsCommon` for the six mandatory features is a judgment call, not a proven invariant**, arrived at through discussion, not derived mechanically from the old catalog (which, notably, did *not* treat `SoftValueObjects`/`MediatorModuleIntegration` as common — this model deliberately corrects that).
