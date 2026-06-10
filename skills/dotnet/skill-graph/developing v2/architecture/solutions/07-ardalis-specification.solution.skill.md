---
uid: 3f8a1c2e-7b4d-4e9f-a6c3-d5e8f2a1b7c4
order: 7
name: ardalis-specification
description: Defines the Ardalis Specification pattern — named reusable query descriptors that encapsulate filtering, ordering, and projection logic, keeping raw LINQ out of handlers
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - application
  - ardalis
  - specification
  - query
triggers:
  - write a specification
  - query entity by criteria
  - filter entities
  - project entity to DTO
  - load entity in handler
  - ardalis spec
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Specification.class.skill|Specification.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Queries csproj/classes/Specification.class.skill|QueriesSpecification.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Application csproj/{Module}.Application.csproj.skill|{Module}.Application.csproj.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/App.Queries csproj/App.Queries.csproj.skill|App.Queries.csproj.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.solution.skill|06-domain-behaviour.solution.skill]]"
---
# Goal
- Encapsulate all query criteria, ordering, includes, and projections into named reusable classes
- Keep raw LINQ out of handlers and repositories — query intent is expressed by name, not by inline lambda chains
- Define three spec shapes: entity filter (`Specification<T>`), DTO projection (`Specification<T, TResult>`), and their correct placement across Domain, Application, and App.Queries
- Establish placement rules so every spec lives in exactly one correct location — never duplicated or misplaced

# Core Principles
- Specification encodes query intent — the repository executes it, the spec never touches the database
- `Specification<T>` returns entities — used when the handler needs to work with the full domain object
- `Specification<T, TResult>` projects to DTO inside the query — used when only read data is needed
- Simple single-condition specs live in `{Module}.Domain` — reusable across features and event handlers
- Complex multi-condition or feature-specific specs live in `{Module}.Application` — belong to one use case
- Cross-module JOIN projection specs live in `App.Queries` — the only place that has access to multiple module entity types
- Spec name reflects intent, not implementation — `TaskByIdSpec` not `TaskWhereId`, `ActiveTasksByAssigneeSpec` not `TaskWhereStatusAndAssignee`
- Raw LINQ never appears in handlers — all filtering goes through a named spec

# Depend on solutions
- [[01-module-boundary.solution.skill]] — defines {Module}.Domain and {Module}.Application project boundaries where specs are placed
- [[02-solution-layer-structure.solution.skill]] — defines App.Queries project where cross-module specs live
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/06-domain-behaviour.solution.skill|06-domain-behaviour.solution.skill]] — entities that specs filter are defined by this solution

# Implementation

## {Module}.Application (.csproj) (extended)

### Project extension

#### Goal
- Store all specifications what belong to single domain - single-condition, entity-scoped, multi-condition or feature-specific specifications

#### Core Principals
- Domain specs are the most reusable — they filter one entity type by one condition
- Domain specs have no knowledge of Infrastructure, or other modules
- Every entity that is loaded by `Id` or `Guid` must have a corresponding Domain spec
- Application specs combine multiple conditions that do not belong in Domain alone
- Application specs may also be projection specs that map entities to DTOs
- Idempotency check specs for event handlers live here — scoped to the handler's use case

#### Structure

##### Project Structure
```
/{Module}.Application
  /Specifications
    {Entity}ByIdSpec.cs
    {Entity}ByGuidSpec.cs
    Active{Entities}Spec.cs
    Active{Entities}By{Owner}Spec.cs
    {Entity}SummarySpec.cs
    {Entity}ByEventIdSpec.cs
```

##### Directory and class skills
| `Directory\|file` | Description                                                                            | Pattern skill             |
| ----------------- | -------------------------------------------------------------------------------------- | ------------------------- |
| /Specifications   | Single-condition, Multi-condition or projection specs scoped to this module's features | Specification.class.skill |

#### Rules
MUST:
- All specs for this module live in `/{Module}.Application/Specifications`
- Idempotency specs for event handlers live in `/Specifications` — not inside `/Features`
-  Every entity that can be loaded by `Id` has a `{Entity}ByIdSpec`

MUST NOT:
- Application specs reference other modules' Domain types directly
- Application specs reference Infrastructure, or other module types
---

### Class extension

#### Specification (created)

##### Goal
- Encapsulate and Combine filter conditions or ordering that belongs to one specific use case and return data acording to case
- Centralise feature-specific query logic that would otherwise be duplicated across handlers
- Be reusable across multiple features and event handlers without duplication

##### Core Principals
- Inherits from `Specification<T>` (Ardalis)
- Constructor receives filter parameters and calls `Query.Where(...)`, `Query.OrderBy(...)`, or `Query.Include(...)`
- Still never touches the database — describes what to fetch
- Spec class is the query's name — reading the class name tells you what it fetches
##### Naming convention

