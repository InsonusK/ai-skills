---
name: solution-unit-of-work
description: Defines IUnitOfWork, UnitOfWorkContext, and UnitOfWorkBehavior — the pipeline mechanism that commits all staged entity changes atomically after the top-level command handler completes, ensuring sub-commands never commit prematurely
domain: skill
type: architecture
version: 20260629
tags:
  - skill/architecture/solution
  - stack/dotnet
  - application
  - infrastructure
  - unit-of-work
  - framework/mediatr
  - framework/ef-core
  - pipeline
  - concern/architecture
  - solution/unit-of-work

triggers:
  - define unit of work
  - commit changes after handler
  - atomic command transaction
  - UnitOfWorkBehavior
  - SaveChanges pattern
  - nested command dispatch
creates:
  - Shared.UnitOfWork.IUnitOfWork.cs
  - BuildingBlocks.MediatR.UnitOfWorkContext.cs
  - BuildingBlocks.MediatR.UnitOfWorkBehavior.cs
  - App.Infrastructure.UnitOfWork.UnitOfWork.cs
extends:
  - Shared.csproj
  - BuildingBlocks.csproj
  - App.Infrastructure.csproj
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]"
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|solution-pipeline-registration]]"
---

# Goal
- Define `IUnitOfWork` as the single commit point for all staged entity changes — the only place `SaveChangesAsync` is called
- Define `UnitOfWorkBehavior` as the pipeline mechanism that automatically commits after the top-level command handler completes
- Define `UnitOfWorkContext` as the nesting depth tracker that prevents sub-commands from committing prematurely
- Ensure all changes from a command and all its sub-commands are committed atomically in a single `SaveChangesAsync` call
- Enforce that no handler ever calls `SaveChangesAsync` directly

# Capabilities
- Atomic commit of all staged changes in a single `SaveChangesAsync`
- Prevention of premature commits for nested sub-commands
- No explicit rollback needed thanks to EF implicit transactions
- Clear separation: handlers stage changes, pipeline commits them
- Consistent commit behavior across all commands

# Core Principles
- `IUnitOfWork` lives in Shared — every layer can reference the commit contract without coupling to infrastructure
- `IUnitOfWork` is the only component that calls `SaveChangesAsync` — handlers, repositories, and domain services never call it
- `UnitOfWorkBehavior` and `UnitOfWorkContext` live in BuildingBlocks — they reference `ICommand` and `IUnitOfWork` from Shared
- `UnitOfWorkBehavior` activates only on `ICommand` — queries never trigger a commit
- `UnitOfWorkContext` tracks nesting depth — only the outermost command (`Depth == 1`) commits
- Sub-commands dispatched via `_mediator.Send()` call `_context.Enter()` — they stage changes but defer commit to the root
- **No explicit rollback needed** — EF Core uses implicit transactions: if `SaveChangesAsync` is never called, the DbContext is disposed at request scope end and all staged changes are silently abandoned. Explicit rollback is only required when using `DbContext.Database.BeginTransactionAsync()`, which this architecture does not use.
- If the handler throws, `SaveChangesAsync` is never called — all staged changes are discarded with the request scope
- `UnitOfWorkContext` is registered as `Scoped` — one instance per HTTP request, shared across all nested command dispatches within that request

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/Implementation/Shared.csproj.create|Shared.csproj]] - hosts `IUnitOfWork` commit contract
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/Implementation/BuildingBlocks.csproj.create|BuildingBlocks.csproj]] - hosts `UnitOfWorkContext` and `UnitOfWorkBehavior`
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]] - hosts `UnitOfWork` EF Core implementation
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Host.csproj.create|App.Host.csproj]] - hosts `IUnitOfWork` and `UnitOfWorkContext` registrations
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `IRepository<T>` that stages changes for `IUnitOfWork` to commit
    - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]] - write-side repository used by command handlers
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill|solution-command-integration]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `ICommand<T>` marker that `UnitOfWorkBehavior` constrains on
    - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create|ICommand.cs]] - marker interface limiting `UnitOfWorkBehavior` to writes
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-pipeline-registration.skill/solution-pipeline-registration.skill|solution-pipeline-registration]]
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-pipeline-registration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - provides centralized `PipelineRegistration` where `UnitOfWorkBehavior` is registered

