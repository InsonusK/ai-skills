---
name: plateau-offline-sync-service--csproj-module-application
description: Project {Module}.Application in the plateau-offline-sync-service plateau — orchestration only (command/query handlers, per-feature validators, reusable property/DTO validators, module DI self-registration)
whenToUse: when adding or editing a handler, a validator, or the module registration in {Module}.Application, or deciding whether logic belongs here rather than in Domain or Interfaces
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
  - "[[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Orchestrate use cases by connecting the public contract to work — at plateau-core, dispatch and data-shaping only; there is no domain layer and no persistence.
- Own every command/query handler, every per-feature validator, the reusable property/DTO validators, and the module's one DI self-registration method.
- Contain no business logic.

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Core Principles
- One feature folder per operation under `/Features/{FeatureName}` — the handler (`{FeatureName}.Handler.cs`) and its per-feature validator (`{FeatureName}.Validator.cs`) co-located.
- Handler shape at this plateau: `guard → load → domain call → stage → return Result<T>` — a persisted command loads via a named spec + `IRepository<T>`, calls the entity's guarded method, and stages with `UpdateAsync`/`AddAsync`. It never calls `SaveChangesAsync` (that is `UnitOfWorkBehavior`'s job).
- Per-feature validators enforce transport correctness only and compose the reusable property/DTO validators via `SetValidator` — they never re-state a condition.
- Reusable validators live under `/Validators`: `Property/{ValueObject}PropertyValidator` (`AbstractValidator<Soft{ValueObject}>`), `Model/{Dto}Validator`, `Async/{Feature}Check` (its `Load` is an unimplemented seam until VP2).
- Handlers and validators self-register by assembly scan in `{Module}ApplicationRegistration.Register{ModuleName}Module()` — `AddMediatR` + `AddValidatorsFromAssembly`. Pipeline behaviors are **not** registered here.
- Cross-module interaction is `ISender.Send` / `IPublisher.Publish` against another module's `Interfaces` — never a direct call.

