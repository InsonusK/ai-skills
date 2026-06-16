---
uid: 68b952b7-5c1e-450b-9803-477905f11ecb
name: default-sln
description: Default plateau — full solution architecture composed from all validated v3 architecture solutions
domain: skill
type: template
version: 20260616
tags:
  - skill/template/sln
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules.solution.skill]]"
---

# Structure

## Project Structure
```
/src
  /Modules
    /{ModuleName}
      /{ModuleName}.Api
      /{ModuleName}.Application
      /{ModuleName}.Domain
      /{ModuleName}.Interfaces
      /{ModuleName}.Api.Tests
      /{ModuleName}.Application.Tests
      /{ModuleName}.Domain.Tests
      /{ModuleName}.Integration.Tests
  /App
    /App.Host
    /App.Infrastructure
    /App.Infrastructure.Migrations
    /App.Queries
  /Shared
  /BuildingBlocks
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/Repository.create.md|Repository.create]]


## Directory and class skills
| Directory | file | Description | Pattern skill |
| --------- | ---- | ----------- | ------------- |
| /Shared | Shared.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/Shared.csproj.skill.md|Shared.csproj.skill]] |
| /Shared/classes | ConflictResult.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/ConflictResult.class.skill.md|ConflictResult.class.skill]] |
| /Shared/classes | ICommand.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/ICommand.class.skill.md|ICommand.class.skill]] |
| /Shared/classes | IEntityVersionResolver.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IEntityVersionResolver.class.skill.md|IEntityVersionResolver.class.skill]] |
| /Shared/classes | IGuidResolver.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IGuidResolver.class.skill.md|IGuidResolver.class.skill]] |
| /Shared/classes | IHasGuid.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IHasGuid.class.skill.md|IHasGuid.class.skill]] |
| /Shared/classes | IHasVersions.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IHasVersions.class.skill.md|IHasVersions.class.skill]] |
| /Shared/classes | IQuery.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IQuery.class.skill.md|IQuery.class.skill]] |
| /Shared/classes | IReadRepository.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IReadRepository.class.skill.md|IReadRepository.class.skill]] |
| /Shared/classes | IRepository.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IRepository.class.skill.md|IRepository.class.skill]] |
| /Shared/classes | IUnitOfWork.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IUnitOfWork.class.skill.md|IUnitOfWork.class.skill]] |
| /Shared/classes | IVersioned.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/Shared/classes/IVersioned.class.skill.md|IVersioned.class.skill]] |
| /BuildingBlocks | BuildingBlocks.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/BuildingBlocks.csproj.skill.md|BuildingBlocks.csproj.skill]] |
| /BuildingBlocks/classes | ConcurrencyBehavior.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/ConcurrencyBehavior.class.skill.md|ConcurrencyBehavior.class.skill]] |
| /BuildingBlocks/classes | ETagEncoder.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/ETagEncoder.class.skill.md|ETagEncoder.class.skill]] |
| /BuildingBlocks/classes | EntityByIdSpec.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/EntityByIdSpec.class.skill.md|EntityByIdSpec.class.skill]] |
| /BuildingBlocks/classes | GuidResolvingBehavior.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/GuidResolvingBehavior.class.skill.md|GuidResolvingBehavior.class.skill]] |
| /BuildingBlocks/classes | UnitOfWorkBehavior.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/UnitOfWorkBehavior.class.skill.md|UnitOfWorkBehavior.class.skill]] |
| /BuildingBlocks/classes | UnitOfWorkContext.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/UnitOfWorkContext.class.skill.md|UnitOfWorkContext.class.skill]] |
| /BuildingBlocks/classes | ValidationBehavior.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/BuildingBlocks/classes/ValidationBehavior.class.skill.md|ValidationBehavior.class.skill]] |
| /App.Host | App.Host.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/App.Host.csproj.skill.md|App.Host.csproj.skill]] |
| /App.Host/classes | ApiRegistration.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/ApiRegistration.class.skill.md|ApiRegistration.class.skill]] |
| /App.Host/classes | EntityVersionResolverRegistration.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/EntityVersionResolverRegistration.class.skill.md|EntityVersionResolverRegistration.class.skill]] |
| /App.Host/classes | ModuleRegistration.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/ModuleRegistration.class.skill.md|ModuleRegistration.class.skill]] |
| /App.Host/classes | PipelineRegistration.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/PipelineRegistration.class.skill.md|PipelineRegistration.class.skill]] |
| /App.Host/classes | RepositoryRegistration.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Host/classes/RepositoryRegistration.class.skill.md|RepositoryRegistration.class.skill]] |
| /App.Infrastructure | App.Infrastructure.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/App.Infrastructure.csproj.skill.md|App.Infrastructure.csproj.skill]] |
| /App.Infrastructure/classes | EntityVersionResolver.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/EntityVersionResolver.class.skill.md|EntityVersionResolver.class.skill]] |
| /App.Infrastructure/classes | ModuleToModuleConfig.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/ModuleToModuleConfig.class.skill.md|ModuleToModuleConfig.class.skill]] |
| /App.Infrastructure/classes | Repository.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/Repository.class.skill.md|Repository.class.skill]] |
| /App.Infrastructure/classes | UnitOfWork.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure/classes/UnitOfWork.class.skill.md|UnitOfWork.class.skill]] |
| /App.Infrastructure.Migrations | App.Infrastructure.Migrations.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Infrastructure.Migrations/App.Infrastructure.Migrations.csproj.skill.md|App.Infrastructure.Migrations.csproj.skill]] |
| /App.Queries | App.Queries.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Queries/App.Queries.csproj.skill.md|App.Queries.csproj.skill]] |
| /App.Queries/classes | AppQueriesRegistration.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Queries/classes/AppQueriesRegistration.class.skill.md|AppQueriesRegistration.class.skill]] |
| /App.Queries/classes | CrossModuleQueryHandler.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/App.Queries/classes/CrossModuleQueryHandler.class.skill.md|CrossModuleQueryHandler.class.skill]] |
| /{Module}.Interfaces | {Module}.Interfaces.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Interfaces/{Module}.Interfaces.csproj.skill.md|{Module}.Interfaces.csproj.skill]] |
| /{Module}.Interfaces/classes | Command.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Interfaces/classes/Command.class.skill.md|Command.class.skill]] |
| /{Module}.Interfaces/classes | Dto.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Interfaces/classes/Dto.class.skill.md|Dto.class.skill]] |
| /{Module}.Interfaces/classes | Query.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Interfaces/classes/Query.class.skill.md|Query.class.skill]] |
| /{Module}.Domain | {Module}.Domain.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/{Module}.Domain.csproj.skill.md|{Module}.Domain.csproj.skill]] |
| /{Module}.Domain/classes | BehaviorService.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/BehaviorService.class.skill.md|BehaviorService.class.skill]] |
| /{Module}.Domain/classes | Entity.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/Entity.class.skill.md|Entity.class.skill]] |
| /{Module}.Domain/classes | EntityConfig.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/EntityConfig.class.skill.md|EntityConfig.class.skill]] |
| /{Module}.Domain/classes | Rule.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/Rule.class.skill.md|Rule.class.skill]] |
| /{Module}.Domain/classes | ValueObject.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Domain/classes/ValueObject.class.skill.md|ValueObject.class.skill]] |
| /{Module}.Application | {Module}.Application.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/{Module}.Application.csproj.skill.md|{Module}.Application.csproj.skill]] |
| /{Module}.Application/classes | CreateEntityGuidResolver.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/classes/CreateEntityGuidResolver.class.skill.md|CreateEntityGuidResolver.class.skill]] |
| /{Module}.Application/classes | EntityByGuidSpec.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/classes/EntityByGuidSpec.class.skill.md|EntityByGuidSpec.class.skill]] |
| /{Module}.Application/classes | EntityByIdSpec.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/classes/EntityByIdSpec.class.skill.md|EntityByIdSpec.class.skill]] |
| /{Module}.Application/classes | EntitySummarySpec.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/classes/EntitySummarySpec.class.skill.md|EntitySummarySpec.class.skill]] |
| /{Module}.Application/classes | Feature.Handler.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/classes/Feature.Handler.class.skill.md|Feature.Handler.class.skill]] |
| /{Module}.Application/classes | Feature.Validator.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/classes/Feature.Validator.class.skill.md|Feature.Validator.class.skill]] |
| /{Module}.Application/classes | ModuleApplicationRegistration.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Application/classes/ModuleApplicationRegistration.class.skill.md|ModuleApplicationRegistration.class.skill]] |
| /{Module}.Api | {Module}.Api.csproj | Project skill | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/{Module}.Api.csproj.skill.md|{Module}.Api.csproj.skill]] |
| /{Module}.Api/classes | ConflictResultExtensions.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/classes/ConflictResultExtensions.class.skill.md|ConflictResultExtensions.class.skill]] |
| /{Module}.Api/classes | EntityController.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/classes/EntityController.class.skill.md|EntityController.class.skill]] |
| /{Module}.Api/classes | EntityRelatedController.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/classes/EntityRelatedController.class.skill.md|EntityRelatedController.class.skill]] |
| /{Module}.Api/classes | ResultExtensions.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/classes/ResultExtensions.class.skill.md|ResultExtensions.class.skill]] |
| /{Module}.Api/classes | SingleEntityController.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/classes/SingleEntityController.class.skill.md|SingleEntityController.class.skill]] |
| /{Module}.Api/classes | SingleEntityPropertyController.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/classes/SingleEntityPropertyController.class.skill.md|SingleEntityPropertyController.class.skill]] |
| /{Module}.Api/classes | SingleEntityRelatedController.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/classes/SingleEntityRelatedController.class.skill.md|SingleEntityRelatedController.class.skill]] |
| /{Module}.Api/classes | SystemEndpoints.class.skill.md | Class pattern | [[skills/dotnet/skill-graph/developing v3/architecture/plateau/default/{Module}.Api/classes/SystemEndpoints.class.skill.md|SystemEndpoints.class.skill]] |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/Repository.create.md|Repository.create]]


# Rules
MUST:
	- `ICommand` and `ICommand<TResponse>` defined in Shared — not BuildingBlocks, not any module
	- All commands implement `ICommand<Result<T>>` — not `IRequest<T>` directly
	- Commands declared as `record` in `/{Module}.Interfaces/Commands`
	- Result records declared in the same file as their command
	- One handler per command — `IRequestHandler<TCommand, Result<T>>`
	- Handler structure: load → guard → domain call → stage → return result
	- All entity loading in handlers uses named specs from [[repository-integration.solution.skill]]
	- Handlers inject `IRepository<T>` from Shared — never `DbContext`
	- Cross-module writes dispatched via `_mediator.Send()` — never direct calls
	- Each module has `Register{ModuleName}Module()` extension method
	- Handlers and validators registered via assembly scan — never manually
	- One `AbstractValidator<TCommand>` per command — co-located with handler in feature folder
	- Validator file named `{FeatureName}.Validator.cs`, class named `{FeatureName}Validator`
	- Validator extends `AbstractValidator<TCommand>`
	- Validators registered via `AddValidatorsFromAssembly` in module registration
	- No validator for query handlers
	- Every entity property mutation validates state through domain rules before assigning
	- Every entity method that changes state validates through domain rules before mutating
	- `DomainException` thrown when a rule returns `false`
	- Bulky logic extracted to static extension methods in `{Module}.Domain/Services`
	- Service extensions delegate all validation to domain rules
	- A single entity property must not have multiple uncoordinated public mutation points
	- One `IEntityTypeConfiguration<T>` per entity
	- `TableName` defined as `public const string`
	- All index and constraint names defined as `public const string` constants
	- `OwnsOne` configured for every multi-property VO property
	- All configurations registered via `ApplyConfigurationsFromAssembly`
	- Domain entities have zero EF attributes
	- Every mutable entity has `public uint Version { get; internal set; }`
	- Every mutable entity implements `IVersioned`
	- Every mutable entity config class declares a public `const string VersionedEntityName` with the stable business name
	- Every mutable entity configuration maps `Version` to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()`
	- All update and patch commands implement `IHasVersions`
	- `IVersioned`, `IHasVersions`, and `IEntityVersionResolver` live in Shared
	- `ETagEncoder` and `ConcurrencyBehavior` live in BuildingBlocks
	- `EntityVersionResolver` lives in App.Infrastructure and discovers mutable entity types by scanning `IEntityTypeConfiguration<T>` config classes for entities that implement `IVersioned`
	- `EntityVersionResolver` registered as `Singleton` in App.Host via `EntityVersionResolverRegistration`
	- `EntityVersionResolver` receives all module Domain assemblies from App.Host
	- Entity name keys in `IHasVersions` and `EntityVersionResolver` are stable business strings — never C# type names
	- Pipeline behaviors registered via centralized `PipelineRegistration` in App.Host
	- GET responses for mutable entities include `ETag` header with encoded versions
	- PUT/PATCH endpoints check `If-Match` presence — return 412 if missing or malformed
	- DTOs returned by GET for mutable entities include `Version` field
	- `ConcurrencyBehavior` returns `Result.Conflict` on version mismatch — never throws
	- `ConcurrencyBehavior` returns `Result.NotFound` if entity missing during version check
	- `ConcurrencyBehavior` reads `Version` through `IVersioned` — no reflection
	- External-created entities have `public Guid Guid { get; internal set; }`
	- `Guid` set exactly once in the entity factory method — never reassigned
	- Unique index on `Guid` configured with named constant `UX_Guid` in entity configuration
	- `{Entity}ByGuidSpec` defined in `/{Module}.Application/Specifications`
	- `IHasGuid`, `IGuidResolver<TResponse>` defined in Shared
	- `ConflictResult<T>` defined in `Shared/Results/ConflictResult.cs`
	- `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
	- `GuidResolvingBehavior` constrained to `where TRequest : IHasGuid`
	- Create commands for external-created entities implement both `ICommand<Result<Create{Entity}Result>>` and `IHasGuid`
	- `Guid` is first property in create command record
	- One `Create{Entity}GuidResolver` per external-created entity type in `/{Module}.Application/Resolvers`
	- Each `IGuidResolver<TResponse>` registered as `Scoped` in module DI registration
	- `IGuidResolver<TResponse>` returns `Task<TResponse?>` — null means not found, non-null means conflict
	- `IGuidResolver<TResponse>` `TResponse` matches the command handler response type exactly
	- Resolver returns `ConflictResult<Create{Entity}Result>` when entity exists — same type as handler success response
	- `GuidResolvingBehavior` returns the resolver's result when it returns non-null — never throws
	- `Create{Entity}Result` contains only the entity Id
	- 409 response body contains the existing entity result — which is `{ id: ... }` because the result contains only Id
	- API layer maps `ConflictResult<Create{Entity}Result>` to HTTP 409 with the result body
	- API layer is a thin HTTP adapter — map input, dispatch once, map output
	- Every controller action dispatches exactly one `ISender.Send()` call
	- Entity lifecycle operations use Controllers — system/webhook/batch use Minimal API
	- Controllers inject `ISender` — never `IMediator`
	- All error responses use `ProblemDetails`
	- Every `ResultStatus` handler can return has an explicit `[ProducesResponseType]`
	- Unexpected `ResultStatus` throws `InvalidOperationException` in `switch` default arm
	- Controller naming follows the five-type model: `{Entity}`, `Single{Entity}`, `Single{Entity}{Property}`, `{Entity}{Related}`, `Single{Entity}{Related}`
	- Routes use kebab-case, singular nouns, `int` route constraints for IDs
	- `ResultStatus.Ok` → 200 OK
	- `ResultStatus.Created` → 201 Created with `CreatedAtAction`
	- `ResultStatus.NoContent` → 204 No Content
	- `ResultStatus.Invalid` → 400 Bad Request with `ProblemDetails`
	- `ResultStatus.NotFound` → 404 Not Found with `ProblemDetails`
	- `ResultStatus.Conflict` → 409 Conflict with `ProblemDetails`
	- `ResultStatus.Error` → 500 Internal Server Error with `ProblemDetails`
	- Any other `ResultStatus` → throw `InvalidOperationException`
	- `PipelineRegistration.cs` defined in `App.Host/DependencyInjection/PipelineRegistration.cs`
	- `AddPipeline()` called once from `Program.cs`
	- All behaviors registered inside `AddPipeline()` using `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior<,>))`
	- Behaviors registered in this exact execution order:
	- Pipeline behaviors registered in App.Host — never inside a module's registration method
	- Behaviors registered in intended execution order
	- `IQuery<TResponse>` defined in Shared — not BuildingBlocks, not any module
	- `IQuery` does not extend `ICommand` — queries are read-only operations and must remain distinct from write-side markers
	- All queries implement `IQuery<Result<T>>` — not `IRequest<T>` directly
	- Queries declared as `record` in `/{Module}.Interfaces/Queries`
	- DTOs declared as `record` in `/{Module}.Interfaces/DTOs`
	- Single-module handlers in `/{Module}.Application/Queries` — inject `IReadRepository<T>`
	- Single-module handlers load via named specs — no inline LINQ
	- Cross-module handlers in `/App.Queries/Queries/{QueryName}` — inject DbContext directly
	- Cross-module handlers apply `AsNoTracking()` on all queries
	- App.Queries handlers registered via `RegisterAppQueries()` assembly scan in App.Host
	- Query handlers return `Result.NotFound()` when entity is missing
	- `RegisterAppQueries()` called from App.Host — after all module registrations
	- `IReadRepository<T>` and `IRepository<T>` defined in Shared, inheriting Ardalis base interfaces
	- `IRepository<T>` extends `IReadRepository<T>` and `IRepositoryBase<T>`
	- `IRepository<T>` has no `SaveChangesAsync` — committing belongs to Unit of Work
	- Single generic `Repository<T>` in App.Infrastructure, inheriting `RepositoryBase<T>` from Ardalis
	- `Repository<T>` implements `IRepository<T>`
	- `Repository<T>` constructor receives `AppDbContext` and passes it to the Ardalis base
	- Open generic DI registration in App.Host for both `IRepository<>` and `IReadRepository<>` pointing to `Repository<>`
	- Registered with `Scoped` lifetime
	- All repository read methods accept `ISpecification<T>` — no raw lambda or LINQ parameters
	- Command handlers inject `IRepository<T>`
	- Query handlers inject `IReadRepository<T>`
	- All entity loading in handlers uses a named spec — no inline `Where(...)` LINQ
	- All specifications for a module live in `/{Module}.Application/Specifications`
	- Cross-module JOIN specs live in `/App.Queries/Specifications`
	- Every entity loaded by Id has a `{Entity}ByIdSpec` in Application
	- Projection specs use `Specification<T, TResult>` — entity filter specs use `Specification<T>`
	- Spec name reflects query intent — not field names or implementation detail
	- Each module has exactly Api, Application, Domain, Interfaces projects
	- Other modules reference only {ModuleName}.Interfaces
	- All cross-module writes go through MediatR command dispatch
	- All cross-module reads go through MediatR query dispatch or App.Queries
	- Tests colocated with module
	- Every project belongs to exactly one layer
	- App.Host is the only composition root
	- App.Infrastructure is the only project with DbContext
	- App.Queries is the only place for cross-module JOIN queries
	- Shared has no project dependencies
	- BuildingBlocks depends only on Shared
	- BuildingBlocks does not define common interfaces — only implements patterns using interfaces from Shared
	- App.Host references BuildingBlocks; modules and other layers reference Shared directly to implement or consume interfaces
	- Pipeline behaviors registered once in App.Host
	- `IUnitOfWork` defined in Shared — single `SaveChangesAsync` method only
	- `UnitOfWorkContext` defined in BuildingBlocks — registered as `Scoped`
	- `UnitOfWorkBehavior` defined in BuildingBlocks — constrained to `ICommand` only
	- `UnitOfWork` implementation in App.Infrastructure
	- `UnitOfWorkBehavior` uses `try/finally` — depth always restored on exception
	- `UnitOfWorkBehavior` commits only when `Depth == 1`
	- `IUnitOfWork` and `UnitOfWorkContext` registered as `Scoped`
	- Sub-commands safe to dispatch from handlers — depth counter prevents premature commit
	- `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
	- `ValidationBehavior` constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
	- Collect all errors from all validators before returning — full error set, not first-error-only
	- Return `Result.Invalid(errors)` on failure — not throw an exception
	- Pass through when no validators registered — missing validator is not a fault
	- All Value Objects declared as `sealed record`
	- All Value Objects immutable — no public setters
	- All Value Objects self-validating — throw `DomainException` on invalid construction
	- Value Objects live in `/{Module}.Domain/ValueObjects` or `/Shared/ValueObjects` when cross-module
	- Multi-property VO has private parameterless constructor
	- Multi-property VO has `OwnsOne` EF configuration on owning entity
	- All rules implemented as static extension methods
	- Rules return `bool` — caller decides whether to throw
	- Rules are stateless and deterministic
	- Primitive rule is single source of truth — VO rules delegate to it
	- All rules live in `/{Module}.Domain/Rules` or `/Shared/Rules` when cross-module
	- Named `{Type}Rules` for primitive/VO rules, `{Condition}Rule` for contextual rules
	- Extract reusable VO/rule to Shared.csproj when used by two or more modules
	- Use Value Object on Entity property when the value has invariant state or carries business semantics
	- Call domain rules inside entity methods before mutating state
	- Throw `DomainException` when a rule returns `false` — the entity enforces, the rule only predicates
	- Configure multi-property Value Objects with `OwnsOne` in the entity's EF configuration
