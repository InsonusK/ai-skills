---
description: Provide all persistence implementation — DbContext, repository implementations, outbox interceptor, background dispatcher
name: App.Infrastructure.csproj
element_kind: project
change_kind: create
tags:
  - solution/sln-structure
  - element/app-infrastructure-csproj
---

# Goals
- Provide all persistence implementation — DbContext, repository implementations, outbox interceptor, background dispatcher
- Be the only layer that knows EF Core implementation details

# Core Principles
- App.Infrastructure implements interfaces defined in Shared
- No module Application or Domain layer references App.Infrastructure

# Structure

## Project Structure
```
/App.Infrastructure
  /Persistence
    AppDbContext.cs
    /Configurations
      OutboxMessageConfig.cs
  /Repositories
    Repository.cs
  /UnitOfWork
    UnitOfWork.cs
  /Outbox
    DomainEventInterceptor.cs
    OutboxDispatcher.cs
  /Concurrency
    EntityVersionResolver.cs
  App.Infrastructure.csproj
```

## Directory and class skills
| `Directory\|file` | Description                                    |
| ----------------- | ---------------------------------------------- |
| /Persistence      | DbContext and EF configurations                |
| /Repositories     | Generic Repository<T> implementation           |
| /UnitOfWork       | UnitOfWork implementation                      |
| /Outbox           | EF interceptor and background dispatcher       |
| /Concurrency      | EntityVersionResolver mapping strings to types |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behavior registration — belongs to App.Host
- Cross-module JOIN queries — belongs to App.Queries

# Allowed Dependencies
- BuildingBlocks
- Shared
- {ModuleName}.Domain (all modules)
- {ModuleName}.Interfaces (all modules)

# Rules

## MUST
- App.Infrastructure is the only project with DbContext
- `Repository<T>` generic implementation registered here
- DomainEventInterceptor registered on DbContext here

## MUST NOT
- App.Infrastructure be referenced by any module Application, Domain, or Api
- App.Infrastructure be referenced by App.Queries directly for DbContext
- Any module Application reference App.Infrastructure

# Anti-patterns
- Module Application referencing App.Infrastructure — use repository abstractions from Shared
- Putting cross-module JOIN queries in App.Infrastructure — belongs in App.Queries

# Check list
- [ ] AppDbContext defined here
- [ ] Generic `Repository<T>` implemented and registered
- [ ] DomainEventInterceptor registered on DbContext
- [ ] No module Application references this project
