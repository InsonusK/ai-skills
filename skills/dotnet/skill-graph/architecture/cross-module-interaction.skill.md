---
uid: e28f03aa-e252-4ea0-a49c-3471a4e20cea
status: draft
name: cross-module-interaction
description: rules for interaction between bounded context modules in backend architecture
domain: skill
type: declarative
tags:
- architecture
- modular-monolith
- boundaries
triggers:
- cross module communication
- module integration design  
- querying other module data  
- calling external module logic  
- integration event design
---
# Goal
Define strict rules for interaction between modules in a modular monolith system.
# Skill Scope
__This skill:__
- how modules communicate
- how data is requested across bounded contexts
- how commands, queries, and events are used between modules
- how to avoid direct coupling between domains

This skill defines **interaction semantics**, not structure or dependency graph.
# Core Principle
Modules never depend on each other’s implementation.  
They interact only through contracts.

# Allowed Interactions
## Command-based interaction (write intent)
A module may request another module to perform an action via:
- Command in [[backend-project-structure.skill#Interfaces (.csproj)|{Module}.Interfaces]]

__Rules:__
- Command is sent via MediatR (or dispatcher)
- Only input contract is shared
- Execution logic stays inside target module Application layer
- Caller does NOT know implementation

__Example:__
```
Task module → AssignUserToTaskCommand → User module
```
## Query-based interaction (read intent)
A module may request data from another module via:
- Query defined in `{Module}.Interfaces`

__Rules:__
- Query returns DTO, never domain entities
- Query implementation can be:
    - [[backend-project-structure.skill#Application (.csproj)|{Module Name}.Application]] (internal read model)
    - [[backend-project-structure.skill#App Infrastructure (.csproj)|App Infrastructure]] (cross-module joins allowed)
- Caller does NOT access database directly

__Example:__
```
TimeLog module → GetUserActiveTasksQuery → Task module
```

## Event-based Communication
__Type of events:__
- Domain Events (internal)
- Integration Events (cross-module)

__Event Source__:
- [[backend-project-structure.skill#Application (.csproj)|{ModuleName}.Application]] - sending domain event
- [[backend-project-structure.skill#App Infrastructure (.csproj)|App.Infrastructure]] - sending app event

__Rules:__
- Events are immutable facts
- No expectation of response
- Multiple consumers allowed
- Must be idempotent-safe

__Example:__
```
TaskAssignedEvent → consumed by TimeLog module
```
# Forbidden Interactions
- Direct Domain-to-Domain references
- Direct DbContext cross-module usage
- Direct repository usage across modules
- Cross-module transactional writes

## Naming consistency
- Commands: imperative verbs (Assign, Create, Update)
- Queries: descriptive read intent (Get, Search, List)
- Events: past tense (Assigned, Created, Updated)

# Anti Goals
This skill explicitly prevents:
- hidden coupling between modules
- shared domain model across bounded contexts
- infrastructure-driven communication
- accidental API exposure through internal classes
- cross-module business logic execution
- "smart queries" leaking domain rules
# Check List
-  No direct module-to-module Application calls
-  No cross-module domain references    
-  All interactions go through Interfaces    
-  Queries are read-only across modules
-  Events are immutable and versionable    
-  Infrastructure only executes cross-module read models    
-  DTOs never expose domain internals