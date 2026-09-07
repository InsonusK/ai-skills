# Worked example: `skills/dotnet/architecture/v3.1`

A short walkthrough of applying [../feature-map-create.skill.md](../feature-map-create.skill.md) to a real Program Family, built live in dialogue with the family's owner.

## The baseline came first, and it changed the verdicts
Before any `IsCommon` call was made, the concrete baseline was written out: `App.Host`, `Modules/{ModuleName}/{ModuleName}.Application(+.Test)`, `{ModuleName}.Interfaces(+.Test)`, `Shared`, `BuildingBlocks/MediatR/{ExceptionHandlingBehaviour,ValidationBehavior}.cs` — critically, **no `{ModuleName}.Domain` project**. That one omission is what correctly excluded `DomainLogic` (guarded entity behavior) from the common set — a module can be "a simple service for performing functions" with no entities at all, so entity invariants cannot be something every family member has.

## A capability nearly excluded as "just infrastructure," three times
Exception handling, structured logging, and build/test conformance gates were all first treated as fixed infrastructure outside the Feature Model entirely — the same mistake, made three times before it was caught. Once asked, the family's owner wanted all three tracked as explicit, first-class common features (`ExceptionHandlingPipeline`, `AppLogging`, `TestConformance` with its own four mandatory children: Cucumber Test, Code Coverage Test, Mutation Test, Test Reports) — `AppLogging` in particular is deliberately minimal today specifically because it is expected to grow (file-backed persistence later), which is exactly the kind of thing worth tracking as a feature rather than assuming away.

## Two features that turned out to be one
`CommandIntegration` and `QueryIntegration` were originally modeled as two separate optional features (a CQRS read/write split). Once the actual MediatR mechanism was examined, both are the same request-dispatch mechanism — the "command" vs. "query" distinction is a naming convention, not a technical difference. They were merged into one common feature, `MediatorModuleIntegration`, covering both Request (command/query) and Notification (pub/sub) dispatch — after which several downstream constraints (e.g. "`HttpApi` requires a command or query integration") became unconditionally true and were dropped entirely, since the thing they required was now common.

## Matrix layout and typed edges
Once `Common` grew past four members, it was reorganized as a row matrix (nested `direction LR` subgraphs inside a `direction TB` block, row borders hidden via `style rowId fill:none,stroke:none`) instead of a vertical list — see [feature-diagram.mmd](../../../feature/diagrams/feature-diagram.mmd). Every edge carries its exact relation label; `SyncInboundApi`/`SyncOutboundApi` each use `"Optional (at least one)"` for their protocol children (`HttpApiController`/`GrpcApiController`, `HttpApiClient`/`GrpcApiClient`) — a module can pick either, or both, but the family's owner explicitly preferred this phrase over the literature's own "Or (group)" term.

## A flagged, unconfirmed constraint
`OutboxPattern` was modeled as requiring `Persistence` (an outbox table must be written in the same transaction as the business change) — but unlike every other constraint in the model, no existing solution implements Kafka/outbox yet to verify this against. It was flagged as architectural reasoning pending confirmation, not presented with the same confidence as a verified constraint.

See the real, complete result: [feature-model.md](../../../feature/feature-model.md) and [feature-diagram.mmd](../../../feature/diagrams/feature-diagram.mmd).
