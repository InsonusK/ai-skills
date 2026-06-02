---
uid: ae1e4d1a-3191-4e50-9675-647163c36dde
status: in-work
name: backend-project-structure
description: describe backend project file structure
domain: skill
type: declarative
tags:
  - dotnet
  - architectur
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
	- /[Modules](#modules-layer)
	- /[BuildingBlocks](#buildingblocks-csproj)
	- /[Shared](#shared-csproj)
	- /[Host](#host-csproj)

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
	- /[{ModuleName}.Api](#api-csproj)
	- /[{ModuleName}.Application](#application-csproj)
	- /[{ModuleName}.Domain](#domain-csproj)
	- /[{ModuleName}.Infrastructure](#infrastructure-csproj)
	- /[{ModuleName}.Infrastructure.Migrations](#infrastructure-migration-csproj)
	- /[{ModuleName}.Interfaces](#interfaces-csproj)
	- /{ModuleName}.Api.Tests
	- /{ModuleName}.Application.Tests
	- /{ModuleName}.Domain.Tests
	- /{ModuleName}.Infrastructure.Tests

__Example:__
- /Modules
	- /Task
		- /Task.Api
		- /Task.Application
		- /Task.Domain
		- /Task.Infrastructure
		- /Task.Infrastructure.Migrations
		- /Task.Interfaces
		- /Task.Api.Tests
		- /Task.Application.Tests
		- /Task.Domain.Tests
		- /Task.Infrastructure.Tests
	- /TimeLog
		- /TimeLog.Api
		- /TimeLog.Application
		- /TimeLog.Domain
		- /TimeLog.Infrastructure
		- /TimeLog.Infrastructure.Migrations
		- /TimeLog.Interfaces
		- /TimeLog.Api.Tests
		- /TimeLog.Application.Tests
		- /TimeLog.Domain.Tests
		- /TimeLog.Infrastructure.Tests

### Test Structure
Tests are co-located with modules.

__Example:__
- /Modules/Task
	-  /Task.Api.Tests
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
- [Application](#application-csproj)

### Interfaces (.csproj)
`{ModuleName}.Interfaces.csproj`

__Contains:__
- Module avaliable commands and notifications
- Module Contracts

__Structure:__
- /{ModuleName}.Interfaces  
	- /Commands  
	- /Queries  
	- /DTOs  
	- /Events  
	- /Contracts

__Rules:__
- inter-module communication layer
- stable contract boundary
- decoupling Application from external usage
- NO business logic

__Depends on:__
- [Domain](#domain-csproj)

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
- No direct Infrastructure usage
- No DbContext usage
- Orchestration only
- Only [complex_validator](???)

__Depends on:__
- [Interfaces](#interfaces-csproj)
- [Domain](#domain-csproj)
- [Shared](#shared-csproj)
- [BuildingBlocks](#buildingblocks-csproj)

### Domain (.csproj)
`{ModuleName}.Domain.csproj`

__Contains:__
- Entities
- Value Objects
- Domain Services
- Domain Events

__Structure:__
- /{ModuleName}.Domain
	- /Entities
	- /ValueObjects
	- /Services
	- /Events
	- {ModuleName}.Domain.csproj

__Rules:__
- No dependency on Application or Infrastructure
- Must enforce invariants
- Pure business logic only

### Infrastructure (.csproj)
`{ModuleName}.Infrastructure.csproj`

__Contains:__
- EF Core DbContext
- Migrations
- Repository implementations
- External integrations
- Outbox implementation

__Structure:__
- /{ModuleName}.Infrastructure
	- /Persistence
	- /Repositories
	- /Messaging
	- /External
	- {ModuleName}.Infrastructure.csproj

__Example:__
- /Task.Infrastructure
	- /Persistence
		- TaskDbContext.cs
		- /Configurations
			- TaskConfiguration.cs
	- /Repositories
		- TaskRepository.cs
	- /Migrations
	- /Outbox
		- OutboxMessage.cs
		- OutboxDispatcher.cs
	- /Messaging
		- KafkaProducer.cs
		- /EventPublishers
	- /External
		- /HttpClients
		- /ExternalServices
	- /QueryServices
		- TaskReadService.cs

__Rules:__
- Depends only on Domain
- No business logic
- No orchestration logic
- EF Core only here, SQL/DBContext only here
- External IO only hear

__Depends on:__
- [Domain](#domain-csproj)

### Infrastructure Migration (.csproj)
`{ModuleName}.Infrastructure.Migration.csproj`

__Structure:__
- /{ModuleName}.Infrastructure.Migrations  
	- /Migrations  
	- {ModuleName}DbContextFactory.cs

__Rule:__
- design-time DbContext factory
- EF migrations only
- CLI tooling target
- Doesn't contain runtime code
- Doesn't contain repository
- ONLY for creating Migrations

__Depends on:__
- [Infracstructure](#infrastructure-csproj)

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

## Host Layer (Composition Root)

### Host (.csproj)
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
### API Composition Rule
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
```
Api → Application → Domain
Infrastructure → Domain
Host → Api (all modules)
Application → Interfaces (of another Module)
```

## Forbidden
- Domain → Application
- Domain → Infrastructure
- Application → Infrastructure
- Cross-module Domain references
- API → Infrastructure directly
# Cross-Module Rules
Defines in [[cross-module-interaction]]

## Allowed
- cross-module reads via Interface layer
- shared BuildingBlocks usage
- indirect dependencies through Host composition

## Forbidden
- direct reference between module domains
- direct repository usage across modules
- direct call to Module.Application / Module.Domain

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
