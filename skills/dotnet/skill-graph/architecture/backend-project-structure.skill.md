---
uid: ae1e4d1a-3191-4e50-9675-647163c36dde
status: in-work
name: backend-project-structure
description: describe backend project file structure
domain: skill
type: declarative
tags:
  - dotnet
  - architecture
  - structure
triggers:
  - project initialization
  - new module creation
  - file placement decision
---
# Goal
Define strict rules for backend solution structure:
- where modules are placed
- where application layers live
- how to organize code

# Skill Scope
__This skill:__
- defines physical and logical folder structure of backend solution
- defines module layout rules
- defines layer separation inside each module
- defines naming and placement rules for CQRS, domain, infrastructure
- defines rules for cross-module referencing
- defines dependency graph

__This skill does NOT:__
- define business logic
- define implementation of commands/handlers
- define validation rules
- define API contracts
- define database schema design

# Structure
## Solution Structure (Solution Level)
__Structure:__
- /src
	- /[[#Modules Layer|Modules]]
	- /[[#BuildingBlocks (.csproj)|BuildingBlocks]]
	- /[[#Shared (.csproj)|Shared]]
	- /[[#App Layer (Composition Root)|App]]
		- [[#App Host (.csproj)|App.Host]]
		- [[#App Infrastructure (.csproj)|App.Infrastructure]]
		- [[#App Infrastructure Migrations (.csproj)|App.Infrastructure Migration]]

## Modules Layer
Each module is self-contained and an isolated bounded context.

__Structure:__
```
/Modules
	/{ModuleName}
```

__Example__:
```
/Modules
	/Task
	/TimeLog
	/User
```

### Module Structure (Project Level)
Each module is a bounded context and contains multiple .csproj projects.

__Structure:__
- /Modules/{ModuleName}
	- /[[#Api (.csproj)|{ModuleName}.Api]]
	- /[[#Application (.csproj)|{ModuleName}.Application]]
	- /[[#Domain (.csproj)|{ModuleName}.Domain]]
	- /[[#Interfaces (.csproj)|{ModuleName}.Interfaces]]
	- /{ModuleName}.Api.Tests
	- /{ModuleName}.Application.Tests
	- /{ModuleName}.Domain.Tests

__Example:__
```
/Modules
	/Task
		/Task.Api
		/Task.Application
		/Task.Domain
		/Task.Interfaces
		/Task.Api.Tests
		/Task.Application.Tests
		/Task.Domain.Tests
	/TimeLog
		/TimeLog.Api
		/TimeLog.Application
		/TimeLog.Domain
		/TimeLog.Interfaces
		/TimeLog.Api.Tests
		/TimeLog.Application.Tests
		/TimeLog.Domain.Tests
```

### Test Structure
Tests are co-located with modules.

__Example:__
```
/Modules/Task
	/Task.Api.Tests
	/Task.Application.Tests
	/Task.Domain.Tests
	/Task.Integration.Tests
```

__Rules:__
- Tests live next to production code
- No global /tests folder
- Keeps cognitive locality high in large solutions

## Module Project Responsibilities

### Api (.csproj)
`{ModuleName}.Api.csproj`

Contains:
- Controllers
- Minimal APIs
- Request/Response DTOs
- API mapping layer

__Structure__:
```
/{ModuleName}.Api
	/Controllers
	/Endpoints (Minimal API optional)
	/Contracts
		/Requests
		/Responses
	/Mappings
	{ModuleName}.Api.cspoj
```

Rules:
- No business logic
- No EF Core
- Only MediatR dispatch

Depends on:
- [[#Interfaces (.csproj)|{ModuleName}.Interfaces]] - for call commands, queries

### Interfaces (.csproj)
`{ModuleName}.Interfaces.csproj`

__Contains:__
- Commands
- Queries
- DTOs
- Integration Events
- 
__Structure:__
```
/{ModuleName}.Interfaces  
	/Commands  
	/Queries  
	/DTOs  
	/Events  
```

__Rules:__
- Stable public contract boundary — changes here are breaking changes
- NO business logic
- NO implementation — declarations only
- Cross-module queries declared here, implemented in [[#App Queries (.csproj)|App.Queries]]
- Other modules depend on this project, never on Application or Domain directly
### Application (.csproj)
`{ModuleName}.Application.csproj`

__Contains:__
- Command handlers
- Query handlers (single-module queries)
- Validators
- Complex Ardalis Specifications (cross-feature or multi-condition)

__Structure:__
```
/{ModuleName}.Application
	/Features
		/{FeatureName}
			{FeatureName}.Command.cs
			{FeatureName}.Handler.cs
			{FeatureName}.Validator.cs
	/Specifications
	{ModuleName}.Application.csproj
```

__Rules:__
- No DbContext usage — use abstractions only:
	- IRepository
	- IReadRepository
	- IUnitOfWork
	- IQueryExecutor
- Orchestration only — loads data, calls domain, saves
- Complex Ardalis Specifications live here when they span multiple features

__Depends on:__
- [[#Interfaces (.csproj)|{ModuleName}.Interfaces]] - implements handlers for commands and in module queries
- [[#Interfaces (.csproj)|{Other ModuleName}.Interfaces]] - using other moduler command and queries
- [[#Domain (.csproj)|{ModuleName}.Domain]] - user in handler realization
- [[#Shared (.csproj)|Shared]]
- [[#BuildingBlocks (.csproj)|BuildingBlocks]]

### Domain (.csproj)
`{ModuleName}.Domain.csproj`

__Contains:__
- Entities — see [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|entity-pattern.skill]]
- Value Objects — see [[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill|value-object-pattern.skill]]
- Domain Services — see [[skills/dotnet/skill-graph/Domain Layer/domain-service.skill|domain-service.skill]]
- Domain Events — see [[skills/dotnet/skill-graph/Domain Layer/domain-event-pattern.skill]]
- Rules — see [[skills/dotnet/skill-graph/Domain Layer/domain-rule-pattern.skill|domain-rule-pattern.skill]]
- EF Configurations — see [[skills/dotnet/skill-graph/Domain Layer/domain-configuration-pattern.skill|domain-configuration-pattern.skill]]
- Simple Ardalis Specifications (entity-scoped, reusable filters)

__Structure:__
- /{ModuleName}.Domain
	- /[[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|Entities]]
	- /[[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill|Value Objects]]
	- /[[skills/dotnet/skill-graph/Domain Layer/domain-service.skill|Services]]
	- /Events
	- /[[skills/dotnet/skill-graph/Domain Layer/domain-rule-pattern.skill|Rules]]
	- /[[skills/dotnet/skill-graph/Domain Layer/domain-configuration-pattern.skill|Configuration]]
	- /Specifications
	- {ModuleName}.Domain.csproj

__Rules:__
- Pure business logic only — no orchestration, no IO
- Pure business logic only
- Must enforce invariants
- EF Configurations live here — Domain references `Microsoft.EntityFrameworkCore` for `IEntityTypeConfiguration<T>` only; no runtime EF usage
- Simple Specifications (single-entity filters) live here
- Complex Specifications (multi-condition, cross-feature) live in Application

__Depends on:__
- `Microsoft.EntityFrameworkCore` (for IEntityTypeConfiguration only)
- [[#Shared (.csproj)|Shared]]

## Shared / Building Blocks

### Shared (.csproj)
Cross-module primitives — no business logic, no infrastructure:

__Contains:__
- Result types
- Exceptions
- Logging abstractions
- Common base types

__Example__:
```
/Shared 
	/Common 
		Result.cs  
		Error.cs  
	/Exceptions  
		DomainException.cs  
	/Primitives  
		Entity.cs  
		ValueObject.cs  
	/Abstractions  
		IClock.cs
```
### BuildingBlocks (.csproj)
Reusable infrastructure patterns — framework-level, not business-level:

__Contains:__
- MediatR pipelines
- Specification base
- Outbox base
- EF extensions

__Example:__
```
- /BuildingBlocks  
	- /MediatR  
		- /PipelineBehaviors
	- /Specification 
		- BaseSpecification.cs  
	- /Outbox  
		- OutboxBase.cs  
	- /EF  
		- DbContextExtensions.cs  
	- /Logging  
		- LoggingBehavior.cs
```

## App Layer (Composition Root)
### App Infrastructure (.csproj)
`App.Infrastructure.csproj`

__Responsible for:__
- Central persistence and external integration layer.  
  
__Contains:__  
- DbContext (single shared context)
- EF Configurations cross-module foreign keys only
- Migrations  
- Outbox implementation 
- Messaging integrations (MediatR -> outbox -> message broker)
- Repository implementations
- Unit of Work implementation

__Structure:__
```
/App.Infrastructure  
	/Persistence  
		AppDbContext.cs  
		/Configuration ← cross-module FK configs only
/Migrations  
/Outbox  
/Messaging  
/Queries  
/External
```

__Rules:__
- ONLY place allowed to access DbContext at runtime
- Depends only on Domain and implements cross model queries from Interfaces
- No business logic
- No orchestration logic
- Cross-module FK configurations live here — all other EF configs live in Domain
- Outbox pattern used for all integration events — see [[skills/dotnet/skill-graph/Domain Layer/domain-event-pattern.skill]]

__Depends on:__
- [[#Domain (.csproj)|{All Modules}.Domain]] - get Module entities 
- [[#Interfaces (.csproj)|{All Modules}.Interfaces]] - realize cross join module queries
- [[#BuildingBlocks (.csproj)|BuildingBlocks]]

### App Queries (.csproj)
`App.Queries.csproj`

__Responsible for:__
- Cross-module read model execution
- Reporting projections
- Any query requiring JOINs across module boundaries

__Contains:__
- Implementations of cross-module queries declared in `{Module}.Interfaces`
- Complex Ardalis Specifications that join across module boundaries
- Read-only DTOs for projections

__Structure:__
```
/App.Queries
    /Queries
        /{ModuleName}
            {QueryName}Handler.cs
    /Specifications
    App.Queries.csproj
```

__Rules:__
- Read-only — no writes, no commands
- No business logic — projection and mapping only
- Implements query contracts declared in `{Module}.Interfaces`
- Cross-module JOINs allowed and expected here
- Must not implement queries that can be served by a single module

__Depends on:__
- [[#App Infrastructure (.csproj)|App.Infrastructure]] - accesses DbContext for query execution
- [[#Interfaces (.csproj)|{All Modules}.Interfaces]] - implements cross-module query contracts
- [[#Domain (.csproj)|{All Modules}.Domain]] - accesses entities for projections

### App Infrastructure Migrations (.csproj)
`App.Infrastructure.Migrations.csproj`

__Structure:__
```
/App.Infrastructure.Migrations  
	/Migrations  
	DbContextFactory.cs
```

__Rule:__
- design-time DbContext factory only
- EF migrations only — CLI tooling target
- No runtime code
- No repositories
- ONLY for generating and applying migrations

__Depends on:__
- [[#App Infrastructure (.csproj)|App Infrastructure]]

### App Host (.csproj)
`Host.csproj`

__Responsible for:__
- ASP.NET Core startup
- DI composition
- Module registration
- API aggregation

__Structure__:
```
/Host  
	/DependencyInjection  
	/ModuleRegistration  
	/Middleware  
	/Routing
	/Program.cs  
	/Host.csproj
```

__Rules:__
- Composition root only — no business logic, no queries
- Registers all modules, infrastructure, and queries into DI container

__Depends on:__
- [[#Api (.csproj)|{All Modules}.Api]]
- [[#Application (.csproj)|{All Modules}.Application]] - registers handlers
- [[#App Infrastructure (.csproj)|App.Infrastructure]] - registers persistence
- [[#App Queries (.csproj)|App.Queries]] - registers cross-module query handlers

#### API Composition Rule
Host exposes unified API surface:
```
Modules/*/Api -> Host endpoint registration
```

__Host:__
- references all module Api projects
- maps controllers/minimal APIs into single application
- defines global middleware pipeline

# Dependency Rules

## Allowed
[[#App Host (.csproj)|App.Host]] -> [[#Api (.csproj)|{ModuleName}.Api]]
[[#App Host (.csproj)|App.Host]] -> [[#App Infrastructure (.csproj)|App.Infrastructure]]
[[#App Host (.csproj)|App.Host]] -> [[#Application (.csproj)|{ModuleName}.Application]]
[[#App Host (.csproj)|App.Host]] -> [[#App Queries (.csproj)|App.Queries]]

[[#Api (.csproj)|{ModuleName}.Api]] -> [[#Interfaces (.csproj)|{ModuleName}.Interfaces]]

[[#App Infrastructure Migrations (.csproj)|App.Infrastructure Migration]] -> [[#App Infrastructure (.csproj)|App.Infrastructure]]

[[#App Queries (.csproj)|App.Queries]] -> [[#App Infrastructure (.csproj)|App.Infrastructure]] 
[[#App Queries (.csproj)|App.Queries]] -> [[#Domain (.csproj)|{ModuleName}.Domain]]
[[#App Queries (.csproj)|App.Queries]] -> [[#Interfaces (.csproj)|{ModuleName}.Interfaces]]

[[#App Infrastructure (.csproj)|App.Infrastructure]] -> [[#Interfaces (.csproj)|{ModuleName}.Interfaces]]
[[#App Infrastructure (.csproj)|App.Infrastructure]] -> [[#Domain (.csproj)|{ModuleName}.Domain]]
[[#App Infrastructure (.csproj)|App.Infrastructure]] -> [[#BuildingBlocks (.csproj)|BuildingBlocks]]

[[#Application (.csproj)|{ModuleName}.Application]] -> [[#Interfaces (.csproj)|{ModuleName}.Interfaces]] 
[[#Application (.csproj)|{ModuleName}.Application]] ->  [[#Domain (.csproj)|{ModuleName}.Domain]] 
[[#Application (.csproj)|{ModuleName}.Application]] -> [[#Shared (.csproj)|Shared]]
[[#Application (.csproj)|{ModuleName}.Application]] -> [[#BuildingBlocks (.csproj)|BuildingBlocks]]

[[#Domain (.csproj)|{ModuleName}.Domain]] -> [[#Shared (.csproj)|Shared]]
[[#Domain (.csproj)|{ModuleName}.Domain]] -> Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)
## Forbidden
- [[#Domain (.csproj)|{ModuleName}.Domain]] -> [[#Application (.csproj)|{ModuleName}.Application]]
- [[#Domain (.csproj)|{ModuleName}.Domain]] -> [[#App Infrastructure (.csproj)|App.Infrastructure]]
- [[#Domain (.csproj)|{ModuleName}.Domain]] -> [[#App Queries (.csproj)|App.Queries]]
- [[#Domain (.csproj)|{ModuleName}.Domain]] -> [[#Domain (.csproj)|{OtherModuleName}.Domain]] (cross-module domain reference)
- [[#Application (.csproj)|{ModuleName}.Application]] -> [[#App Infrastructure (.csproj)|App.Infrastructure]]
- [[#Application (.csproj)|{ModuleName}.Application]] ->[[#App Queries (.csproj)|App.Queries]]
- [[#Application (.csproj)|{ModuleName}.Application]] -> [[#Application (.csproj)|{OtherModuleName}.Application]] 
- [[#Api (.csproj)|{ModuleName}.Api]] -> [[#App Infrastructure (.csproj)|App.Infrastructure]] 
- [[#Api (.csproj)|{ModuleName}.Api]] ->[[#App Queries (.csproj)|App.Queries]]

# Anti Goals
- Do not centralize tests in root folder
- Do not bypass Host composition layer
- Do not reference Infrastructure from Application, Domain, or API
- Do not put cross-module JOINs in Application or Domain
- Do not put business logic in App.Queries
- Do not collapse modules into shared monolith
- Do not flatten structure into only layers without modules
- Do not mix module APIs directly

# Check List
- [ ] Each module has its own .csproj separation
- [ ] Domain is pure and independent    
- [ ] Application does not depend on Infrastructure    
- [ ] API is thin and dispatch-only    
- [ ]  Cross-module queries implemented in App.Queries, declared in Interfaces
- [ ]  EF entity configurations live in Domain/Configurations
- [ ]  Cross-module FK configurations live in App.Infrastructure only
- [ ]  Host composes all module APIs, infrastructure, and queries
- [ ]  Tests are colocated with modules
- [ ]  No cross-module Domain references
- [ ]  File placement follows rules consistently