SHOULD:
	- Guard checks return early before domain call — fail fast pattern
	- Handler follow the exact load → guard → domain call → stage → return sequence
	- Validator rules cover all command properties that carry input constraints
	- Use the transport validation boundary table to decide what belongs in validator vs handler vs domain
	- Keep entity methods small and delegate complex calculations to service extensions
	- Name service files after the behavior they encapsulate
	- `Guid` be the first property in the command record — signals external-created entity at a glance
	- Return `Result<Create{Entity}Result>.Created(new Create{Entity}Result(id))` from the handler on successful creation
	- `[Route]` use `{entity}` singular noun — not plural
	- `CreatedAtAction` reference the `Single{Entity}Controller.Get` method for 201 responses
	- Keep `AddPipeline()` the only method that adds `IPipelineBehavior<,>` registrations
	- Document the ordering with inline comments in `AddPipeline()`
	- Use projection spec when DTO maps directly from entity fields — avoids loading full entity
	- Use in-handler mapping when DTO requires computed fields, conditional logic, or nested structure
	- `Transient` lifetime — new behavior instance per pipeline invocation
	- Single-property VO has implicit conversion operators
	- All VOs override `ToString()` when used in logs or UI
	- Complex invariant logic extracted to domain rule
	- Rules be synchronous
	- Rules avoid allocations
	- Use the most specific rule available (primitive, VO, or contextual) for the condition being checked
