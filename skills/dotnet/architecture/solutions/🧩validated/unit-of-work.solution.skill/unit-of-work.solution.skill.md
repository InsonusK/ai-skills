---
uid: 4b7e2c9a-1f5d-4a8b-c3e6-d9f1a4b2e7c6
name: unit-of-work
description: Defines IUnitOfWork, UnitOfWorkContext, and UnitOfWorkBehavior — the pipeline mechanism that commits all staged entity changes atomically after the top-level command handler completes, ensuring sub-commands never commit prematurely
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  - dotnet
  - application
  - infrastructure
  - unit-of-work
  - mediatr
  - pipeline
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]"
---

# Goal
- Define `IUnitOfWork` as the single commit point for all staged entity changes — the only place `SaveChangesAsync` is called
- Define `UnitOfWorkBehavior` as the pipeline mechanism that automatically commits after the top-level command handler completes
- Define `UnitOfWorkContext` as the nesting depth tracker that prevents sub-commands from committing prematurely
- Ensure all changes from a command and all its sub-commands are committed atomically in a single `SaveChangesAsync` call
- Enforce that no handler ever calls `SaveChangesAsync` directly

# Core Principles
- `IUnitOfWork` lives in Shared — every layer can reference the commit contract without coupling to infrastructure
- `IUnitOfWork` is the only component that calls `SaveChangesAsync` — handlers, repositories, and domain services never call it
- `UnitOfWorkBehavior` and `UnitOfWorkContext` live in BuildingBlocks — they reference `ICommand` and `IUnitOfWork` from Shared
- `UnitOfWorkBehavior` activates only on `ICommand` — queries never trigger a commit
- `UnitOfWorkContext` tracks nesting depth — only the outermost command (`Depth == 1`) commits
- Sub-commands dispatched via `_mediator.Send()` increment depth — they stage changes but defer commit to the root
- **No explicit rollback needed** — EF Core uses implicit transactions: if `SaveChangesAsync` is never called, the DbContext is disposed at request scope end and all staged changes are silently abandoned. Explicit rollback is only required when using `DbContext.Database.BeginTransactionAsync()`, which this architecture does not use.
- If the handler throws, `SaveChangesAsync` is never called — all staged changes are discarded with the request scope
- `UnitOfWorkContext` is registered as `Scoped` — one instance per HTTP request, shared across all nested command dispatches within that request

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts `IUnitOfWork` commit contract
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/BuildingBlocks.csproj.create.md|BuildingBlocks.csproj]] - hosts `UnitOfWorkContext` and `UnitOfWorkBehavior`
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj]] - hosts `UnitOfWork` EF Core implementation
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-structure.solution.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts `IUnitOfWork` and `UnitOfWorkContext` registrations
- [[skills/dotnet/architecture/solutions/🧩validated/repository-integration.solution.skill/repository-integration.solution.skill.md|repository-integration.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `IRepository<T>` that stages changes for `IUnitOfWork` to commit
    - [[skills/dotnet/architecture/solutions/🧩validated/repository-integration.solution.skill/Implementation/Shared.csproj.extend/IRepository.cs.create.md|IRepository.cs]] - write-side repository used by command handlers
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - provides `ICommand<T>` marker that `UnitOfWorkBehavior` constrains on
    - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - marker interface limiting `UnitOfWorkBehavior` to writes
- [[skills/dotnet/architecture/solutions/🧩validated/pipeline-registration.solution.skill/pipeline-registration.solution.skill.md|pipeline-registration.solution.skill]]
  - [[skills/dotnet/architecture/solutions/🧩validated/pipeline-registration.solution.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - provides centralized `PipelineRegistration` where `UnitOfWorkBehavior` is registered


NUGET:
- `MediatR` {version} - provides `IPipelineBehavior<TRequest, TResponse>`
- `Microsoft.EntityFrameworkCore` {version} - provides `DbContext.SaveChangesAsync` called by `UnitOfWork`

# Template Skill Mutations

PROJECT:
- [[./Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - Add IUnitOfWork commit contract without infrastructure coupling
  - [[./Implementation/Shared.csproj.extend/IUnitOfWork.cs.create.md|IUnitOfWork.cs]] - create - Single-method commit contract accessible by every layer
- [[./Implementation/BuildingBlocks.csproj.extend.md|BuildingBlocks.csproj]] - extend - Add UnitOfWorkContext and UnitOfWorkBehavior pipeline components
  - [[./Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs]] - create - Scoped nesting depth counter preventing premature sub-command commit
  - [[./Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create.md|UnitOfWorkBehavior.cs]] - create - Pipeline behavior that commits at depth 1 after handler completes
- [[./Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Add UnitOfWork EF Core implementation
  - [[./Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create.md|UnitOfWork.cs]] - create - IUnitOfWork implementation delegating to AppDbContext
- [[./Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - Register IUnitOfWork and UnitOfWorkContext with scoped lifetimes
  - [[./Implementation/App.Host.csproj.extend/RepositoryRegistration.cs.extend.md|RepositoryRegistration.cs]] - extend - Add IUnitOfWork and UnitOfWorkContext scoped registrations

# Rules

MUST:
- `IUnitOfWork` defined in Shared — single `SaveChangesAsync` method only
- `UnitOfWorkContext` defined in BuildingBlocks — registered as `Scoped`
- `UnitOfWorkBehavior` defined in BuildingBlocks — constrained to `ICommand` only
- `UnitOfWork` implementation in App.Infrastructure
- `UnitOfWorkBehavior` uses `try/finally` — depth always restored on exception
- `UnitOfWorkBehavior` commits only when `Depth == 1`
- `IUnitOfWork` and `UnitOfWorkContext` registered as `Scoped`
- Sub-commands safe to dispatch from handlers — depth counter prevents premature commit
- Pipeline behaviors registered via centralized `PipelineRegistration` in App.Host

MUST NOT:
- Any handler call `SaveChangesAsync` or inject `IUnitOfWork`
- `UnitOfWorkBehavior` activate on queries — constrained to `ICommand`
- `UnitOfWorkContext` registered as `Singleton` or `Transient`
- `UnitOfWork` contain logic beyond `DbContext.SaveChangesAsync` delegation
- `UnitOfWorkBehavior` contain a catch/rollback block — EF implicit transactions do not require it
- `IUnitOfWork` defined in BuildingBlocks — belongs in Shared

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
- [ ] `UnitOfWorkBehavior` uses `try/finally` for depth decrement
- [ ] `UnitOfWorkBehavior` calls `SaveChangesAsync` only when `Depth == 1`
- [ ] `UnitOfWork` implemented in `App.Infrastructure/UnitOfWork/UnitOfWork.cs`
- [ ] `UnitOfWork` registered as `Scoped` in App.Host
- [ ] `UnitOfWorkContext` registered as `Scoped` in App.Host
- [ ] No `SaveChangesAsync` call in any handler
- [ ] No `IUnitOfWork` injection in any handler

# Unittest TestCases
- [ ] When top-level command completes successfully Then `SaveChangesAsync` called exactly once
- [ ] When command dispatches sub-command Then `SaveChangesAsync` called once after root completes — not at sub-command level
- [ ] When sub-command completes Then `UnitOfWorkContext.Depth` returns to 1 — root still owns commit
- [ ] When handler throws Then `SaveChangesAsync` never called — staged changes discarded
- [ ] When query handler runs Then `UnitOfWorkBehavior` does not activate — no `SaveChangesAsync`
- [ ] When multiple repositories used in one handler Then all changes committed in single `SaveChangesAsync`
- [ ] When `UnitOfWorkContext` registered as Scoped Then nested dispatch shares same depth counter
