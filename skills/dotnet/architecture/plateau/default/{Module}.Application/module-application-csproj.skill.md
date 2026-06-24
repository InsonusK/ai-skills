---
uid: f563cd3b-7156-437a-9e39-77b7af130fe1
name: module-application-csproj
description: Orchestrate use cases by connecting the API contract to the domain model
domain: skill
type: template
version: 20260622
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change-solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification-solution.skill]]"
---

# Goal
- Orchestrate use cases by connecting the API contract to the domain model
- Ensure all command handlers use `IRepository<T>` for write-staging
- Ensure all query handlers use `IReadRepository<T>` for read-only access
- Keep Application layer decoupled from DbContext and Infrastructure
- Store all specifications for this module in Application — simple, multi-condition, projection, and idempotency
- Own single-module query handler implementations in `/Queries` alongside command handlers
- No validator alongside query handlers — queries are read-only
- Own `Create{Entity}GuidResolver` implementations in `/Resolvers` — one per external-created entity type
- Host `{Entity}ByGuidSpec` in `/Specifications`
- Register each resolver in the module DI registration
- Own all CommandHandler implementations, per-command validators, and the module's DI registration
- Realize `IEntityVersionResolver` for every versioned entity in the module
- Keep version loading next to the module's specifications and repositories
- Structure each feature as a vertical slice — handler and validator co-located in one folder
- Self-register all handlers and validators via assembly scan

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Core Principals
- Application coordinates — it never contains business logic
- Application knows its own Domain and its own Interfaces
- Application may reference other modules' Interfaces for cross-module dispatch
- Command handlers inject `IRepository<T>` — they may read and stage changes
- Query handlers inject `IReadRepository<T>` — they only read, signaling intent
- No handler references `AppDbContext` or `App.Infrastructure` directly
- All specifications for this module live in one place — `/{Module}.Application/Specifications`
- Simple single-condition specs are reusable across features and event handlers
- Multi-condition or feature-specific specs belong to one use case
- Projection specs map entities to DTOs for read models
- Query handlers located under `/Queries` — one folder per feature, separate from command handlers
- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler` — same convention as commands
- `.Validator.cs` file is optional alongside query handlers — transport correctness only when needed
- Single-module handlers only — cross-module JOIN handlers live in App.Queries
- One feature folder per write operation under `/Features`
- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler`
- Validator file named `{FeatureName}.Validator.cs`, class named `{FeatureName}Validator`
- Each module exposes one `Register{ModuleName}Module()` extension method
- Pipeline behaviors are NOT registered here — that is App.Host's responsibility
- Each versioned entity has a dedicated `{Entity}VersionResolver` class in `{Module}.Application/Concurrency`
- Resolvers use the module's `{Entity}ByIdSpec` and `IReadRepository<{Entity}>` from Shared
- Resolver's `VersionedEntityName` constant matches the Domain config constant exactly

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Application
```


## Project Structure
```
/{ModuleName}.Application
  /Handlers
  /Validators
  /Specifications
  {ModuleName}.Application.csproj
```

```
/{Module}.Application
  /Specifications
    {Entity}ByIdSpec.cs
    {Entity}ByGuidSpec.cs
    Active{Entities}Spec.cs
    Active{Entities}By{Owner}Spec.cs
    {Entity}SummarySpec.cs
    {Entity}ByEventIdSpec.cs
  /Features
    /{FeatureName}
      {Command}Handler.cs
      {Query}Handler.cs
```

```
/{Module}.Application
  /Features
    /CreateTask
      CreateTask.Handler.cs     ← command handler
      CreateTask.Validator.cs   ← command validator
  /Queries
    /GetTask
      GetTask.Handler.cs        ← query handler
      GetTask.Validator.cs      ← optional transport validator
    /GetTasks
      GetTasks.Handler.cs       ← query handler
  /Specifications
    TaskByIdSpec.cs             ← repository-integration
    TaskSummarySpec.cs          ← repository-integration
```

```
/{Module}.Application
  /Features
    /Create{Entity}
      Create{Entity}.Handler.cs
      Create{Entity}.Validator.cs
  /Specifications
    {Entity}ByGuidSpec.cs
  /Resolvers
    Create{Entity}GuidResolver.cs
  {Module}ApplicationRegistration.cs