MUST NOT:
	- Handler contain business logic — delegate to domain
	- Handler call `SaveChangesAsync` — Unit of Work owns commit
	- Handler reference another module's Domain or Application directly
	- Command properties reference domain entity types
	- `ICommand` defined in BuildingBlocks — belongs in Shared
	- Validator contain business rules — transport correctness only
	- Validator inject repositories or services — purely declarative
	- Validator be shared across multiple commands
	- Pipeline behaviors registered inside any module's registration method
	- Reimplement rule logic inline inside entity methods or service extensions
	- Mutate state before validating with rules
	- Allow invalid state to persist silently
	- Let a service extension bypass entity methods and write directly to properties
	- Duplicate invariant logic across setters, methods, or service extensions
	- Use EF data annotations on domain entities
	- Define table, index, or constraint names as inline strings
	- Use `static` instead of `const` for `TableName`, index, or constraint names
	- Put mapping logic in `DbContext.OnModelCreating` directly
	- Configure cross-module foreign keys in Domain config
	- Immutable entities have `Version` property or implement `IVersioned`
	- Create or delete commands implement `IHasVersions`
	- Handler check versions manually — `ConcurrencyBehavior` owns this
	- Controller return 400 for missing `If-Match` — 412 Precondition Failed is correct
	- Entity name keys use C# type names — breaks on entity rename
	- `ConcurrencyBehavior` call `SaveChangesAsync`
	- `ConcurrencyBehavior` use reflection to read `Version`
	- Guid used in domain logic, domain events, relationships, or routes after creation
	- Guid regenerated or changed after entity creation
	- Update, delete, or internal-create commands implement `IHasGuid`
	- `IGuidResolver` registered as open generic — each entity type registers its own concrete resolver
	- Resolver throw exceptions — null means not found, non-null means exists
	- Resolver return a different response type than the command handler
	- `GuidResolvingBehavior` throw exceptions for duplicate Guid detection
	- `GuidResolvingBehavior` construct response DTOs
	- `Create{Entity}Result` carry fields beyond the entity Id for external-created entities
	- Per-controller handling for Guid conflicts — conflict is expressed as `Result<T>` and mapped by the API layer
	- Define a dedicated HTTP middleware for conflict handling
	- Controller action contain business logic, validation, domain rules, or persistence
	- Controller reference Application, Domain, Infrastructure, or DbContext
	- Controller inject `IRepository<T>` or `IUnitOfWork`
	- Minimal API replace entity-lifecycle controllers
	- Undocumented HTTP responses returned — every response shape declared in `ProducesResponseType`
	- Register behaviors inside module registration methods
	- Change pipeline order in multiple files
	- Create multiple pipeline registration extension methods
	- Register behaviors directly in `Program.cs`
	- Query handler inject `IRepository<T>` — signals write intent, use `IReadRepository<T>`
	- Query handler inject `IUnitOfWork` or call `SaveChangesAsync`
	- Query handler modify entity state or dispatch commands
	- Single-module handler use DbContext directly — use `IReadRepository<T>`
	- Cross-module handler live in `{Module}.Application` — Application has no multi-module DB access
	- DTOs expose domain entity types
	- Query handlers may have transport validators — `ValidationBehavior` validates structural correctness before the handler runs
	- `IQuery` extend `ICommand` — queries must remain distinct from write-side markers
	- Cross-module handlers do not use `Include()` — all mapping is done in handler via `Select()` or manual projection
	- Application layer reference DbContext directly
	- Per-entity repository subclass be created
	- `Repository<T>` call `SaveChangesAsync`
	- Raw LINQ predicates appear in repository method signatures
	- Spec call the database or reference DbContext
	- Spec contain business logic — filtering, ordering, and projection only
	- Handler contain inline `Where(...)` LINQ — always delegate to a named spec
	- Specs placed in `{Module}.Domain` — all specs belong in Application
	- Generic spec names used across multiple entities (`GetByIdSpec`) — name per entity
	- Single-module specs live in App.Queries — they belong in the module's Application
	- Module reference another module's Domain
	- Module reference another module's Application
	- Domain reference any other module's project
	- Api reference Domain or Application directly
	- Any module Application reference App.Infrastructure
	- Any module Application reference App.Queries
	- Any module Domain reference another module's project
	- Any module Api reference Domain or Application directly
	- App.Queries be referenced by module Application or Domain
	- Any handler call `SaveChangesAsync` or inject `IUnitOfWork`
	- `UnitOfWorkBehavior` activate on queries — constrained to `ICommand`
	- `UnitOfWorkContext` registered as `Singleton` or `Transient`
	- `UnitOfWork` contain logic beyond `DbContext.SaveChangesAsync` delegation
	- `UnitOfWorkBehavior` contain a catch/rollback block — EF implicit transactions do not require it
	- `IUnitOfWork` defined in BuildingBlocks — belongs in Shared
	- Contain any command-specific conditions in `ValidationBehavior`
	- Throw `ValidationException` — always return typed `Result.Invalid`
	- Value Object depend on infrastructure, repositories, or application services
	- Value Object expose public setters
	- Value Object be used to carry identity — use entity Id for that
	- Primitive used in place of VO when the primitive carries business meaning
	- Rule throw exceptions internally
	- Rule depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
	- Rule mutate any object
	- Rule duplicate logic that already exists in another rule
	- Rule be instantiated with `new` — always static
	- Reimplement rule logic inline inside entity methods — always delegate to existing rules
	- Duplicate the same VO/rule logic across multiple module Domain projects
	- Use primitive type on Entity property when the value carries business meaning or invariant constraints

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]]

