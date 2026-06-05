---
uid: b1c2d3e4-f5a6-4b7c-8d9e-f0a1b2c3d4e5 
status: draft 
name: module-layer 
description: defines what a module is, its internal project structure, and inter-module interaction rules 
domain: skill 
type: architecture 
tags:
- dotnet
- architecture
- module
- bounded-context 
triggers:
- new module creation
- module structure
- bounded context design
- module interaction rules 
aliases:
- Module
- BoundedContext
---
# Goal
Define what a module is, what projects it contains, how its projects relate to each other, and how modules interact. A module is a self-contained bounded context — it owns its domain model, its application logic, its API surface, and its public contracts. Without this pattern, module boundaries blur, teams step on each other, and the codebase becomes a distributed monolith with hidden coupling.

# Core Principles
- A module is a bounded context — it owns everything inside its boundary
- Modules never depend on each other's implementation — only on Interfaces contracts
- Each module has exactly four projects: Api, Application, Domain, Interfaces
- Interfaces is the only public surface of a module — it is stable and breaking changes are versioned
- Domain is the innermost layer — it has no dependency on any other module or infrastructure

# Module Structure
```
/Modules/{ModuleName}
  /{ModuleName}.Api           ← HTTP endpoints — see [[module-api.skill]]
  /{ModuleName}.Application   ← orchestration — see [[module-application.skill]]
  /{ModuleName}.Domain        ← business logic — see [[module-domain.skill]]
  /{ModuleName}.Interfaces    ← public contracts (commands, queries, DTOs, events)
  /{ModuleName}.Api.Tests
  /{ModuleName}.Application.Tests
  /{ModuleName}.Domain.Tests
  /{ModuleName}.Integration.Tests
```

## Example
```
/Modules
  /Task
    /Task.Api
    /Task.Application
    /Task.Domain
    /Task.Interfaces
    /Task.Api.Tests
    /Task.Application.Tests
    /Task.Domain.Tests
    /Task.Integration.Tests
  /TimeLog
    /TimeLog.Api
    /TimeLog.Application
    /TimeLog.Domain
    /TimeLog.Interfaces
```

# Interfaces Project
`{ModuleName}.Interfaces.csproj` is the only project other modules may depend on.

**Contains:**
```
/{ModuleName}.Interfaces
  /Commands     ← write intent contracts
  /Queries      ← read intent contracts
  /DTOs         ← response shapes
  /Events       ← integration event contracts
```

**Rules:**
- Stable public contract boundary — changes here are breaking changes
- NO business logic
- NO implementation — declarations only
- Cross-module queries declared here, implemented in App.Queries
- Other modules depend on this project, never on Application or Domain directly

# Internal Dependency Order
```
Api → Interfaces (own module only)
Application → Interfaces (own + other modules) + Domain + Shared + BuildingBlocks
Domain → Shared + Microsoft.EntityFrameworkCore (config only)
Interfaces → (no dependencies)
```

# Inter-module Interaction Rules
Modules communicate only through contracts. See [[cross-module-communication.solution.skill]] for full rules.

|Interaction type|Mechanism|Example|
|---|---|---|
|Write to another module|MediatR command via Interfaces|`_mediator.Send(new BookItemCommand(...))`|
|Read from another module|MediatR query via Interfaces|`_mediator.Send(new GetUserQuery(...))`|
|React to another module's event|`INotificationHandler<TEvent>`|`TaskAssignedEventHandler` in TimeLog|
|Cross-module JOIN read|App.Queries|`GetTaskWithUserDetailsHandler`|

# Rules
MUST:
- Module has exactly four projects: Api, Application, Domain, Interfaces
- Other modules reference only `{ModuleName}.Interfaces` — never Application or Domain
- All cross-module writes go through MediatR command dispatch
- All cross-module reads go through MediatR query dispatch or App.Queries
- Tests colocated with module — no global tests folder 
MUST NOT:
- Module reference another module's Domain directly
- Module reference another module's Application directly
- Domain reference any other module's project

# Anti-patterns
- Shared domain model across modules — each module owns its own entities
- Direct method call into another module's Application — use MediatR
- Depending on another module's Domain for entity types — use DTOs from Interfaces
- Putting cross-module JOIN logic in Application — belongs in App.Queries

# Checklist
- [ ] Module has Api, Application, Domain, Interfaces projects
- [ ] No direct dependency on another module's Application or Domain
- [ ] All cross-module writes dispatched via MediatR
- [ ] All cross-module reads use Interfaces contracts
- [ ] Tests colocated with module
- [ ] Interfaces contains only declarations — no implementation

# Relations
- [[backend-project-structure.skill]] — solution-level structure this module fits into
- [[module-api.skill]] — Api project detail
- [[module-application.skill]] — Application project detail
- [[module-domain.skill]] — Domain project detail
- [[cross-module-communication.solution.skill]] — inter-module interaction rules in depth