NUGET:
- `MediatR` {version} - provides `IPipelineBehavior<TRequest, TResponse>`
- `Microsoft.EntityFrameworkCore` {version} - provides `DbContext.SaveChangesAsync` called by `UnitOfWork`

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - extend - Add IUnitOfWork commit contract without infrastructure coupling
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create|IUnitOfWork.cs]] - create - Single-method commit contract accessible by every layer
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend|BuildingBlocks.csproj]] - extend - Add UnitOfWorkContext and UnitOfWorkBehavior pipeline components
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create|UnitOfWorkContext.cs]] - create - Scoped thread-safe nesting depth counter preventing premature sub-command commit
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]] - create - Pipeline behavior that commits at depth 1 after handler completes
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]] - extend - Add UnitOfWork EF Core implementation
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]] - create - IUnitOfWork implementation delegating to AppDbContext
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Register IUnitOfWork and UnitOfWorkContext with scoped lifetimes
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]] - extend - Add IUnitOfWork and UnitOfWorkContext scoped registrations

# Rules

## MUST:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend#MUST|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend#MUST|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create#MUST|UnitOfWork.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend#MUST|BuildingBlocks.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create#MUST|UnitOfWorkBehavior.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create#MUST|UnitOfWorkContext.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend#MUST|Shared.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create#MUST|IUnitOfWork.cs]]

## MUST NOT:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend#MUST NOT|App.Host.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend#MUST NOT|RepositoryRegistration.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend#MUST NOT|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create#MUST NOT|UnitOfWork.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend#MUST NOT|BuildingBlocks.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create#MUST NOT|UnitOfWorkBehavior.cs]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create#MUST NOT|UnitOfWorkContext.cs]]
- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend#MUST NOT|Shared.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create#MUST NOT|IUnitOfWork.cs]]

# Anti-patterns
- `await _unitOfWork.SaveChangesAsync(ct)` in a handler — `UnitOfWorkBehavior` owns the commit
- `UnitOfWorkBehavior` without depth counter — sub-commands commit prematurely, breaking atomicity
- `UnitOfWorkContext` registered as `Singleton` — depth shared across requests, causes incorrect commit decisions
- `UnitOfWorkContext` registered as `Transient` — sub-command gets a fresh instance, depth counter never reaches 1 in nested dispatch
- `IUnitOfWork` injected into a handler — handler must trust the pipeline
- Explicit `catch { rollback }` in `UnitOfWorkBehavior` without explicit transaction — creates false safety, `DbContext.SaveChangesAsync` is already atomic

# Check list
- [ ] `IUnitOfWork` defined in `Shared/UnitOfWork/IUnitOfWork.cs` with single `SaveChangesAsync` method
- [ ] `UnitOfWorkContext` defined in `BuildingBlocks/MediatR/UnitOfWorkContext.cs`
- [ ] `UnitOfWorkBehavior` defined in `BuildingBlocks/MediatR/UnitOfWorkBehavior.cs`
- [ ] `UnitOfWorkBehavior` constrained to `where TRequest : ICommand`
- [ ] `UnitOfWorkBehavior` uses `try/finally` with `_context.Leave()` for depth decrement
- [ ] `UnitOfWorkBehavior` calls `SaveChangesAsync` only when `_context.Depth == 1`
- [ ] `UnitOfWork` implemented in `App.Infrastructure/UnitOfWork/UnitOfWork.cs`
- [ ] `UnitOfWork` registered as `Scoped` in App.Host
- [ ] `UnitOfWorkContext` registered as `Scoped` in App.Host
- [ ] No `SaveChangesAsync` call in any handler
- [ ] No `IUnitOfWork` injection in any handler
