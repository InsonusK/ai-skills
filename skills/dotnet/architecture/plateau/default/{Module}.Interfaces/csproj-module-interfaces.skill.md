---
name: csproj-module-interfaces
description: Provide the single stable public surface through which other modules interact with this module
domain: skill
type: template
version: 20260627
plateau: default
tags:
  - skill/template/csproj
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]"
---

# Goal
- Provide the single stable public surface through which other modules interact with this module
- Declare all write intent contracts (commands), read intent contracts (queries), response shapes (DTOs), and integration event contracts
- Own all Query record declarations and DTO response shapes for this module
- Be the contract surface other modules and the API layer use to request data from this module
- Extend create commands for externally-created entity types with `IHasGuid`
- Extend all update and patch commands with `IHasVersions` to carry client-supplied version information
- Own all Command record declarations and their associated result records for this module
- Be the only project other modules depend on when dispatching commands to this module
- Expose soft value object shapes from `{Module}.Interfaces/ValueObjects` so other modules can use them in commands and DTOs

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Core Principals
- Interfaces is a declarations-only project — no business logic, no implementation
- Changes to Interfaces are breaking changes and must be versioned
- Other modules depend on this project only — never on Application or Domain
- Queries are declarations only — records with input properties, no methods, no logic
- DTOs are read-only response shapes — records, no domain entity references
- Both declared in Interfaces so other modules can dispatch queries without depending on Application or Domain
- Cross-module queries are also declared here — implemented in App.Queries, declared in the owning module's Interfaces
- `Versions` property typed as `IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>>`
- Populated by the API controller from the decoded `If-Match` header — never hardcoded
- Create and delete commands do NOT implement `IHasVersions` — only update and patch
- Commands are declarations only — records with properties, no methods, no logic
- Result records are declared alongside their command in the same file
- Both Command and Result are `record` types — immutable by design
- `Soft{ValueObject}` declarations live in `/{Module}.Interfaces/ValueObjects` and allow invalid values
- `Soft{ValueObject}` has no validation logic — validators belong in `{Module}.Application`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Interfaces
```


## Project Structure
- /{Module}.Interfaces
  - /Commands
    - [{Command}.cs](./classes/class-command.skill.md)
  - /Queries
    - [Get{Entity}Query.cs](./classes/class-query.skill.md)
    - [Get{Entities}Query.cs](./classes/class-query.skill.md)
    - [Get{Entity}With{Related}Query.cs](./classes/class-query.skill.md)
  - /DTOs
    - [{Entity}Dto.cs](./classes/class-dto.skill.md)
    - [{Entity}SummaryDto.cs](./classes/class-dto.skill.md)
    - [{Entity}With{Related}Dto.cs](./classes/class-dto.skill.md)
  - /ValueObjects
    - [Soft{ValueObject}.cs](./classes/class-soft-value-object.skill.md)
  - /Events
    - {IntegrationEvent}.cs
  - {Module}.Interfaces.csproj

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

## Directory and class skills
| `Directory                         | file`                                                          | Description                                                                                                         | Pattern skill |
| ---------------------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- | ------------- |
| /Commands                          | Write intent contracts                                         |                                                                                                                     |               |
| /Queries                           | Read intent contracts                                          |                                                                                                                     |               |
| /DTOs                              | Response shapes                                                |                                                                                                                     |               |
| /ValueObjects                      | Soft value object declarations                                 | [[skills/dotnet/architecture/plateau/default/{Module}.Interfaces/classes/class-soft-value-object.skill.md|class-SoftValueObject.skill]] |               |
| /Events                            | Integration event contracts                                    |                                                                                                                     |               |
| /Queries                           | Read intent contract declarations for this module              |                                                                                                                     |               |
| /DTOs                              | Response shape declarations consumed by query handlers and API |                                                                                                                     |               |
| /Commands/Create{Entity}Command.cs | Create command with Guid and IHasGuid                          |                                                                                                                     |               |
| /Commands                          | Write intent contract declarations for this module             |                                                                                                                     |               |
| {Command}.cs                       | Command record and its result record                           | [[skills/dotnet/architecture/plateau/default/{Module}.Interfaces/classes/class-command.skill\|class-Command.skill]] |               |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Queries implement `IQuery<T>` which extends `IRequest<T>` |
| `Ardalis.Result` | latest stable | Query return types use `Result<T>` |
| `MediatR` | latest stable | Required for `ICommand<T>` marker usage |
| `Ardalis.Result` | latest stable | Required for `Result<T>` return type usage |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

## What Does NOT Belong Here
- Business logic — belongs to Domain
- Implementation — belongs to Application
- Infrastructure concerns — belongs to App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

## Allowed Dependencies
- Shared
- Shared — for `IQuery<T>` marker

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Rules
MUST:
	- Interfaces contains only declarations — records, interfaces, DTOs, soft value objects
	- All soft value object declarations live in `/{Module}.Interfaces/ValueObjects`
	- Every `Soft{ValueObject}` has a matching Domain Value Object that inherits from it
	- All commands declared here
	- All queries declared here
	- All integration events declared here
	- All queries for this module declared in `/{Module}.Interfaces/Queries`
	- All DTOs for this module declared in `/{Module}.Interfaces/DTOs`
	- Cross-module query contracts declared in the owning module's Interfaces — implemented in App.Queries
	- Queries implement `IQuery<Result<T>>`
	- DTOs declared as `record`
	- `Guid` is the first property on the command record
	- Command implements both `ICommand<Result<T>>` and `IHasGuid`
	- `Guid` typed as `System.Guid` — never `string` or `int`
	- All update and patch commands implement `IHasVersions`
	- All commands for this module declared in `/{Module}.Interfaces/Commands`
	- Each command file contains the command record and its result record
	- Commands implement `ICommand<Result<T>>` from Shared
MUST NOT:
	- Interfaces reference Domain, Application, or any infrastructure project
	- Interfaces contain any implementation code
	- Queries contain any logic or methods
	- DTOs expose domain entity types — projection shapes only
	- DTOs have public setters — declared as `record` for immutability
	- Reference BuildingBlocks — commands implement interfaces from Shared only
	- `Soft{ValueObject}` throw exceptions or validate values
	- Update, delete, or internal-create commands implement `IHasGuid`
	- Create commands implement `IHasVersions` — new entities have no version
	- Delete commands implement `IHasVersions` — deletion does not require version check in this architecture
	- Commands contain any logic or methods
	- Commands reference Domain entity types — input properties are primitives or shared value types only
	- Interfaces project reference Domain, Application, or infrastructure projects

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Anti-patterns
- Placing command handlers in Interfaces — handlers belong in Application
- Placing domain entities in Interfaces — use DTOs for cross-module data shapes
- Referencing another module's Domain from Interfaces
- Query declared in Application — Interfaces is the public contract surface
- DTO with domain entity property — breaks layer isolation
- `Guid` not as first property — signals external-created entity at a glance
- `{Module}.Interfaces` referencing BuildingBlocks just to implement `IHasGuid`
- `Versions` constructed in application code instead of passed from controller
- Declaring command handlers or validators in Interfaces

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Check list
- [ ] /Commands folder exists
- [ ] /Queries folder exists
- [ ] /DTOs folder exists
- [ ] /Events folder exists
- [ ] Interfaces.csproj references only Shared
- [ ] No implementation code in any file
- [ ] `/{Module}.Interfaces/Queries` folder exists
- [ ] `/{Module}.Interfaces/DTOs` folder exists
- [ ] All query records implement `IQuery<Result<T>>`
- [ ] All DTOs are `record` types
- [ ] No domain entity types referenced in DTOs
- [ ] Create command implements `IHasGuid`
- [ ] `Guid` is first property
- [ ] All update commands implement `IHasVersions`
- [ ] No create or delete command implements `IHasVersions`
- [ ] `/Commands` folder exists
- [ ] Each command file contains command and result records
- [ ] `MediatR` package referenced
- [ ] `Ardalis.Result` package referenced
- [ ] Interfaces references only Shared

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create|{Module}.Interfaces.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
