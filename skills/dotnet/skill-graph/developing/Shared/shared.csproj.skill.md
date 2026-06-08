---
uid: ab82897c-b884-49fc-ac7b-f2635bb543cb
name: shared
description: defines the Shared project boundary, its structure, and what belongs inside it
domain: skill
type: template
version: 20260607
tags:
  - skill/template/csproj
  - dotnet
  - shared
  - common
triggers:
  - shared project structure
  - what belongs in shared
  - common interfaces
  - cross cutting primitives
aliases:
  - Shared project
  - Shared
---
# Goal
Define what the `Shared` project contains. Shared is the bottom of the dependency graph — every layer references it, it references nothing. It stores interfaces, marker types, and common abstractions that any layer needs without pulling in framework or architectural dependencies.

# Core Principles
- Shared has zero dependencies — it references no other project
- Contains only interfaces, markers, and simple abstractions — no framework dependencies
- No implementation with behavior — only contracts and simple types
- Safe for Domain, Interfaces, Application, Infrastructure, and Host to reference
- If adding something to Shared requires a NuGet reference, it does not belong here

# Structure
## Solution place
Defined in [[skills/dotnet/skill-graph/developing/Module/module-layer.skill|module-layer.skill]]
```
/src
  /Shared
    Shared.csproj
```
## Structure
```
/Shared
  /Events
    IDomainEvent.cs
  /Exceptions
    DomainException.cs
  /Mediatr
    ICommand.cs
    IQuery.cs
    IHasGuid.cs
    IHasVersions.cs
    IGuidResolver.cs
    IHasDomainEvents.cs
  /Repositories
    IRepository.cs
    IReadRepository.cs
  /UnitOfWork
    IUnitOfWork.cs
  Shared.csproj
```

## Directory and class skills

| Directory | File | Description | Skill |
|---|---|---|---|
| /Events | IDomainEvent.cs | Base interface for all domain events | idomain-event.class.skill.md |
| /Exceptions | DomainException.cs | Base domain exception type | domain-exception.class.skill.md |
| /Mediatr | ICommand.cs | Write intent marker for MediatR | icommand.class.skill.md |
| /Mediatr | IQuery.cs | Read intent marker for MediatR | iquery.class.skill.md |
| /Mediatr | IHasGuid.cs | Marker for externally created entity commands | ihas-guid.class.skill.md |
| /Mediatr | IHasVersions.cs | Marker for update commands with concurrency versions | ihas-versions.class.skill.md |
| /Mediatr | IGuidResolver.cs | Contract for Guid existence check per command | iguid-resolver.class.skill.md |
| /Mediatr | IHasDomainEvents.cs | Marker for entities that collect domain events | ihas-domain-events.class.skill.md |
| /Repositories | IRepository.cs | Write-capable repository abstraction | irepository.class.skill.md |
| /Repositories | IReadRepository.cs | Read-only repository abstraction | iread-repository.class.skill.md |
| /UnitOfWork | IUnitOfWork.cs | Commit abstraction for staged changes | iunit-of-work.class.skill.md |

## What Does NOT Belong Here
- Pipeline behavior implementations — belong in BuildingBlocks
- `OutboxMessage` — belongs in BuildingBlocks
- `UnitOfWorkContext` — belongs in BuildingBlocks
- DbContext or EF Core types — belong in App.Infrastructure
- Any class that requires a NuGet package reference

## Allowed Dependencies
```
Shared → (nothing)
```

# Rules
MUST:
- Zero project or NuGet references
- Only interfaces, markers, and simple abstract types
- Every interface or type here must be needed by at least two different layers
MUST NOT:
- Reference any other project
- Contain implementations with behavior
- Contain framework-specific types (MediatR, EF Core, FluentValidation)

# Anti-patterns
- Adding `OutboxMessage` here — it has behavior and belongs in BuildingBlocks
- Adding `ValidationBehavior` here — pipeline behavior, belongs in BuildingBlocks
- Adding a type only used by one layer — put it in that layer instead

# Checklist
- [ ] No project references in Shared.csproj
- [ ] No NuGet references in Shared.csproj
- [ ] Every type is an interface or simple abstract type
- [ ] Every type is used by at least two different layers

# Relations
- building-blocks.csproj.skill.md — implementations that depend on Shared contracts
- module-interfaces.csproj.skill.md — references Shared for ICommand, IQuery, IDomainEvent
- module-domain.csproj.skill.md — references Shared for IDomainEvent, DomainException
- module-application.csproj.skill.md — references Shared for IRepository, IUnitOfWork
