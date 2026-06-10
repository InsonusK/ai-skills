---
uid: 481d9975-77c6-433c-a48f-454622a64ef3
order: 1
name: module-boundary
description: Defines what a module is, its four-project internal structure, folder placement in solution, and inter-module interaction rules
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - architecture
  - module
  - bounded-context
triggers:
  - create new module
  - design module boundary
  - define bounded context structure
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill|{Module}.Domain.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Interface csproj/{Module}.Interfaces.csproj.skill|{Module}.Interfaces.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Api csproj/{Module}.Api.csproj.skill|{Module}.Api.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill|Entity]]"
extends:
depends_on:
---

# Goal
- Define a module as a self-contained bounded context that owns its domain, application logic, API surface, and public contracts
- Prevent hidden coupling between modules by enforcing interaction only through declared contracts
- Define the four-project internal structure every module must follow
- Define where modules live in the solution folder structure

# Core Principles
- A module is a bounded context — it owns everything inside its boundary
- Modules never depend on each other's implementation — only on Interfaces contracts
- Each module has exactly four projects: Api, Application, Domain, Interfaces
- Interfaces is the only public surface of a module — breaking changes must be versioned
- Domain is the innermost layer — it has no dependency on any other module or infrastructure
- Tests are colocated with the module — no global tests folder

# Depend on solutions
None — this is a foundation solution.

# Implementation

## App Repository (.sln)

### Project extension

### Structure

#### Project Structure
```
/src
  /Modules
    /{ModuleName}
      /{ModuleName}.Api
      /{ModuleName}.Application
      /{ModuleName}.Domain
      /{ModuleName}.Interfaces
      /{ModuleName}.Api.Tests
      /{ModuleName}.Application.Tests
      /{ModuleName}.Domain.Tests
      /{ModuleName}.Integration.Tests
```

#### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Modules | Root folder for all bounded context modules | |
| /{ModuleName} | One folder per module | |
| /{ModuleName}.Api | HTTP endpoints, MediatR dispatch | |
| /{ModuleName}.Application | Orchestration — handlers, validators, specs | |
| /{ModuleName}.Domain | Business logic — entities, VOs, rules, events | |
| /{ModuleName}.Interfaces | Public contracts — commands, queries, DTOs, events | |

#### Rules
MUST:
- Every module lives under `/src/Modules/{ModuleName}`
- Every module has exactly four projects: Api, Application, Domain, Interfaces
- Tests live alongside module projects — not in a global `/tests` folder

MUST NOT:
- Module projects exist outside `/src/Modules`
- Module have fewer or more than four projects without explicit architectural justification

---

## {Module}.Interfaces (.csproj) 

### Project extension

#### Goal
- Provide the single stable public surface through which other modules interact with this module
- Declare all write intent contracts (commands), read intent contracts (queries), response shapes (DTOs), and integration event contracts

#### Core Principal
- Interfaces is a declarations-only project — no business logic, no implementation
- Changes to Interfaces are breaking changes and must be versioned
- Other modules depend on this project only — never on Application or Domain

#### Structure

##### Project Structure
```
/{ModuleName}.Interfaces
  /Commands
  /Queries
  /DTOs
  /Events
  {ModuleName}.Interfaces.csproj
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Commands | Write intent contracts | |
| /Queries | Read intent contracts | |
| /DTOs | Response shapes | |
| /Events | Integration event contracts | |

#### What Does NOT Belong Here
- Business logic — belongs to Domain
- Implementation — belongs to Application
- Infrastructure concerns — belongs to App.Infrastructure

#### Allowed Dependencies
- None — Interfaces has no project dependencies

#### Rules
MUST:
- Interfaces contains only declarations — records, interfaces, DTOs
- All commands declared here
- All queries declared here
- All integration events declared here

MUST NOT:
- Interfaces reference Domain, Application, or any infrastructure project
- Interfaces contain any implementation code

#### Anti-patterns
- Placing command handlers in Interfaces — handlers belong in Application
- Placing domain entities in Interfaces — use DTOs for cross-module data shapes
- Referencing another module's Domain from Interfaces

#### Check list
- [ ] /Commands folder exists
- [ ] /Queries folder exists
- [ ] /DTOs folder exists
- [ ] /Events folder exists
- [ ] No project references in Interfaces.csproj
- [ ] No implementation code in any file

---

## {Module}.Domain (.csproj) 

### Project extension

#### Goal
- Own the entities, value objects, rules, and domain events for this bounded context
- Store all entity types for this bounded context
- Own the business logic and invariant enforcement for all entities in this module
#### Core Principal
- Domain is the innermost layer — pure business logic, no infrastructure dependencies
- Domain has no knowledge of other modules
- All entities live in /{Module}.Domain/Entities
- Domain is the only layer that contains entity definitions
#### Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)
#### Structure
##### Project Structure
```
/{Module}.Domain
  /Entities
    InternalImmutableEntity.cs
    InternalMutableEntity.cs
    ExternalImmutableEntity.cs
    ExternalMutableEntity.cs