```

```
/{Module}.Application
  /Concurrency
    {Entity}VersionResolver.cs
  /Features
    /{FeatureName}
      {FeatureName}.Handler.cs
      {FeatureName}.Validator.cs
  {Module}ApplicationRegistration.cs
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

## Directory and class skills
| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
| /Handlers | Command and query handlers |  |
| /Validators | Input validation |  |
| /Specifications | Query specifications |  |
| /Specifications | All module specifications — single-condition, multi-condition, projection, idempotency |  |
| /Queries/{FeatureName} | One folder per query feature; contains handler and optional transport validator |  |
| /Specifications | Named reusable specifications for entity loading and projection |  |
| /Specifications/{Entity}ByGuidSpec.cs | Specification for looking up entity by Guid | [[skills/dotnet/architecture/plateau/default/{Module}.Application/classes/entity-by-guid-spec-class.skill.md|EntityByGuidSpec.class.skill]] |
| /Resolvers/Create{Entity}GuidResolver.cs | Per-entity IGuidResolver implementation | [[skills/dotnet/architecture/plateau/default/{Module}.Application/classes/create-entity-guid-resolver-class.skill|CreateEntityGuidResolver.class.skill]] |
| /Features/{FeatureName} | One subfolder per feature — handler and validator co-located |  |
| {FeatureName}.Handler.cs | Command handler implementation | [[skills/dotnet/architecture/plateau/default/{Module}.Application/classes/Feature.Handler.class.skill.md|Feature.Handler.class.skill]] |
| {FeatureName}.Validator.cs | Transport correctness validator | [[skills/dotnet/architecture/plateau/default/{Module}.Application/classes/Feature.Validator.class.skill.md|Feature.Validator.class.skill]] |
| /Concurrency/{Entity}VersionResolver.cs | Reads the current version for one versioned entity | [[skills/dotnet/architecture/plateau/default/{Module}.Application/classes/{Entity}VersionResolver.class.skill.md|{Entity}VersionResolver.class.skill]] |
| {Module}ApplicationRegistration.cs | Module DI self-registration extension | [[skills/dotnet/architecture/plateau/default/{Module}.Application/classes/ModuleApplicationRegistration.class.skill.md|ModuleApplicationRegistration.class.skill]] |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Ardalis.Specification` | latest stable | For creating `Specification<T>` and `Specification<T, TResult>` classes |
| `MediatR` | latest stable | Handler implements `IRequestHandler<TQuery, Result<T>>` |
| `Ardalis.Result` | latest stable | Return typed results |
| `Ardalis.Specification` | latest stable | Named specs for loading and projection |
| `MediatR` | latest stable | Provides `IRequestHandler<TRequest, TResponse>` |
| `Ardalis.Result` | latest stable | Provides `Result<T>` return type |
| `FluentValidation` | latest stable | Provides `AbstractValidator<T>` |
| `FluentValidation.DependencyInjectionExtensions` | latest stable | Provides `AddValidatorsFromAssembly` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Infrastructure implementations — belongs to App.Infrastructure
- Cross-module JOIN queries — belongs to App.Queries

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

## Allowed Dependencies
- {Module}.Interfaces (own module)
- {Module}.Domain (own module)
- {OtherModule}.Interfaces (other modules — contracts only)
- Shared
- `{Module}.Domain`
- `{Module}.Interfaces`
- MediatR
- Shared — for `IQuery<T>` and `IReadRepository<T>`
- `{Module}.Domain` — for entity types used in specs
- `{Module}.Interfaces` — for query and DTO types
- BuildingBlocks
- {Module}.Domain
- {Module}.Interfaces
- `{OtherModule}.Interfaces` for cross-module dispatch

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Rules
MUST:
	- Application references only own Interfaces, own Domain
	- Command handlers inject `IRepository<T>` from Shared
	- Query handlers inject `IReadRepository<T>` from Shared
	- All entity loading uses named specs — no inline `Where(...)` LINQ
	- All specs for this module live in `/{Module}.Application/Specifications`
	- Every entity loadable by `Id` has a `{Entity}ByIdSpec`
	- Idempotency specs for event handlers live in `/Specifications` — not inside `/Features`
	- Single-module query handlers live in `/{Module}.Application/Queries/{FeatureName}`
	- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler`
	- Query handlers registered via `AddMediatR` assembly scan in module registration — same scan as command handlers
	- Query handlers inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
	- One `GuidResolver` per external-created entity type in `/{Module}.Application/Resolvers`
	- Each resolver registered in module DI registration
	- All specs live in `/{Module}.Application/Specifications`
	- Each feature in its own subfolder under `/Features`
	- Handler file named `{FeatureName}.Handler.cs`
	- Handler class named `{FeatureName}Handler`
	- Validator file named `{FeatureName}.Validator.cs`
	- Validator class named `{FeatureName}Validator`
	- Module exposes `Register{ModuleName}Module(IServiceCollection, IConfiguration)` extension method
	- Handlers registered via `AddMediatR` assembly scan
	- Validators registered via `AddValidatorsFromAssembly`
