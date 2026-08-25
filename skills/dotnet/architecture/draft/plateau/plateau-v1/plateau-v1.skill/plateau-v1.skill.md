---
name: v1
description: The deployable baseline — pure composition of plateau-service-with-api, plateau-shared-rules, and plateau-statefull-service, introducing no solutions of its own. A module composing this plateau has real domain logic, real persistence, an optional centralized-rule mechanism with structural guarantees, and a real external surface (HTTP and/or gRPC) — everything built in this hierarchy, unioned.
whenToUse: when scaffolding or reviewing a module meant to actually ship — or when deciding which of the three composed plateaus' own root skill to consult for the full detail behind one bullet here
domain: skill
type: template
version: 20260825140000
tags:
  - skill/template/plateau
  - plateau/v1
parent_plateaus:
  - "[[skills/dotnet/architecture/draft/plateau/plateau-service-with-api/plateau-service-with-api.skill/plateau-service-with-api.skill.md|plateau-service-with-api]]"
  - "[[skills/dotnet/architecture/draft/plateau/plateau-shared-rules/plateau-shared-rules.skill/plateau-shared-rules.skill.md|plateau-shared-rules]]"
  - "[[skills/dotnet/architecture/draft/plateau/plateau-statefull-service/plateau-statefull-service.skill/plateau-statefull-service.skill.md|plateau-statefull-service]]"
standalone: true
created_by:
---

# Goal
Give a module everything this hierarchy has built, unioned into one deployable baseline — pure composition, nothing new of its own:
- Real domain logic: Value Objects, guarded entity behavior, the validation pipeline, cross-module DTO/VO validators, the full command/query chain
- Real persistence: `AppDbContext`, generic `Repository<T>`, `IUnitOfWork`, entity classification, optimistic concurrency, idempotent creation, timestamps, cross-module reads via `App.Queries`
- An optional, portable rule-centralization mechanism (`{Module}.Domain.Rules`) with four Mono.Cecil structural guarantees — applied only where real duplication was observed, never speculatively
- A real external surface: REST Controllers/Minimal API and/or gRPC services, each independently optional, both thin `ISender` adapters over the exact same commands/queries

# Core Principles

## What every module gets unconditionally (inherited, unchanged)
- Fixed four-project module shape (Api/Application/Domain/Interfaces), each production project paired with its own dedicated test project except Api; App.Host as the single composition root; global unhandled-exception handling; the `make unit-test`/`mutation-test`/`test-report`/`test-and-report` conformance gate. See [[skills/dotnet/architecture/draft/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill/plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]] (the foundation every parent here already composes).
- Value Objects at both strengths (`Soft{ValueObject}`, `{ValueObject}`); guarded entity behavior; the `ValidationBehavior` pipeline gate; cross-module `{ValueObject}PropertyValidator`/`{Dto}Validator`/`{Feature}Check`; the full `ICommand<TResponse>`/handler/validator/module-registration chain. See [[skills/dotnet/architecture/draft/plateau/plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md|plateau-service-with-validated-module-interaction]].
- Real, durable persistence: `App.Infrastructure` (created once, empty, extended by every outbound integration); `AppDbContext` as the only `DbContext`; `Repository<T>`/`IReadRepository<T>` staging changes, `IUnitOfWork` committing exactly once per top-level command; entity classification (ownership × mutability) driving exactly which of `Version`/`Guid`/timestamps an entity gets; `ConcurrencyBehavior`/`GuidResolvingBehavior` enforced in the pipeline before the handler runs; `App.Queries` for cross-module JOIN reads. Pipeline order: `ExceptionHandlingBehavior` → `ValidationBehavior` → `ConcurrencyBehavior` → `GuidResolvingBehavior` → `UnitOfWorkBehavior`. See [[skills/dotnet/architecture/draft/plateau/plateau-statefull-service/plateau-statefull-service.skill/plateau-statefull-service.skill.md|plateau-statefull-service]].
- Because persistence is real here, `{Feature}Check`'s `Load` step is real too (a genuine `IReadRepository<T>` + named spec, not the throwing stub the shallower plateaus define) — a Domain-classified rule's async check has something to load from, end to end.

## What's optional here, and why composing this plateau doesn't force either
- **Rule centralization** (`solution-domain-rules` + `solution-cecil-architecture-tests`, via `plateau-shared-rules`): applying it to a module is a refactor of already-working code, triggered only by real, observed duplication across a VO constructor, an Entity method, a `PropertyValidator`, and a `DtoValidator` — never speculative. When applied: one rule shape everywhere (`IsValid()`/`IRuleBuilder`-extension/`Check()`), one shared Gherkin source per rule (`{Module}.Domain.Rules.Spec`) proven independently at up to three layers, and four Mono.Cecil structural guarantees (dead-rule detection, exception-type scoping, rejection-code uniqueness, guarded-property coverage) reading compiled IL rather than executing it. See [[skills/dotnet/architecture/draft/plateau/plateau-shared-rules/plateau-shared-rules.skill/plateau-shared-rules.skill.md|plateau-shared-rules]].
- **External surface** (`solution-http-api-publication` and/or `solution-grpc-integration`, via `plateau-service-with-api`): a module composing this plateau needs to be reachable — that's the whole point of `plateau-v1` being the deployable baseline — but *which* protocol(s) is a free choice. HTTP-only, gRPC-only, and both are equally complete applications; neither solution's `Implementation` files, nor App.Host's `AddApi()`/`AddGrpcApi()` pair, reference or assume the other. Both dispatch the exact same `{Module}.Interfaces` commands/queries — an operation is never defined twice. HTTP is resource-route-oriented (five controller archetypes); gRPC is method-oriented (one `{Entity}GrpcService`, one RPC per operation) — neither model is forced onto the other's protocol. `ResultExtensions.ToProblemDetails()`/`RpcExceptionExtensions.ToRpcException()` are each the single place their protocol's status mapping lives.