# Anti-patterns
- Business rule in handler: `if (task.Status == TaskStatus.Closed) return Result.Conflict(...)` — belongs in entity
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id, ct)` — use named spec
- Manual handler registration in module: `services.AddTransient<CreateTaskHandler>()` — use assembly scan
- `SaveChangesAsync` in handler — Unit of Work commits after handler returns
- Direct call to another module: `_taskService.Create(...)` — use `_mediator.Send(new CreateTaskCommand(...))`
- `CreateTaskCommandHandler.cs` as file name — use `CreateTask.Handler.cs`
- Multiple top-level commands dispatched sequentially from one handler — design as a single orchestrating command
- `ICommand` defined in BuildingBlocks — modules would need a BuildingBlocks reference, violating layer rules
- `RuleFor(x => x.AssigneeId).MustAsync(async (id, ct) => await _repo.AnyAsync(...))` — entity existence is a handler guard, not transport validation
- Validator placed outside its feature folder — always co-located with handler
- Shared validator used for multiple commands: `public class EntityIdValidator : AbstractValidator<IHasId>` — one validator per command
- Business invariant in validator: `RuleFor(x => x.Status).Must(s => s != TaskStatus.Closed)` — belongs in domain entity
- Entity has several points changing the same property with separate validation
- Service extension bypasses entity methods and writes to `internal set` properties directly
- Property mutated from both the entity and multiple service extensions
- Inline rule logic inside entity methods instead of calling rules from `{Module}.Domain/Rules`
- Service extension holds state or depends on infrastructure
- Same business condition checked in controller, validator, entity, and service separately
- Mapping multi-property VO without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names
- Using `[ConcurrencyCheck]` attribute on entity instead of fluent config
- Registering configs manually in DbContext — use `ApplyConfigurationsFromAssembly`
- Annotating domain entity with `[Column]`, `[Index]`, `[ForeignKey]` — all mapping belongs in config class
- Single config class shared across multiple entities — one config per entity, no exceptions
- Cross-module FK configured in Domain config — belongs in App.Infrastructure
- `Version` as plain `uint` on command property instead of `IHasVersions` — does not scale to multi-entity updates
- Handler catches `DbUpdateConcurrencyException` and returns conflict — `ConcurrencyBehavior` should catch this earlier at the application level
- ETag encoding only primary entity version — misses secondary entity conflicts when command touches multiple entities
- `EntityVersionResolver` key using `nameof(TodoTask)` — fragile, breaks on class rename; use a stable business string constant
- Hardcoded entity dictionary in `EntityVersionResolver` — duplicates the entity list and is easy to forget; scan config classes instead
- Defining `IHasVersions` or `IEntityVersionResolver` in BuildingBlocks — violates the rule that common contracts live in Shared
- Handler checks for duplicate Guid manually — duplicates pipeline logic, not reusable
- `IGuidResolver` implemented in Domain — resolver uses `IReadRepository<T>`, belongs in Application
- `IGuidResolver` registered as open generic — breaks DI resolution per command result type
- `Guid` used as foreign key in a relation — leaks external identity into domain relationships
- `Guid` route parameter after creation — internal `Id` is the only identity in routes
- `IHasGuid` or `IGuidResolver<TResponse>` defined in BuildingBlocks — they are contracts that belong in Shared per solution-structure.solution.skill
- Throwing `ConflictException` from `GuidResolvingBehavior` — breaks the no-exceptions-for-flow-control principle
- `GuidResolvingBehavior` constructing response DTOs — belongs in the resolver/handler
- Resolver returning a response type different from the command handler — breaks 201/409 symmetry
- `Create{Entity}Result` with fields beyond `Id` for external-created entities — violates "server returns only Id"
- Business logic in controller action: `if (task.IsComplete) return Conflict(...)` — belongs in domain
- Multiple `_sender.Send()` calls in one controller action without explicit system-level justification
- Returning 200 for a create operation — use 201 with `CreatedAtAction`
- Missing `[ProducesResponseType]` for a `ResultStatus` the handler can return — undocumented response
- Swallowing unexpected `ResultStatus` with a fallback 500 — throw `InvalidOperationException`
- Using `IMediator` instead of `ISender` — `ISender` is the correct interface for request dispatch
- Minimal API used for entity CRUD — entity lifecycle belongs in typed controllers
- Pipeline order scattered across multiple files
- `AddPipeline()` duplicated or replaced by module-specific registration methods
- Registering behaviors directly in `Program.cs` instead of inside `PipelineRegistration`
- `UnitOfWorkBehavior` registered before `ValidationBehavior` — invalid commands would open a unit of work
- `ConcurrencyBehavior` registered before `GuidResolvingBehavior` — duplicate external creates would run a version check against a non-existent entity
- `GuidResolvingBehavior` registered before `ValidationBehavior` — invalid commands would hit the database lookup
- `IRepository<T>` injected into query handler — use `IReadRepository<T>`
- Cross-module JOIN in `{Module}.Application` — Application has no cross-module DB access
- Single-module query implemented in App.Queries — unnecessary cross-module machinery
- Inline LINQ in single-module handler — use named spec
- DTO returning domain entity directly — always project to DTO record
- Query handler dispatching a command — queries are read-only
- `TaskRepository : Repository<TodoTask>` — unnecessary subclass, open generic covers all types
- Handler injects DbContext directly — use `IRepository<T>` or `IReadRepository<T>`
- `IRepository<T>` used in query handler — signals wrong intent, use `IReadRepository<T>`
- Repository method accepts `Expression<Func<T, bool>>` — all filtering goes through specs
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id)` — define `TaskByIdSpec` instead
- `GetByIdSpec` shared across entity types — each entity has its own `TaskByIdSpec`, `OrderByIdSpec`
- Business rule inside spec: `Where(t => t.Price * 0.9m > threshold)` — rule belongs in Domain, not spec
- Specs scattered across Domain and Application — all specs belong in Application
- Cross-module JOIN spec placed in a module's Application — App.Queries is the only correct location
- Shared domain model across modules — each module owns its own entities
- Direct method call into another module's Application — use MediatR
- Depending on another module's Domain for entity types — use DTOs from Interfaces
- Cross-module JOIN logic in Application — belongs in App.Queries
- Global tests folder — tests live next to their module
- Cross-module JOIN in module Application — belongs in App.Queries
- DbContext referenced from module Application — use IRepository from Shared (implemented in BuildingBlocks)
- Pipeline behaviors registered inside module registration — register once in App.Host
- Business logic in App.Host — wiring only
- `await _unitOfWork.SaveChangesAsync(ct)` in a handler — `UnitOfWorkBehavior` owns the commit
- `UnitOfWorkBehavior` without depth counter — sub-commands commit prematurely, breaking atomicity
- `UnitOfWorkContext` registered as `Singleton` — depth shared across requests, causes incorrect commit decisions
- `UnitOfWorkContext` registered as `Transient` — sub-command gets a fresh instance, depth counter never reaches 1 in nested dispatch
- `IUnitOfWork` injected into a handler — handler must trust the pipeline
- Explicit `catch { rollback }` in `UnitOfWorkBehavior` without explicit transaction — creates false safety, `DbContext.SaveChangesAsync` is already atomic
- Implementing per-request validation logic inside `ValidationBehavior`
- Returning exceptions instead of `Result.Invalid`
- Primitive on Entity instead of Value Object when the value has invariant state — loses invariant enforcement
- VO with public setter — allows post-construction mutation, invalidates immutability guarantee
- VO that throws on `ToString()` when null internal state — private constructor must not leave fields unset for EF
- Multi-property VO without private parameterless constructor — EF materialization fails silently
- Multi-property VO without `OwnsOne` config — EF creates a shadow table or fails mapping
- VO with infrastructure dependency — couples domain to persistence layer
- Reusing same VO type across modules via project reference — each module should reference Shared, not another module's Domain
- Rule throws `DomainException` itself — rule returns `bool`, the VO or entity caller throws
- `new CanDriveCarRule().IsSatisfied()` — rules are static, never instantiated
- VO rule reimplements primitive rule logic — always delegate to primitive overload
- Same business condition checked in controller, validator, entity, and service separately — define once as rule
- Rule depends on DbContext or HttpContext — pure domain predicates only
- Rule has instance state — all rules must be stateless
- Duplicating identical Value Object or Rule across multiple modules instead of placing it in Shared
- Putting module-specific VO or Rule in Shared — Shared must contain only cross-cutting primitives

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]]

