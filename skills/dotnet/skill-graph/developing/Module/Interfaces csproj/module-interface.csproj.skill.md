---
uid: b26eab3c-df05-4f3e-9cc5-f4eab08efc2d
name: skill-name
description: defines the Interfaces project boundary, its structure, and what contracts it exposes to other modules
domain: skill
type: template
version: 20260607
tags:
  - skill/template/csproj
  - dotnet
  - module
  - interfaces
  - contracts
triggers:
  - create interfaces project
  - module public contract
  - add command to interfaces
  - add query to interfaces
  - add integration event
aliases:
  - "{ModuleName}.Interfaces"
---
# Goal
Define what the `{ModuleName}.Interfaces` project is and what it contains. Interfaces is the only public surface of a module — the single project other modules are allowed to reference. It contains only declarations — no implementation, no business logic. Every cross-module interaction starts here.

# Core Principles
- Interfaces is the only project other modules may reference — never Application or Domain
- Contains only declarations — no implementation, no logic
- Stable public contract boundary — changes here are breaking changes
- Commands declare write intent — queries declare read intent
- DTOs are flat projections — never expose domain entity internals
- Integration events are cross-module facts — declared here so consuming modules can subscribe

# Structure
## Solution place
Defined in [[skills/dotnet/skill-graph/developing/Module/module-layer.skill|module-layer.skill]]
```
/src/Modules/{ModuleName}
  /{ModuleName}.Interfaces
```

## Structure
```
/{ModuleName}.Interfaces
  /Commands
    Create{Entity}Command.cs
    Update{Entity}Command.cs
    Delete{Entity}Command.cs
  /Queries
    Get{Entity}Query.cs
    Get{Entities}Query.cs
  /DTOs
    {Entity}Dto.cs
    {Entity}SummaryDto.cs
  /Events
    {Entity}{Action}IntegrationEvent.cs
  {ModuleName}.Interfaces.csproj
```

## Directory and class skills

| Directory | File | Description | Skill |
|---|---|---|---|
| /Commands | {Name}Command.cs | Write intent contract | command.class.skill.md |
| /Queries | {Name}Query.cs | Read intent contract | query.class.skill.md |
| /DTOs | {Name}Dto.cs | Response shape | dto.class.skill.md |
| /Events | {Name}IntegrationEvent.cs | Cross-module event contract | integration-event.class.skill.md |

## What Does NOT Belong Here
- Command handlers — belong in `{Module}.Application`
- Query handlers — belong in `{Module}.Application` or `App.Queries`
- Domain entities or value objects — belong in `{Module}.Domain`
- Business logic of any kind — belongs in `{Module}.Domain` or `{Module}.Application`
- Validators — belong in `{Module}.Application`

## Allowed Dependencies
```
{ModuleName}.Interfaces → Shared        (IDomainEvent, Result<T>)
{ModuleName}.Interfaces → BuildingBlocks (ICommand<T>, IQuery<T>, IHasGuid, IHasVersions)
```

# Rules
MUST:
- Only declarations — no method bodies, no logic
- Commands implement `ICommand<Result<T>>`
- Queries implement `IQuery<Result<T>>`
- DTOs are `record` types — immutable, flat projections
- Integration events implement `IDomainEvent`
- Breaking changes to any contract are versioned
MUST NOT:
- Reference `{Module}.Application` or `{Module}.Domain`
- Reference other modules — not even their Interfaces
- Contain any implementation

# Anti-patterns
- Command contains validation logic — belongs in Application validator
- DTO exposes domain entity type — project to flat record
- Interfaces project references another module's Interfaces — creates coupling between modules

# Check list
- [ ] Only `Shared` and `BuildingBlocks` referenced in .csproj
- [ ] All commands implement `ICommand<Result<T>>`
- [ ] All queries implement `IQuery<Result<T>>`
- [ ] All DTOs are `record` types
- [ ] No implementation in any file
- [ ] No reference to Application or Domain

# Relations
- command.class.skill.md — command contract structure
- query.class.skill.md — query contract structure
- dto.class.skill.md — DTO structure
- integration-event.class.skill.md — integration event structure
- module-layer.csproj.skill.md — Interfaces is one of four module projects
- cross-module-communication.solution.skill.md — Interfaces is the only cross-module reference point