## Composition mechanics specific to this plateau
- `plateau-shared-rules` was rebound (see its own ADR) to sit directly on `plateau-statefull-service`, so it is already the deepest, most cumulative source for everything except the external API surface — `plateau-statefull-service` is reached both directly and transitively through `plateau-shared-rules` in this plateau's own `parent_plateaus`; union-merge dedups this cleanly, nothing here is doubled. `plateau-service-with-api`'s own lineage never passed through persistence or rule-centralization, so the only genuinely new content it contributes to this union is `{Module}.Api` itself and App.Host's `ApiRegistration`/`GrpcRegistration`.
- `App.Host` is an ASP.NET Core web host (`WebApplication.CreateBuilder`) — `Program.cs` calls `AddModules()`, `AddPipeline()`, `AddInfrastructure()`, and whichever of `AddApi()`/`AddGrpcApi()` are applied, then (after `Build()`) whichever of `UseApi()`/`UseGrpcApi()` are applied, before `Run()`.
- Standalone: `standalone: true` — every ingredient a module needs to actually ship is present once this plateau is composed: domain logic, persistence, and a real external surface. This is the terminal plateau in the hierarchy as currently built — nothing composes on top of `plateau-v1`.
- Known, disclosed gap, carried forward unresolved: `{Module}.Api` has no dedicated test project. `ResultExtensions`/`RpcExceptionExtensions` are real, pure, testable mapping functions with zero coverage anywhere in this plateau — not a silent gap, see the sln-level structure skill's own note.

# Capabilities
- domain (inherited) — Value Objects at both strengths, guarded entity behavior, static domain services.
- validation (inherited) — `ValidationBehavior` before every handler; cross-module `PropertyValidator`/`DtoValidator`/`{Feature}Check`.
- commands (inherited) — the full `ICommand<TResponse>` chain.
- persistence (inherited) — `AppDbContext`, `Repository<T>`, `IUnitOfWork`, concurrency, idempotent creation, timestamps, `App.Queries`.
- rule-centralization (optional) — one declaration per business predicate once duplication is observed, reusable fail-fast and collect-all, reusable unmodified by another .NET service.
- rule-conformance (optional) — one Gherkin source per rule, proven at up to three layers; `{Module}.Domain.Rules`'s own isolated mutation-testing surface.
- rule-structural-integrity (optional) — dead-rule detection, exception-type scoping, rejection-code uniqueness, guarded-property coverage, all build-time facts no BDD scenario proves by construction.
- rest-api (optional) — five controller archetypes, Minimal API for system/webhook/batch operations, `ProblemDetails`, per-module Swagger documents.
- grpc-api (optional) — one `.proto` + one `{Entity}GrpcService` per entity, `RpcException`/`StatusCode` mapping.
- composability — rule-centralization and either/both external-surface protocols are independent axes; applying none, one, or all of them (beyond the always-present domain/persistence baseline) are all valid, complete applications of this plateau.

# Usecases

## Scaffold a module that will actually ship
1. Start from the base four projects (inherited) — the module already has domain logic, the validation pipeline, and the full command chain the moment it exists.
2. Persistence is already there: give the module's entities a classification (ownership × mutability), map `{Entity}Config`, and the concurrency/idempotency/timestamp infrastructure that classification requires follows automatically. See `plateau-statefull-service`'s own Usecases.
3. Publish it: apply `solution-http-api-publication`, `solution-grpc-integration`, or both, depending on what actually needs to call this module. This is the step that makes the module reachable — nothing before it is externally callable.
4. Once a condition is found duplicated across the validation layer's own consumers (not before), centralize it via `solution-domain-rules` — see `plateau-shared-rules`'s own Usecases for the full workflow, including its Cecil structural guarantees.

## Add a Domain-classified rule that needs data from another aggregate
1. This is now provable end to end, not just illustratively — `plateau-statefull-service`'s real `IReadRepository<T>` backs `{Feature}Check`'s `Load` step, so the async check has something to load.
2. Confirm same-aggregate (stays synchronous) versus cross-aggregate/cross-service (Try/Confirm) — see `plateau-shared-rules`'s own Usecases for the full decision procedure and the saga shape.
3. If the same condition is also enforced by the entity's own method, centralize both into one `{Rule}.Check()` — proven independently at the rule level, the VO/Entity level, and the DtoValidator/`{Feature}Check` level via the shared `.feature` source.

## Publish a module over HTTP, gRPC, or both
See `plateau-service-with-api`'s own Usecases for the full per-protocol workflow — nothing about persistence or rule-centralization changes which protocol(s) a module chooses; both adapters dispatch the same `{Module}.Interfaces` commands/queries this plateau's persistence layer already backs.