| use case                    | class name pattern                   | class name                  | file name pattern                       | file name                      |
| --------------------------- | ------------------------------------ | --------------------------- | --------------------------------------- | ------------------------------ |
| Load by internal Id         | `{Entity}ByIdSpec`                   | `TaskByIdSpec`              | `{Entity}ByIdSpec.cs`                   | `TaskByIdSpec.cs`              |
| Load by Guid                | `{Entity}ByGuidSpec`                 | `TaskByGuidSpec`            | `{Entity}ByGuidSpec.cs`                 | `TaskByGuidSpec.cs`            |
| Filter by condition         | `{Condition}{Entities}Spec`          | `ActiveTasksSpec`           | `{Condition}{Entities}Spec.cs`          | `ActiveTasksSpec.cs`           |
| Filter by owner + condition | `{Condition}{Entities}By{Owner}Spec` | `ActiveTasksByAssigneeSpec` | `{Condition}{Entities}By{Owner}Spec.cs` | `ActiveTasksByAssigneeSpec.cs` |
| Idempotency check           | `{Entity}ByEventIdSpec`              | `TimeEntryByEventIdSpec`    | `{Entity}ByEventIdSpec.cs`              | `TimeEntryByEventIdSpec.cs`    |

##### Implementation changes
A Domain spec must inherit `Specification<T>` and apply all criteria in the constructor:

```csharp
// Task.Domain/Specifications/TaskByIdSpec.cs
public class TaskByIdSpec : Specification<TodoTask>
{
    public TaskByIdSpec(int id)
    {
        Query.Where(t => t.Id == id);
    }
}
```

```csharp
// Task.Domain/Specifications/ActiveTasksSpec.cs
public class ActiveTasksSpec : Specification<TodoTask>
{
    public ActiveTasksSpec()
    {
        Query.Where(t => t.Status == TaskStatus.Active);
    }
}
```

A multi-condition Application spec combines criteria in the constructor:

```csharp
// Task.Application/Specifications/ActiveTasksByAssigneeSpec.cs
public class ActiveTasksByAssigneeSpec : Specification<TodoTask>
{
    public ActiveTasksByAssigneeSpec(int assigneeId)
    {
        Query
            .Where(t => t.AssigneeId == assigneeId)
            .Where(t => t.Status == TaskStatus.Active)
            .OrderBy(t => t.CreatedAt);
    }
}
```

Idempotency spec for an event handler — checks if the side effect was already applied:

```csharp
// TimeLog.Application/Specifications/TimeEntryByEventIdSpec.cs
public class TimeEntryByEventIdSpec : Specification<TimeEntry>
{
    public TimeEntryByEventIdSpec(Guid eventId)
    {
        Query.Where(e => e.CreatedByEventId == eventId);
    }
}
```

##### Rule changes
MUST:
- Inherit `Specification<T>`
- Live in `/{Module}.Application/Specifications`
- Be named to reflect query intent — not implementation detail

MUST NOT:
- Call the database or reference DbContext
- Contain business logic — filtering and ordering only
- Reference other modules' Domain types

---

#### Specification (projection) (created)

##### Goal
- Project entity data directly to a DTO inside the query — avoids loading full entity when only read data is needed
- Used by query handlers that return flat read models

##### Core Principals
- Inherits from `Specification<T, TResult>` — the second type parameter is the DTO
- Uses `Query.Select(...)` to define the projection
- Repository executes the projection at the database level — no entity materialisation in memory

##### Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Project to summary DTO | `{Entity}SummarySpec` | `TaskSummarySpec` | `{Entity}SummarySpec.cs` | `TaskSummarySpec.cs` |
| Project to report DTO | `{Entity}ReportSpec` | `OrderReportSpec` | `{Entity}ReportSpec.cs` | `OrderReportSpec.cs` |

##### Implementation changes
A projection spec uses `Specification<T, TResult>` and defines `Select`:

```csharp
// Task.Application/Specifications/TaskSummarySpec.cs
public class TaskSummarySpec : Specification<TodoTask, TaskSummaryDto>
{
    public TaskSummarySpec(int assigneeId)
    {
        Query
            .Where(t => t.AssigneeId == assigneeId)
            .Select(t => new TaskSummaryDto(t.Id, t.Title, t.Status.ToString()));
    }
}
```

##### Rule changes
MUST:
- Inherit `Specification<T, TResult>`
- Define projection via `Query.Select(...)`
- DTO type declared in `{Module}.Interfaces/DTOs`

MUST NOT:
- Load full entity when only DTO fields are needed — use projection spec instead
- Define DTO inline in the spec — DTO belongs in Interfaces