__Applied solutions:__
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
- [[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Application
```

## Project Structure
- /{ModuleName}.Application
  - /Features/{FeatureName}
    - [{FeatureName}.Handler.cs](./classes/plateau-offline-sync-service--class-feature-handler.skill.md)
    - [{FeatureName}.Validator.cs](./classes/plateau-offline-sync-service--class-feature-validator.skill.md)
  - /Events/{EventName}
    - [{EventName}.EventHandler.cs](./classes/plateau-offline-sync-service--class-event-handler.skill.md)
  - /Validators
    - /Property/[{ValueObject}PropertyValidator.cs](./classes/plateau-offline-sync-service--class-value-object-property-validator.skill.md)
    - /Model/[{Dto}.Validator.cs](./classes/plateau-offline-sync-service--class-dto-validator.skill.md)
    - /Async/[{Feature}Check.cs](./classes/plateau-offline-sync-service--class-feature-check.skill.md) — `Load` throws until VP2
  - /Specifications
    - [{Entity}ByIdSpec.cs](./classes/plateau-offline-sync-service--class-entity-byidspec.skill.md) — named load-by-Id spec (VP2)
  - /Concurrency
    - [{Entity}VersionResolver.cs](./classes/plateau-offline-sync-service--class-entity-version-resolver.skill.md) — per-entity `IEntityVersionResolver` (VP5)
  - /Resolvers
    - [Create{Entity}GuidResolver.cs](./classes/plateau-offline-sync-service--class-create-entity-guid-resolver.skill.md) — per-entity `IGuidResolver` (VP6)
  - [{ModuleName}ApplicationRegistration.cs](./classes/plateau-offline-sync-service--class-module-application-registration.skill.md)
  - {ModuleName}.Application.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Features/{FeatureName}/{FeatureName}.Handler.cs | Command or query handler | [[./classes/plateau-offline-sync-service--class-feature-handler.skill.md\|class-feature-handler]] |
| /Features/{FeatureName}/{FeatureName}.Validator.cs | Per-feature transport validator | [[./classes/plateau-offline-sync-service--class-feature-validator.skill.md\|class-feature-validator]] |
| /Events/{EventName}/{EventName}.EventHandler.cs | Notification handler | [[./classes/plateau-offline-sync-service--class-event-handler.skill.md\|class-event-handler]] |
| /Validators/Property/{ValueObject}PropertyValidator.cs | Reusable `Soft{ValueObject}` validator | [[./classes/plateau-offline-sync-service--class-value-object-property-validator.skill.md\|class-value-object-property-validator]] |
| /Validators/Model/{Dto}.Validator.cs | Reusable RequestDto validator | [[./classes/plateau-offline-sync-service--class-dto-validator.skill.md\|class-dto-validator]] |
| /Validators/Async/{Feature}Check.cs | Async cross-aggregate check seam | [[./classes/plateau-offline-sync-service--class-feature-check.skill.md\|class-feature-check]] |
| {ModuleName}ApplicationRegistration.cs | Module DI self-registration (+ `AddScoped<{Entity}VersionResolver>()`) | [[./classes/plateau-offline-sync-service--class-module-application-registration.skill.md\|class-module-application-registration]] |
| /Specifications/{Entity}ByIdSpec.cs | Named load-by-Id specification | [[./classes/plateau-offline-sync-service--class-entity-byidspec.skill.md\|class-entity-byidspec]] |
| /Concurrency/{Entity}VersionResolver.cs | Reads one entity's current version | [[./classes/plateau-offline-sync-service--class-entity-version-resolver.skill.md\|class-entity-version-resolver]] |
| /Specifications/{Entity}ByGuidSpec.cs | Guid lookup spec for an external-created entity | [[./classes/plateau-offline-sync-service--class-entity-byguidspec.skill.md\|class-entity-byguidspec]] |
| /Resolvers/Create{Entity}GuidResolver.cs | Returns the existing entity as a `ConflictResult` on a duplicate Guid | [[./classes/plateau-offline-sync-service--class-create-entity-guid-resolver.skill.md\|class-create-entity-guid-resolver]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| MediatR | central | `IRequestHandler<,>`, `INotificationHandler<>`, `ISender`/`IPublisher` |
| Ardalis.Result | central | `Result<T>` return type |
| Ardalis.Specification | central | `Specification<T>` for named specs |
| FluentValidation | central | `AbstractValidator<T>` |
| FluentValidation.DependencyInjectionExtensions | central | `AddValidatorsFromAssembly` |

## What Does NOT Belong Here
- Business rules — belong to `{Module}.Domain` (does not exist at plateau-core); model them as guards on cross-request facts until it does.
- Pipeline behaviors and their registration — belong to [[../App.Host/plateau-offline-sync-service--csproj-app-host.skill.md|App.Host]].
- `DbContext`, repositories, `SaveChangesAsync` — arrive with VP2.
- Cross-module JOIN specs — belong to `App.Queries` (does not exist at plateau-core).

## Allowed Dependencies
- `Shared`, `{Module}.Interfaces`, `{Module}.Domain` (own module), `{OtherModule}.Interfaces` — never `App.Infrastructure`, never `DbContext`
- NuGet: `MediatR`, `Ardalis.Result`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`

# Rules
MUST:
- Put each feature in its own `/Features/{FeatureName}` folder; name files `{FeatureName}.Handler.cs` / `{FeatureName}.Validator.cs`, classes `{FeatureName}Handler` / `{FeatureName}Validator`.
- Keep the handler shape `guard → (dispatch | read) → return Result<T>`; never inject `DbContext`, write inline LINQ, or call `SaveChangesAsync`.
- Keep per-feature validators declarative and transport-only; compose `Soft{ValueObject}` / DTO validation via `SetValidator(IValidator<T>)`, never a repeated inline rule; never inject a repository or service into a validator.
- Expose exactly one `Register{ModuleName}Module(IServiceCollection, IConfiguration)`; register handlers via `AddMediatR` scan and validators via `AddValidatorsFromAssembly`; also register each `{Entity}VersionResolver` (VP5) and each `IGuidResolver<Result<Create{Entity}Result>>` → `Create{Entity}GuidResolver` (VP6) as `Scoped`; never register a pipeline behavior or an infrastructure service here.
- Never contain a business rule; never reference another module's `Application` or `Domain`; cross-module only via `ISender`/`IPublisher`.

__Applied solutions:__
- [[../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Check list
- [ ] `/Features/{FeatureName}` exists per operation with the handler + validator co-located.
- [ ] `/Validators/Property`, `/Validators/Model`, `/Validators/Async` folders exist.
- [ ] `{ModuleName}ApplicationRegistration.cs` exposes `Register{ModuleName}Module`, scans for handlers and validators, registers no behavior.
- [ ] No handler references `DbContext` / calls `SaveChangesAsync`; no business rule in any handler or validator.
- [ ] `{Module}.Application.csproj` references only `Shared`, own `Interfaces`/`Domain`, other modules' `Interfaces`.
