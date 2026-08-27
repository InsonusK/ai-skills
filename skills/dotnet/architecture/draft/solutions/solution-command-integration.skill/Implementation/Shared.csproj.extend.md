---
description: Add MediatR package and the ICommand marker interfaces to Shared
name: Shared.csproj
element_kind: project
change_kind: extend
tags:
  - solution/command-integration
  - element/shared-csproj
---

# Goals
- Make the `ICommand` marker available to every layer without coupling to BuildingBlocks
- Enable MediatR routing and pipeline behavior constraints for write operations

# Core Principles
- Shared defines only interfaces and markers — no implementations
- `ICommand<TResponse>` extends MediatR `IRequest<TResponse>` as a raw pass-through — a command writes `Result<T>` explicitly as its own type argument (`ICommand<Result<T>>`), the marker does not auto-wrap it

# Implementation changes

**AS IS** (from `plateau-stateless-non-interactive-service`, via `solution-sln-structure`) — the plateau's own structure already reserves `/MediatR/ICommand.cs` and `IQuery.cs` as placeholders in its folder map, but neither has real content yet, and no NuGet package backs them:
```
/Shared
  /Events
    IDomainEvent.cs
  /MediatR
    ICommand.cs   (placeholder — no content yet)
    IQuery.cs     (placeholder — no content yet)
  /Repositories
    IRepository.cs
    IReadRepository.cs
  /Results
  /UnitOfWork
    IUnitOfWork.cs
  /Outbox
    IHasDomainEvents.cs
  /Concurrency
    IVersioned.cs
    IHasVersions.cs
    IEntityVersionResolver.cs
  Shared.csproj
```
No NuGet packages; no project references.

**TO BE** (after this solution) — this solution is what first gives `ICommand.cs` real content:
```
/Shared
  /MediatR
    ICommand.cs   (defines ICommand : IRequest<Result>, ICommand<TResponse> : IRequest<TResponse>)
    IQuery.cs     (still a placeholder — a future solution-query-integration fills it in)
  ...(unchanged otherwise)
```
NuGet: adds `MediatR` and `Ardalis.Result`. Project references: unchanged (none).

# Structure

## Project Structure
```
/Shared
  /MediatR
    ICommand.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /MediatR | MediatR marker interfaces |
| ICommand.cs | Write operation marker interfaces |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IRequest<T>` that `ICommand<T>` extends |
| `Ardalis.Result` | latest stable | Provides `Result`/`Result<T>` returned by every command |

# Allowed Dependencies
- None — Shared has no project dependencies

# Rules

## MUST
- `MediatR` package referenced in `Shared.csproj`
- `Ardalis.Result` package referenced in `Shared.csproj`
- `ICommand` and `ICommand<TResponse>` placed in `/Shared/MediatR`
- `ICommand` extends `IRequest<Result>` and `ICommand<TResponse>` extends `IRequest<TResponse>`
- Handlers inject `IRepository<T>` from Shared — never `DbContext`

## MUST NOT
- Add FluentValidation or EF Core packages to Shared
- Add implementation code to Shared
- Validator be shared across multiple commands

# Anti-patterns
- Defining `ICommand` in BuildingBlocks — forces modules to reference BuildingBlocks for contracts
- Adding behavior logic to a marker interface

# Check list
- [ ] `MediatR` referenced in `Shared.csproj`
- [ ] `Ardalis.Result` referenced in `Shared.csproj`
- [ ] `/Shared/MediatR/ICommand.cs` exists
- [ ] `ICommand` extends `IRequest<Result>`
- [ ] `ICommand<TResponse>` extends `IRequest<TResponse>`