```

##### Directory and class skills

|Directory \| file|Description|Pattern skill|
|---|---|---|
|/Entities|All entity types for this module|[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill]]|

#### Rules
MUST:
- Domain depends only on Shared and EF Core (for IEntityTypeConfiguration only)
- All entities live in /{Module}.Domain/Entities

MUST NOT:
- Domain reference any other module's project
- Domain use EF Core beyond IEntityTypeConfiguration

#### Anti-patterns
- Injecting DbContext into a domain class — domain has no persistence dependency
- Referencing another module's Domain for shared entity types — each module owns its own entities
- Using EF Core attributes on domain entities — use configuration classes instead
- Placing entities outside /Entities folder — breaks navigation and discoverability
- Defining entities in Application or Interfaces — entities belong in Domain only

#### Check list
- [ ] Domain.csproj references only EF Core
- [ ] No DbContext reference in any domain class
- [ ] No cross-module domain references
- [ ] /Entities folder exists in {Module}.Domain
- [ ] All entity classes placed in /Entities

---


### Class extension

#### [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill|Entity.class.skill]]

##### Goals
- Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
- Select the correct entity type from the type matrix before implementation
- Define a domain entity as an object with stable identity where identity — not value — determines equality
- Ensure every entity is assigned to exactly one type so the correct set of patterns is applied
- Prevent invalid state by enforcing that all entity properties are accessible only through controlled access modifiers

##### Core Principals

- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
- `Id` is always `internal set` — only persistence layer assigns it, never application code
- Entity type is selected from the type matrix before implementation begins — not discovered during coding
- All public setters or method must validate to prevent invalid state

##### Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity   | {EntityName}       | Order      | {EntityName}.cs   | Order.cs  |

##### Implementation changes

Entity must be a class with `int Id` as primary identity. 

```csharp
public class Currency
{
    public int Id { get; internal set; }
    private string _code;
    public string Code {
	    public get => this._code;
      public set {
		    if (value == "")
		        throw new DomainException("Invalid code");
		    this._code = value;
		  }  
		}
    
		public int Amount {get; internal set;}   
    internal void SetAmount(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Invalid amount");

        this.Amount = amount;
    }

}
```

##### Rule changes

MUST:
- Entity has `int Id` with `internal set`
- All public property setters or methods must validation state
- `Id` used in all domain logic, persistence, relationships, and internal APIs

MUST NOT:
- Use `public` setters on any entity property

##### Anti-patterns (extended)

- `public string Title { get; set; }` — public setter without validation
- Placing entity in Application or Interfaces project — entities belong in Domain only

##### Check list (extended)
- [ ]  Entity type selected from the matrix
- [ ]  `int Id` with `internal set` present
- [ ]  All public property setters and methods has validation
- [ ]  Entity placed in /{Module}.Domain/Entities

##### Unittest TestCases (extended)

- [ ]  When entity created Then Id is default (0) until persisted

## {Module}.Application (.csproj) 

### Project extension

#### Goal
- Orchestrate use cases by connecting the API contract to the domain model

#### Core Principal
- Application coordinates — it never contains business logic
- Application knows its own Domain and its own Interfaces
- Application may reference other modules' Interfaces for cross-module dispatch

#### Allowed Dependencies
- {Module}.Interfaces (own module)
- {Module}.Domain (own module)
- {OtherModule}.Interfaces (other modules — contracts only)
- Shared
- BuildingBlocks

#### Rules
MUST:
- Application references only own Interfaces, own Domain
MUST NOT:
- Application reference another module's Domain
- Application reference another module's Application
- Application contain business logic — delegate to Domain

#### Anti-patterns
- Calling another module's Application method directly — use MediatR dispatch through Interfaces
- Writing business rules in a handler — delegate to entity or domain service

#### Check list
- [ ]  Application.csproj does not reference another module's Domain or Application
- [ ]  No business logic in any handler class

---

## {Module}.Api (.csproj) 

### Project extension

#### Goal
- Expose HTTP endpoints as thin MediatR adapters for this module

#### Core Principal
- Api is a thin adapter — no business logic, no domain rules
- Api references only its own Interfaces project for contracts

#### Allowed Dependencies
- {Module}.Interfaces (own module only)
- BuildingBlocks (for shared API utilities)

#### Rules
MUST:
- Every endpoint dispatches exactly one MediatR command or query
- Api references only own Interfaces and BuildingBlocks
MUST NOT:
- Api reference Domain directly
- Api reference Application directly
- Api contain business logic, validation logic, or domain rules

#### Anti-patterns
- Injecting a repository or DbContext into a controller — use MediatR dispatch only
- Writing business logic in a controller action — belongs in Domain
- Referencing Application project from Api — Api knows only Interfaces contracts

#### Check list
- [ ]  Api.csproj does not reference Domain
- [ ]  Api.csproj does not reference Application
- [ ]  Every controller action dispatches exactly one MediatR request
- [ ]  No business logic in any controller

---

# Rules

MUST:
- Each module has exactly Api, Application, Domain, Interfaces projects
- Other modules reference only {ModuleName}.Interfaces
- All cross-module writes go through MediatR command dispatch
- All cross-module reads go through MediatR query dispatch or App.Queries
- Tests colocated with module

MUST NOT:
- Module reference another module's Domain
- Module reference another module's Application
- Domain reference any other module's project
- Api reference Domain or Application directly

# Anti-patterns
- Shared domain model across modules — each module owns its own entities
- Direct method call into another module's Application — use MediatR
- Depending on another module's Domain for entity types — use DTOs from Interfaces
- Cross-module JOIN logic in Application — belongs in App.Queries
- Global tests folder — tests live next to their module

# Check list
- [ ] Module folder exists under /src/Modules/{ModuleName}
- [ ] Module has exactly four projects: Api, Application, Domain, Interfaces
- [ ] Interfaces has no project dependencies
- [ ] Domain depends only on Shared and EF Core config
- [ ] Application does not reference Infrastructure or App.Queries
- [ ] Api does not reference Domain or Application directly
- [ ] No direct dependency on another module's Application or Domain
- [ ] Tests colocated with module projects

# Unittest TestCases
Not applicable — module boundary is validated via architecture tests, not runtime unit tests.

- [ ] When any project references another module's Domain Then architecture test fails
- [ ] When any project references another module's Application Then architecture test fails
- [ ] When Interfaces project has a project reference Then architecture test fails
- [ ] When Api references Domain directly Then architecture test fails