---

## App.Queries (.csproj) (extended)

### Project extension

#### Goal
- Store cross-module JOIN projection specs that require access to entity types from multiple modules

#### Core Principals
- App.Queries is the only layer that has access to all module entity types simultaneously
- Cross-module specs live here — never in a single module's Domain or Application

#### Structure

##### Project Structure
```
/App.Queries
  /Specifications
    {Entity}With{RelatedEntity}Spec.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Specifications | Cross-module projection specs requiring JOIN across module boundaries | QueriesSpecification.class.skill |

#### Rules
MUST:
- All cross-module JOIN specs live in `/App.Queries/Specifications`

MUST NOT:
- Single-module specs live in App.Queries — they belong in the module's Domain or Application

---

### Class extension

#### Specification (cross-module projection) (created)

##### Goal
- Project data from multiple module entity types into a single DTO in one database query

##### Core Principals
- Inherits from `Specification<T, TResult>`
- May reference entity types from multiple modules — this is intentional and correct only here
- Used exclusively by cross-module query handlers in App.Queries

##### Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| JOIN across modules | `{Entity}With{Related}Spec` | `TaskWithUserDetailsSpec` | `{Entity}With{Related}Spec.cs` | `TaskWithUserDetailsSpec.cs` |

##### Implementation changes

```csharp
// App.Queries/Specifications/TaskWithUserDetailsSpec.cs
public class TaskWithUserDetailsSpec : Specification<TodoTask, TaskWithUserDetailsDto>
{
    public TaskWithUserDetailsSpec(int taskId)
    {
        Query
            .Where(t => t.Id == taskId)
            .Select(t => new TaskWithUserDetailsDto(
                t.Id,
                t.Title,
                t.Assignee.FullName,
                t.Assignee.Email));
    }
}
```

##### Rule changes
MUST:
- Live in `/App.Queries/Specifications`
- Only be used by handlers inside App.Queries

MUST NOT:
- Be referenced from any module's Application or Domain

---

# Rules

MUST:
- All entity loading in handlers uses a named spec — no inline `Where(...)` LINQ
- Single-condition entity-scoped specs live in `/{Module}.Domain/Specifications`
- Multi-condition or feature-specific specs live in `/{Module}.Application/Specifications`
- Cross-module JOIN specs live in `/App.Queries/Specifications`
- Every entity loaded by Id has a `{Entity}ByIdSpec` in Domain
- Projection specs use `Specification<T, TResult>` — entity filter specs use `Specification<T>`
- Spec name reflects query intent — not field names or implementation

MUST NOT:
- Spec call the database or reference DbContext
- Spec contain business logic — filtering, ordering, and projection only
- Handler contain inline `Where(...)` LINQ — always delegate to a named spec
- Domain specs reference Application or Infrastructure types
- Generic spec names used across multiple entities (`GetByIdSpec`) — name per entity

# Anti-patterns
- Inline LINQ in handler: `_repository.FirstOrDefaultAsync(x => x.Id == id)` — define `TaskByIdSpec` instead
- `GetByIdSpec` shared across entity types — each entity has its own `TaskByIdSpec`, `OrderByIdSpec`
- Business rule inside spec: `Where(t => t.Price * 0.9m > threshold)` — rule belongs in Domain, not spec
- Complex multi-condition spec placed in Domain — it belongs in Application
- Simple single-condition spec placed in Application — it belongs in Domain for reuse
- Cross-module JOIN spec placed in a module's Application — App.Queries is the only correct location

# Check list
- [ ] Every entity loaded by Id has `{Entity}ByIdSpec` in `/{Module}.Domain/Specifications`
- [ ] All single-condition specs live in `/{Module}.Domain/Specifications`
- [ ] All multi-condition and feature-specific specs live in `/{Module}.Application/Specifications`
- [ ] All cross-module JOIN specs live in `/App.Queries/Specifications`
- [ ] Idempotency specs live in `/{Module}.Application/Specifications`
- [ ] All projection specs use `Specification<T, TResult>`
- [ ] All entity filter specs use `Specification<T>`
- [ ] No inline LINQ in any handler
- [ ] Spec names reflect intent — not field names or implementation detail

# Unittest TestCases
Specs are tested via integration tests against a real database — not in isolation.
- [ ] When entity matches spec criteria Then it is returned in results
- [ ] When entity does not match spec criteria Then it is not returned
- [ ] When spec has ordering Then results are in the correct order
- [ ] When projection spec is used Then DTO fields are correctly mapped from entity columns
- [ ] When idempotency spec is used with a known EventId Then duplicate is detected correctly
- [ ] When cross-module projection spec is used Then data from both modules is correctly joined