# Check list
- [ ] `ICommand` and `ICommand<TResponse>` defined in `Shared/MediatR/ICommand.cs`
- [ ] All commands declared as `record` in `/{Module}.Interfaces/Commands`
- [ ] All commands implement `ICommand<Result<T>>`
- [ ] Result records co-located with their command in the same file
- [ ] Each feature has its own folder under `/{Module}.Application/Features`
- [ ] Handler file named `{FeatureName}.Handler.cs`
- [ ] Handler class named `{FeatureName}Handler`
- [ ] Handler implements `IRequestHandler<TCommand, Result<T>>`
- [ ] Handler injects `IRepository<T>` — never `DbContext`
- [ ] Handler loads entities via named specs — no inline LINQ
- [ ] Handler follows load → guard → domain call → stage → return structure
- [ ] Handler returns `Result<T>` for all outcomes — no exceptions for flow control
- [ ] Handler never calls `SaveChangesAsync`
- [ ] Cross-module writes dispatched via `_mediator.Send()`
- [ ] Module has `Register{ModuleName}Module()` extension method
- [ ] Handlers registered via `AddMediatR` assembly scan
- [ ] Validators registered via `AddValidatorsFromAssembly` assembly scan
- [ ] One validator per command in `/{Module}.Application/Features/{FeatureName}`
- [ ] Validator file named `{FeatureName}.Validator.cs`
- [ ] Validator class named `{FeatureName}Validator`
- [ ] Validator extends `AbstractValidator<TCommand>`
- [ ] Validator rules cover transport correctness only — no business rules, no DB access
- [ ] No validator exists for any query handler
- [ ] Entity prevents invalid state
- [ ] Every mutation validates before assigning
- [ ] `DomainException` thrown on invariant violation
- [ ] Domain rules from `{Module}.Domain/Rules` used for all validation
- [ ] Complex logic extracted to `{Module}.Domain/Services` static extension methods
- [ ] No property has multiple uncoordinated mutation points
- [ ] Service extensions mutate state only through entity methods or guarded setters
- [ ] Unit test use cases implemented and passed
- [ ] One config class per entity in /{Module}.Domain/Configurations
- [ ] `TableName` defined as `public const string`
- [ ] All index names defined as `public const string` constants
- [ ] All unique indexes use `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured
- [ ] `OwnsOne` configured for every multi-property VO
- [ ] No EF attributes on any domain entity
- [ ] Configurations registered via `ApplyConfigurationsFromAssembly`
- [ ] Cross-module FK configs live in App.Infrastructure/Persistence/Configurations
- [ ] DbContext uses `ApplyConfigurationsFromAssembly` on all module Domain assemblies
- [ ] `uint Version { get; internal set; }` on every mutable entity
- [ ] Every mutable entity implements `IVersioned`
- [ ] Every mutable entity config class declares a public `const string VersionedEntityName`
- [ ] `Version` mapped to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()` in entity configuration
- [ ] `IVersioned` defined in `Shared/Concurrency/IVersioned.cs`
- [ ] `IHasVersions` defined in `Shared/Concurrency/IHasVersions.cs`
- [ ] `IEntityVersionResolver` defined in `Shared/Concurrency/IEntityVersionResolver.cs`
- [ ] `ETagEncoder` defined in `BuildingBlocks/Concurrency/ETagEncoder.cs`
- [ ] `EntityByIdSpec<T>` defined in `BuildingBlocks/Specifications/EntityByIdSpec.cs`
- [ ] `ConcurrencyBehavior` defined in `BuildingBlocks/MediatR/ConcurrencyBehavior.cs`
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] `EntityVersionResolver` constructor accepts `IEnumerable<Assembly>`
- [ ] `EntityVersionResolver` scans supplied assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- [ ] `EntityVersionResolver` registered as `Singleton` in App.Host
- [ ] `EntityVersionResolver` receives module Domain assemblies from App.Host
- [ ] All update and patch commands implement `IHasVersions`
- [ ] GET for mutable entity sets `Response.Headers.ETag`
- [ ] DTO for mutable entity includes `Version` field
- [ ] PUT/PATCH checks `If-Match` — returns 412 if missing or malformed
- [ ] 412 added to `[ProducesResponseType]` on all PUT/PATCH actions
- [ ] `switch` default arm throws `InvalidOperationException` in PUT/PATCH actions
- [ ] `Guid Guid { get; internal set; }` on every external-created entity
- [ ] `Guid` set in entity factory method — never reassigned
- [ ] `UX_Guid` constant defined on entity configuration class
- [ ] Unique index on `Guid` configured with `HasDatabaseName(UX_Guid)` and `IsUnique()`
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] `IGuidResolver<TResponse>` defined in `Shared/Guid/IGuidResolver.cs`
- [ ] `ConflictResult<T>` defined in `Shared/Results/ConflictResult.cs`
- [ ] `GuidResolvingBehavior` defined in `BuildingBlocks/MediatR/GuidResolvingBehavior.cs`
- [ ] `Create{Entity}GuidResolver` in `/{Module}.Application/Resolvers`
- [ ] Resolver uses `IReadRepository<T>` and `{Entity}ByGuidSpec` — no inline LINQ
- [ ] Resolver returns null when not found, `ConflictResult<Create{Entity}Result>` when found
- [ ] `IGuidResolver<Result<Create{Entity}Result>>` registered as `Scoped` in module registration
- [ ] Create command implements `ICommand<Result<Create{Entity}Result>>` and `IHasGuid`
- [ ] `Guid` is first property in create command record
- [ ] `Create{Entity}Result` contains only the entity Id
- [ ] Handler returns `Result<Create{Entity}Result>.Created(...)` on success
- [ ] API layer maps `ConflictResult<Create{Entity}Result>` to 409 with the result body
- [ ] Each module has `/Controllers` and optionally `/MinimalApi` folders in `{Module}.Api`
- [ ] Controller naming follows five-type model
- [ ] Each controller route uses kebab-case singular noun
- [ ] Every controller action dispatches exactly one `ISender.Send()`
- [ ] `ISender` injected — never `IMediator`
- [ ] All error responses use `ProblemDetails` via `ResultExtensions`
- [ ] Every `ResultStatus` handler can return has `[ProducesResponseType]`
- [ ] `switch` default arm throws `InvalidOperationException`
- [ ] 201 Created responses use `CreatedAtAction` pointing to `Single{Entity}Controller.Get`
- [ ] Minimal API used only for non-entity-lifecycle operations
- [ ] All module Api assemblies added as application parts in App.Host
- [ ] `UseExceptionHandler()` registered before `MapControllers()`
- [ ] `AddProblemDetails()` registered in DI
- [ ] `PipelineRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] `ValidationBehavior` registered first
- [ ] `GuidResolvingBehavior` registered after `ValidationBehavior`
- [ ] `ConcurrencyBehavior` registered after `GuidResolvingBehavior`
- [ ] `UnitOfWorkBehavior` registered last
- [ ] No behavior registrations outside `PipelineRegistration.cs`
- [ ] Inline comments document the execution order
- [ ] `AddPipeline()` extension method defined in `PipelineRegistration.cs`
- [ ] `IQuery<TResponse>` defined in `Shared/MediatR/IQuery.cs`
- [ ] `IQuery` does not extend `ICommand` — queries remain distinct from write-side markers
- [ ] All queries declared as `record` implementing `IQuery<Result<T>>`
- [ ] All queries in `/{Module}.Interfaces/Queries`
- [ ] All DTOs declared as `record` in `/{Module}.Interfaces/DTOs`
- [ ] DTOs have no domain entity type properties
- [ ] Single-module handlers in `/{Module}.Application/Queries/{FeatureName}`
- [ ] Single-module handlers inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- [ ] Single-module handlers load via named specs — no inline LINQ
- [ ] Cross-module handlers in `/App.Queries/Queries/{QueryName}`
- [ ] Cross-module handlers inject `AppDbContext` directly
- [ ] Cross-module handlers apply `AsNoTracking()`
- [ ] Cross-module handlers do not use `Include()` — mapping done in handler
- [ ] `AppQueriesRegistration` defined in App.Queries
- [ ] `RegisterAppQueries()` called from App.Host
- [ ] Query transport validators (when present) check structural correctness only
- [ ] Query handlers return `Result.NotFound()` when entity is missing
- [ ] No `SaveChangesAsync` call in any query handler
- [ ] `IReadRepository<T>` defined in `Shared/Repositories`, inherits `IReadRepositoryBase<T>`
- [ ] `IRepository<T>` defined in `Shared/Repositories`, inherits `IRepositoryBase<T>` and `IReadRepository<T>`
- [ ] `IRepository<T>` has no `SaveChangesAsync`
- [ ] `Repository<T>` implemented in `App.Infrastructure/Repositories/Repository.cs`
- [ ] `Repository<T>` inherits `RepositoryBase<T>` from Ardalis
- [ ] `Repository<T>` implements `IRepository<T>`
- [ ] `Repository<T>` never calls `SaveChangesAsync`
- [ ] Open generic DI registration in `App.Host` for both interfaces
- [ ] Registered with `Scoped` lifetime
- [ ] No per-entity repository subclass exists
- [ ] Command handlers inject `IRepository<T>`
- [ ] Query handlers inject `IReadRepository<T>`
- [ ] No DbContext reference in any Application class
- [ ] Every entity loaded by Id has `{Entity}ByIdSpec` in `/{Module}.Application/Specifications`
- [ ] All specifications for the module live in `/{Module}.Application/Specifications`
- [ ] All cross-module JOIN specs live in `/App.Queries/Specifications`
- [ ] All projection specs use `Specification<T, TResult>`
- [ ] All entity filter specs use `Specification<T>`
- [ ] No inline LINQ in any handler
- [ ] Spec names reflect intent — not field names or implementation detail
- [ ] Module folder exists under /src/Modules/{ModuleName}
- [ ] Module has exactly four projects: Api, Application, Domain, Interfaces
- [ ] Interfaces has no project dependencies
- [ ] Domain depends only on Shared and EF Core config
- [ ] Application does not reference Infrastructure or App.Queries
- [ ] Api does not reference Domain or Application directly
- [ ] No direct dependency on another module's Application or Domain
- [ ] Tests colocated with module projects
- [ ] Solution folder structure matches defined layout
- [ ] Shared.csproj has no project references and contains only interface definitions
- [ ] BuildingBlocks.csproj references only Shared and contains only pattern implementations
- [ ] App.Host references BuildingBlocks and does not directly reference Shared
- [ ] App.Infrastructure is the only project with DbContext
- [ ] App.Queries contains only cross-module JOIN handlers
- [ ] App.Host is the only project referencing all modules
- [ ] No module Application references App.Infrastructure or App.Queries
- [ ] No module Domain references another module
- [ ] Pipeline behaviors registered in App.Host only
- [ ] EF entity configurations live in module Domain/Configurations
- [ ] Cross-module FK configurations live in App.Infrastructure only
- [ ] `IUnitOfWork` defined in `Shared/UnitOfWork/IUnitOfWork.cs` with single `SaveChangesAsync` method
- [ ] `UnitOfWorkContext` defined in `BuildingBlocks/MediatR/UnitOfWorkContext.cs`
- [ ] `UnitOfWorkBehavior` defined in `BuildingBlocks/MediatR/UnitOfWorkBehavior.cs`
- [ ] `UnitOfWorkBehavior` constrained to `where TRequest : ICommand`
- [ ] `UnitOfWorkBehavior` uses `try/finally` for depth decrement
- [ ] `UnitOfWorkBehavior` calls `SaveChangesAsync` only when `Depth == 1`
- [ ] `UnitOfWork` implemented in `App.Infrastructure/UnitOfWork/UnitOfWork.cs`
- [ ] `UnitOfWork` registered as `Scoped` in App.Host
- [ ] `UnitOfWorkContext` registered as `Scoped` in App.Host
- [ ] No `SaveChangesAsync` call in any handler
- [ ] No `IUnitOfWork` injection in any handler
- [ ] `ValidationBehavior` defined in `BuildingBlocks/MediatR/ValidationBehavior.cs`
- [ ] `ValidationBehavior` constrained to `where TRequest : IRequest<TResponse>` and `where TResponse : IResult`
- [ ] `ValidationBehavior` collects all errors — not fail-fast on first error
- [ ] `ValidationBehavior` returns `Result.Invalid(errors)` — not exception
- [ ] `ValidationBehavior` passes through when no validators registered
- [ ] Declared as `sealed record`
- [ ] All invariants validated in constructor
- [ ] `DomainException` thrown on violation — not null, not bool return
- [ ] No public setters
- [ ] No infrastructure or service dependencies
- [ ] Single-property VO has implicit conversion operators
- [ ] Multi-property VO has private parameterless constructor
- [ ] Multi-property VO has `OwnsOne` EF configuration on owning entity
- [ ] `ToString()` implemented when used in logs or UI
- [ ] Lives in `/{Module}.Domain/ValueObjects` or `/Shared/ValueObjects`
- [ ] Rule is a static class with static extension methods
- [ ] Rule returns `bool` — never throws
- [ ] Rule is stateless and deterministic
- [ ] Primitive rule exists as source of truth where applicable
- [ ] VO rule delegates to primitive rule — no logic duplication
- [ ] ContextualRule has primitive tuple overload as source of truth
- [ ] Named correctly: `{Type}Rules` or `{Condition}Rule`
- [ ] Lives in `/{Module}.Domain/Rules` or `/Shared/Rules`
- [ ] No infrastructure dependencies
- [ ] Cross-module VOs live in `/Shared/ValueObjects`
- [ ] Cross-module rules live in `/Shared/Rules`
- [ ] No duplicate VO/rule logic exists in multiple modules
- [ ] Entity uses VO for properties with invariant state
- [ ] Entity calls rules before mutating state
- [ ] Entity throws `DomainException` when rule returns false

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-behaviour.solution.skill/domain-behaviour.solution.skill.md|domain-behaviour]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/domain-configuration.solution.skill/domain-configuration.solution.skill.md|domain-configuration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/http-api-publication.solution.skill/http-api-publication.solution.skill.md|http-api-publication]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration-order.solution.skill/pipeline-registration-order.solution.skill.md|pipeline-registration-order]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/query-integration.solution.skill/query-integration.solution.skill.md|query-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/unit-of-work.solution.skill/unit-of-work.solution.skill.md|unit-of-work]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/validation-behavior.solution.skill/validation-behavior.solution.skill.md|validation-behavior]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/value-objects-and-rules.solution.skill/value-objects-and-rules.solution.skill.md|value-objects-and-rules]]
