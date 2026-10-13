---
name: solution-unit-of-work
description: Defines IUnitOfWork, UnitOfWorkContext, and UnitOfWorkBehavior — the pipeline mechanism that commits all staged entity changes atomically after the top-level command handler completes, ensuring sub-commands never commit prematurely
whenToUse: when a command's persisted changes must commit atomically after the top-level handler completes, including safely across nested sub-command dispatch
domain: skill
type: architecture
version: 20260901000000
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
  - "[[library/dotnet/architecture/solutions/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]"
  - "[[library/dotnet/architecture/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill|solution-infrastructure-project]]"
  - "[[library/dotnet/architecture/solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill|solution-pipeline-registration]]"
built_on_plateau:
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
- [[library/dotnet/architecture/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill|solution-infrastructure-project]]
  - [[library/dotnet/architecture/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]] - hosts `UnitOfWork` EF Core implementation
  - [[library/dotnet/architecture/solutions/solution-infrastructure-project.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - hosts `AddInfrastructure()`, extended here for `IUnitOfWork`/`UnitOfWorkContext` registration
- [[library/dotnet/architecture/solutions/solution-repository-integration.skill/solution-repository-integration.skill|solution-repository-integration]]
  - [[library/dotnet/architecture/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `IRepository<T>` that stages changes for `IUnitOfWork` to commit
    - [[library/dotnet/architecture/solutions/solution-repository-integration.skill/Implementation/Shared.csproj.extend/IRepository.cs.create|IRepository.cs]] - write-side repository used by command handlers
  - [[library/dotnet/architecture/solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create|AppDbContext.cs]] - `UnitOfWork` delegates to this `DbContext`'s `SaveChangesAsync`

NUGET:
- `MediatR` {version} - provides `IPipelineBehavior<TRequest, TResponse>`
- `Microsoft.EntityFrameworkCore` {version} - provides `DbContext.SaveChangesAsync` called by `UnitOfWork`

# Template Skill Mutations

PROJECT:
- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - extend - Add IUnitOfWork commit contract without infrastructure coupling
  - [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create|IUnitOfWork.cs]] - create - Single-method commit contract accessible by every layer
- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend|BuildingBlocks.csproj]] - extend - Add UnitOfWorkContext and UnitOfWorkBehavior pipeline components
  - [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create|UnitOfWorkContext.cs]] - create - Scoped thread-safe nesting depth counter preventing premature sub-command commit
  - [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]] - create - Pipeline behavior that commits at depth 1 after handler completes
- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]] - extend - Add UnitOfWork EF Core implementation
  - [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]] - create - IUnitOfWork implementation delegating to AppDbContext
- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Register IUnitOfWork/UnitOfWorkContext with scoped lifetimes, and UnitOfWorkBehavior last in the pipeline
  - [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend|RepositoryRegistration.cs]] - extend - Add IUnitOfWork and UnitOfWorkContext scoped registrations
  - [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend|PipelineRegistration.cs]] - extend - Insert `UnitOfWorkBehavior` last, after every other applied pipeline behavior

# Rules

## MUST
- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
	- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend#MUST|RepositoryRegistration.cs]]
	- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend#MUST|PipelineRegistration.cs]]
- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend#MUST|App.Infrastructure.csproj]]
	- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create#MUST|UnitOfWork.cs]]
- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend#MUST|BuildingBlocks.csproj]]
	- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create#MUST|UnitOfWorkBehavior.cs]]
	- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create#MUST|UnitOfWorkContext.cs]]
- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/Shared.csproj.extend#MUST|Shared.csproj]]
	- [[library/dotnet/architecture/solutions/solution-unit-of-work.skill/Implementation/Shared.csproj.extend/IUnitOfWork.cs.create#MUST|IUnitOfWork.cs]]

## SHOULD
- Avoid `await _unitOfWork.SaveChangesAsync(ct)` in a handler — `UnitOfWorkBehavior` owns the commit
- Avoid `UnitOfWorkBehavior` without depth counter — sub-commands commit prematurely, breaking atomicity
- Avoid `UnitOfWorkContext` registered as `Singleton` — depth shared across requests, causes incorrect commit decisions
- Avoid `UnitOfWorkContext` registered as `Transient` — sub-command gets a fresh instance, depth counter never reaches 1 in nested dispatch
- Avoid `IUnitOfWork` injected into a handler — handler must trust the pipeline
- Avoid explicit `catch { rollback }` in `UnitOfWorkBehavior` without explicit transaction — creates false safety, `DbContext.SaveChangesAsync` is already atomic

# Check list
- [ ] `IUnitOfWork` defined in `Shared/UnitOfWork/IUnitOfWork.cs` with single `SaveChangesAsync` method
- [ ] `UnitOfWorkContext` defined in `BuildingBlocks/MediatR/UnitOfWorkContext.cs`
- [ ] `UnitOfWorkBehavior` defined in `BuildingBlocks/MediatR/UnitOfWorkBehavior.cs`
- [ ] `UnitOfWorkBehavior` constrained to `where TRequest : ICommand<TResponse>`
- [ ] `UnitOfWorkBehavior` uses `try/finally` with `_context.Leave()` for depth decrement
- [ ] `UnitOfWorkBehavior` calls `SaveChangesAsync` only when `_context.Depth == 1`
- [ ] `UnitOfWork` implemented in `App.Infrastructure/UnitOfWork/UnitOfWork.cs`
- [ ] `UnitOfWork` registered as `Scoped` in App.Host
- [ ] `UnitOfWorkContext` registered as `Scoped` in App.Host
- [ ] No `SaveChangesAsync` call in any handler
- [ ] No `IUnitOfWork` injection in any handler
