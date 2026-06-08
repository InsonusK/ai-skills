---
uid: 4d9c836b-95e8-41af-8f26-1e90a18cf441
status: draft
name: ardalis-specification-pattern
description: rules for defining and placing Ardalis specifications for entity queries and projections
domain: skill
type: template
tags:
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
  - ardalis spec
aliases:
  - Specification
  - Spec
  - ISpecification
---
# Goal
Define how to write, name, and place Ardalis Specifications for querying entities. A Specification encapsulates a named, reusable query descriptor — criteria, ordering, includes, and optional projection — keeping raw LINQ out of handlers and repositories. Without this pattern, query logic scatters into handlers as inline LINQ, making it impossible to reuse, name, or test independently.

# Core Principles
- Specification encodes the query intent — the repository executes it
- Simple specs live in Domain — they are entity-scoped and reusable across features
- Complex specs live in Application — they span multiple conditions or are feature-specific
- Specs never call the database — they describe what to fetch, not how
- `Specification<T>` for entity results, `Specification<T, TResult>` for DTO projections
- Name reflects intent, not implementation — `ActiveTasksByAssigneeSpec` not `TaskWhereStatusAndAssignee`

# Placement Rules

|Spec type|Where|When|
|---|---|---|
|Single-condition filter|`{Module}.Domain/Specifications`|Reusable across features, scoped to one entity|
|Multi-condition or feature-specific|`{Module}.Application/Specifications`|Combines multiple criteria or belongs to one feature|
|Cross-module JOIN projection|`App.Queries/Specifications`|Requires data from multiple modules|

# Structure / Contracts

## File locations

```
/{Module}.Domain
  /Specifications
    TaskByIdSpec.cs
    TaskByGuidSpec.cs
    ActiveTasksSpec.cs

/{Module}.Application
  /Specifications
    ActiveTasksByAssigneeSpec.cs
  /Features
    /AssignTask
      AssignTaskHandler.cs

/App.Queries
  /Specifications
    TaskWithUserDetailsSpec.cs
```

## Simple spec — Domain

Single condition, reusable. Used across multiple features and event handlers.

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

## Simple spec with Guid — Domain

Used by GuidResolvingBehavior and idempotency guards.

```csharp
// Task.Domain/Specifications/TaskByGuidSpec.cs
public class TaskByGuidSpec : Specification<TodoTask>
{
    public TaskByGuidSpec(Guid guid)
    {
        Query.Where(t => t.Guid == guid);
    }
}
```

## Complex spec — Application

Multiple conditions combined. Feature-specific or spans multiple filters.

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

## Idempotency spec — Application/EventHandlers

Used in event handlers to check if an event was already processed.

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

## Projection spec — Application or App.Queries

`Specification<T, TResult>` projects entity to DTO inside the query.

```csharp
// Task.Application/Specifications/TaskSummarySpec.cs
public class TaskSummarySpec : Specification<TodoTask, TaskSummaryDto>
{
    public TaskSummarySpec(int assigneeId)
    {
        Query
            .Where(t => t.AssigneeId == assigneeId)
            .Select(t => new TaskSummaryDto(t.Id, t.Title, t.Status));
    }
}
```

## Usage in handler

```csharp
// single entity
var task = await _repository.FirstOrDefaultAsync(new TaskByIdSpec(command.TaskId), ct);

// collection
var tasks = await _repository.ListAsync(new ActiveTasksByAssigneeSpec(command.AssigneeId), ct);

// existence check (idempotency)
var exists = await _repository.AnyAsync(new TimeEntryByEventIdSpec(notification.EventId), ct);

// projection
var dtos = await _repository.ListAsync(new TaskSummarySpec(query.AssigneeId), ct);
```

# Naming Rules

|Pattern|Example|
|---|---|
|By single property|`TaskByIdSpec`, `TaskByGuidSpec`|
|By condition|`ActiveTasksSpec`, `CompletedOrdersSpec`|
|By owner + condition|`ActiveTasksByAssigneeSpec`|
|Idempotency check|`TimeEntryByEventIdSpec`, `PaymentByEventIdSpec`|
|Projection|`TaskSummarySpec`, `OrderReportSpec`|

# Rules

MUST:
- All repository queries use specifications — no inline LINQ in handlers
- Simple single-condition specs live in `{Module}.Domain/Specifications`
- Complex multi-condition specs live in `{Module}.Application/Specifications`
- Cross-module JOIN specs live in `App.Queries/Specifications`
- Spec name reflects query intent, not implementation detail
- Idempotency specs defined alongside their event handler 
MUST NOT:
- Spec call the database or reference DbContext
- Spec contain business logic — filtering only
- Handler contain inline `Where(...)` LINQ outside a spec
- Domain specs reference Application or Infrastructure types

# Anti-patterns
- Inline LINQ in handler: `_dbContext.Tasks.Where(t => t.Id == id).FirstOrDefault()` — use spec
- Generic `GetByIdSpec` reused for every entity — name per entity: `TaskByIdSpec`
- Business rule inside spec: `Where(t => t.Price * 0.9m > discount)` — rule belongs in domain
- Spec placed in wrong layer: complex multi-condition spec in Domain, simple filter in Application

# Checklist
- [ ] All handler queries use named specifications
- [ ] Simple specs in `{Module}.Domain/Specifications`
- [ ] Complex specs in `{Module}.Application/Specifications`
- [ ] Cross-module specs in `App.Queries/Specifications`
- [ ] Idempotency spec defined for each event handler that uses check-before-act
- [ ] Spec name reflects intent
- [ ] No business logic inside specs

# Unittest TestCases
Specs are tested via integration tests against a real DB — not in isolation.
- [ ] When entity matches spec criteria Then returned in results
- [ ] When entity does not match spec criteria Then not returned
- [ ] When spec has ordering Then results are in correct order
- [ ] When projection spec used Then DTO fields are correctly mapped
- [ ] When idempotency spec used Then duplicate event detected correctly

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/repository-pattern.skill]] — IRepository and IReadRepository execute specifications
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/domain-event-handler-pattern.skill]] — idempotency specs used in event handlers
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/Solutions/command-handling.solution.skill]] — command handlers load entities via specs
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/query-handler-pattern.skill]] — query handlers return results via projection specs
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill]] — specs filter entities defined in Domain