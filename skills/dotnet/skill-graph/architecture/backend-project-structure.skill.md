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
- /Modules
	- /{ModuleName}

__Example__:
- /Modules
	- /Task
	- /TimeLog
	- /User

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
- /Modules
	- /Task
		- /Task.Api
		- /Task.Application
		- /Task.Domain
		- /Task.Interfaces
		- /Task.Api.Tests
		- /Task.Application.Tests
		- /Task.Domain.Tests
	- /TimeLog
		- /TimeLog.Api
		- /TimeLog.Application
		- /TimeLog.Domain
		- /TimeLog.Interfaces
		- /TimeLog.Api.Tests
		- /TimeLog.Application.Tests
		- /TimeLog.Domain.Tests

### Test Structure
Tests are co-located with modules.

__Example:__
- /Modules/Task
	- /Task.Api.Tests
	- /Task.Application.Tests
	- /Task.Domain.Tests
	- /Task.Integration.Tests

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
- /{ModuleName}.Api
	- /Controllers
	- /Endpoints (Minimal API optional)
	- /Contracts
		- /Requests
		- /Responses
	- /Mappings
	- {ModuleName}.Api.cspoj

Rules:
- No business logic
- No EF Core
- Only MediatR dispatch

Depends on:
- [[#Interfaces (.csproj)|{ModuleName}.Interfaces]] - for call commands, queries

### Interfaces (.csproj)
`{ModuleName}.Interfaces.csproj`

__Contains:__
- Module available commands and notifications

__Structure:__
- /{ModuleName}.Interfaces  
	- /Commands  
	- /Queries  
	- /DTOs  
	- /Events  

__Rules:__
- inter-module communication layer
- stable contract boundary
- decoupling Application from external usage
- NO business logic
- realize cross-module join queries

### Application (.csproj)
`{ModuleName}.Application.csproj`

__Contains:__
- Commands
- Queries
- Handlers
- Validators
- Application services

__Structure:__
- /{ModuleName}.Application
	- /Features
		- /{FeatureName}
			- {FeatureName}.Command.cs
			- {FeatureName}.Handler.cs
			- {FeatureName}.Validator.cs
	- {ModuleName}.Application.csproj

__Rules:__
- No DbContext usage
	- use abstractions:
		- IRepository
		- IReadRepository
		- IUnitOfWork  
		- IQueryExecutor
- Orchestration only
- Implements model and complex validators

__Depends on:__
- [[#Interfaces (.csproj)|{ModuleName}.Interfaces]] - implements handlers for commands and in module queries
- [[#Interfaces (.csproj)|{Other ModuleName}.Interfaces]] - using other moduler command and queries
- [[#Domain (.csproj)|{ModuleName}.Domain]] - user in handler realization
- [[#Shared (.csproj)|Shared]]
- [[#BuildingBlocks (.csproj)|BuildingBlocks]]

### Domain (.csproj)
`{ModuleName}.Domain.csproj`

__Contains:__
- Entities
- Value Objects
- Domain Services
- Domain Events

__Structure:__
- /{ModuleName}.Domain
	- /[[entity-pattern.skill|Entities]]
	- /[[value-object-pattern.skill|Value Objects]]
	- /Services
	- /Events
	- /Validators
	- {ModuleName}.Domain.csproj

__Rules:__
- mutation allowed ONLY by internal access methods and setters
- allow internal access to {ModuleName}.Application
- Pure business logic only
- Must enforce invariants
- Implements value and property validators

## Shared / Building Blocks

### Shared (.csproj)
__Cross-module primitives:__
- Result types
- Exceptions
- Logging abstractions
- Common base types

__Example__:
- /Shared 
	- /Common 
		- Result.cs  
		- Error.cs  
	- /Exceptions  
		- DomainException.cs  
	- /Primitives  
		- Entity.cs  
		- ValueObject.cs  
	- /Abstractions  
		- IClock.cs
### BuildingBlocks (.csproj)
Reusable infrastructure patterns:
- MediatR pipelines
- Specification base
- Outbox base
- EF extensions

__Example:__
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

## App Layer (Composition Root)
### App Infrastructure (.csproj)
`App.Infrastructure.csproj`

__Responsible for:__
- Central persistence + external integration layer.  
  
__Contains:__  
- DbContext (shared)  
- EF Configurations for foreing keys between modules
- Migrations  
- Outbox implementation
- Messaging integrations 
- External integrations  
- Cross-module query execution (JOIN allowed)
	- for read models
	- for reporting

__Structure:__
- /App.Infrastructure  
	- /Persistence  
		- AppDbContext.cs  
		- /Configuration
- /Migrations  
- /Outbox  
- /Messaging  
- /Queries  
- /External

__Rules:__
- ONLY place allowed to access database
- Depends only on Domain and realize cross model queries from Interfaces
- No business logic
- No orchestration logic
- External IO only here

__Depends on:__
- [[#Domain (.csproj)|{All Modules}.Domain]] - get Module entities 
- [[#Interfaces (.csproj)|{All Modules}.Interfaces]] - realize cross join module queries

### App Infrastructure Migrations (.csproj)
`App.Infrastructure.Migrations.csproj`

__Structure:__
- /App.Infrastructure.Migrations  
	- /Migrations  
	- DbContextFactory.cs

__Rule:__
- design-time DbContext factory
- EF migrations only
- CLI tooling target
- Doesn't contain runtime code
- Doesn't contain repository
- ONLY for creating Migrations

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
- /Host  
	- /DependencyInjection  
	- /ModuleRegistration  
	- /Middleware  
	- /Routing
	- /Program.cs  
	- /Host.csproj

__Depends on:__
- [[#Api (.csproj)|{All Modules}.Api]]
- [[#App Infrastructure (.csproj)|App.Infrastructure]] - add to DI
- [[#Application (.csproj)|{ModuleName}.Application]] - add to DI

#### API Composition Rule
Host exposes unified API surface:
```
Modules/*/Api → Host endpoint registration
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

[[#Api (.csproj)|{ModuleName}.Api]] -> [[#Interfaces (.csproj)|{ModuleName}.Interfaces]]

[[#App Infrastructure Migrations (.csproj)|App.Infrastructure Migration]] -> [[#App Infrastructure (.csproj)|App.Infrastructure]]

[[#App Infrastructure (.csproj)|App.Infrastructure]] -> [[#Interfaces (.csproj)|{ModuleName}.Interfaces]]
[[#App Infrastructure (.csproj)|App.Infrastructure]] -> [[#Domain (.csproj)|{ModuleName}.Domain]]

[[#Application (.csproj)|{ModuleName}.Application]] -> [[#Interfaces (.csproj)|{ModuleName}.Interfaces]] 
[[#Application (.csproj)|{ModuleName}.Application]] ->  [[#Domain (.csproj)|{ModuleName}.Domain]] 
[[#Application (.csproj)|{ModuleName}.Application]] -> [[#Shared (.csproj)|Shared]]
[[#Application (.csproj)|{ModuleName}.Application]] -> [[#BuildingBlocks (.csproj)|BuildingBlocks]]

## Forbidden
- [[#Domain (.csproj)|{ModuleName}.Domain]] → [[#Application (.csproj)|{ModuleName}.Application]]
- [[#Domain (.csproj)|{ModuleName}.Domain]] → [[#App Infrastructure (.csproj)|App.Infrastructure]]
- [[#Application (.csproj)|{ModuleName}.Application]] → [[#App Infrastructure (.csproj)|App.Infrastructure]]
- Cross-module Domain references
- API → Infrastructure directly

# Anti Goals
- Do not centralize tests in root folder
- Do not bypass Host composition layer
- Do not mix module APIs directly
- Do not reference Infrastructure from Application or API
- Do not collapse modules into shared monolith
- Do not flatten structure into only layers without modules

# Check List
- [ ] Each module has its own .csproj separation
- [ ] Domain is pure and independent    
- [ ] Application does not depend on Infrastructure    
- [ ] API is thin and dispatch-only    
- [ ] Host composes all module APIs    
- [ ] Tests are colocated with modules    
- [ ] No cross-module direct dependencies    
- [ ] File placement follows rules consistently