MUST NOT:
	- Application reference another module's Domain
	- Application reference another module's Application
	- Application contain business logic — delegate to Domain
	- Any Application class reference `AppDbContext`
	- Query handlers inject `IRepository<T>`
	- Specs placed in `{Module}.Domain` — Application is the single spec location
	- Query validators contain business rules — transport correctness only
	- Cross-module JOIN handlers live here — belongs in App.Queries
	- Reference `DbContext` directly
	- Resolver implemented in Domain — resolver uses `IReadRepository<T>`, which belongs in Application
	- Specs placed in Domain
	- Pipeline behaviors registered inside module registration
	- Handler contain business logic — delegate to domain entities and services
	- Handler call `SaveChangesAsync`
	- Handler reference `DbContext` directly — use `IRepository<T>` from Shared
	- Validator inject repositories or services — purely declarative
	- Validator contain business rules

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Anti-patterns
- Calling another module's Application method directly — use MediatR dispatch through Interfaces
- Writing business rules in a handler — delegate to entity or domain service
- `private readonly AppDbContext _dbContext` in a handler
- `IRepository<T>` injected into a query handler
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id)` — use named spec
- Single-condition spec duplicated across modules — reuse via shared interfaces, not copied specs
- Query handler placed outside `/Queries` — keep query handlers under `/Queries`
- `.Validator.cs` next to query handler — queries are read-only, no validation needed
- `IGuidResolver` implemented in Domain — resolver uses `IReadRepository<T>`, belongs in Application
- `CreateTaskCommandHandler.cs` as file name — use `CreateTask.Handler.cs`
- Manual handler registration: `services.AddTransient<CreateTaskHandler>()` — use assembly scan
- Business rule in handler or validator
- Validator placed outside its feature folder

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]

# Check list
- [ ] Application.csproj does not reference another module's Domain or Application
- [ ] No business logic in any handler class
- [ ] Command handlers use `IRepository<T>`
- [ ] Query handlers use `IReadRepository<T>`
- [ ] No DbContext references in Application
- [ ] All reads go through named specifications
- [ ] `/Specifications` folder exists in Application
- [ ] `{Entity}ByIdSpec` exists for every entity loadable by Id
- [ ] Idempotency specs live in `/Specifications`
- [ ] Single-module handlers in `/{Module}.Application/Queries/{FeatureName}`
- [ ] Handler file named `{FeatureName}.Handler.cs`
- [ ] Handler class named `{FeatureName}Handler`
- [ ] Optional `.Validator.cs` paired with query handler checks transport correctness only
- [ ] Query handlers inject `IReadRepository<T>`
- [ ] `Create{Entity}GuidResolver` in `/{Module}.Application/Resolvers`
- [ ] `{Entity}ByGuidSpec` in `/{Module}.Application/Specifications`
- [ ] Resolver registered in module DI
- [ ] `{Entity}VersionResolver` exists for every versioned entity
- [ ] Resolver implements `IEntityVersionResolver`
- [ ] `VersionedEntityName` constant matches Domain config
- [ ] Resolver uses `IReadRepository<{Entity}>` and `{Entity}ByIdSpec`
- [ ] Returns `0` for missing entity
- [ ] `/Features/{FeatureName}` folder exists for each command
- [ ] Validator file named `{FeatureName}.Validator.cs`
- [ ] `{Module}ApplicationRegistration.cs` exists
- [ ] Handlers registered via `AddMediatR` scan
- [ ] Validators registered via `AddValidatorsFromAssembly` scan

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill.md|solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/repository-integration-solution.skill.md|repository-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/query-integration-solution.skill.md|query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/query-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/external-created-entity-solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/command-integration-solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-classification-solution.skill/entity-classification-solution.skill.md|entity-classification]]
