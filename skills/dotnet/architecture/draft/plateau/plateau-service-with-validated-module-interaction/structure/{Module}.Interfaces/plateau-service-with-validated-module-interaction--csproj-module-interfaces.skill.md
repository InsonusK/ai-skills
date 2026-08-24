---
name: csproj-module-interfaces
description: Project {Module}.Interfaces in the service-with-validated-module-interaction plateau
whenToUse: when adding or editing a command, query, DTO, or event contract in {Module}.Interfaces, or deciding whether new code belongs here
domain: skill
type: template
plateau: service-with-validated-module-interaction
version: 20260822140000
tags:
  - skill/template/csproj
  - plateau/service-with-validated-module-interaction
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
- Provide the single stable public surface through which other modules interact with this module
- Declare all write intent contracts (commands), read intent contracts (queries), response shapes (DTOs), and integration event contracts
- Declare permissive, validation-agnostic Value Object shapes DTOs and other modules can reference

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Core Principles
- Interfaces is a declarations-only project — no business logic, no implementation
- Commands are declarations only — records with properties, no methods, no logic; result records co-located in the same file

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Interfaces
```

## Project Structure
- /{Module}.Interfaces
  - /Commands
    - [{Command}.cs](./classes/plateau-service-with-validated-module-interaction--class-command.skill.md)
  - /Queries
  - /DTOs
  - /Events
  - /ValueObjects
    - [Soft{ValueObject}.cs](./classes/plateau-service-with-validated-module-interaction--class-soft-value-object.skill.md)
  - {Module}.Interfaces.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Commands | Write intent contracts | [[./classes/plateau-service-with-validated-module-interaction--class-command.skill.md\|class-command]] |
| /Queries | Read intent contracts | |
| /DTOs | Response shapes | |
| /Events | Integration event contracts | |
| /ValueObjects | Permissive, validation-agnostic value-object records | [[./classes/plateau-service-with-validated-module-interaction--class-soft-value-object.skill.md\|class-soft-value-object]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Required for `ICommand<T>` marker usage |
| `Ardalis.Result` | latest stable | Required for `Result<T>` return type usage |

## Allowed Dependencies
- Shared

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Rules
MUST:
- Interfaces contains only declarations — records, interfaces, DTOs
- All commands, queries, and integration events declared here
- All commands implement `ICommand<T>` (or `ICommand` when no payload is returned) from Shared, declared in `/Commands`
- `Soft{ValueObject}` types declared in `/ValueObjects`, validation-agnostic
MUST NOT:
- Interfaces reference Domain, Application, or any infrastructure project
- Interfaces contain any implementation code

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]

# Check list
- [ ] /Commands, /Queries, /DTOs, /Events, /ValueObjects folders exist
- [ ] Interfaces.csproj references only Shared
- [ ] No implementation code in any file
- [ ] Every command implements `ICommand<T>` (or `ICommand` when no payload is returned)

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj.create]]
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj.extend]]
