---
name: ardalis-specification
description: rules for defining, naming, and placing Ardalis Specifications for entity queries and projections
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - domain
  - ardalis
  - specification
triggers:
  - write a specification
  - query entity by criteria
  - filter entities
  - project entity to DTO
---
# Goal
Define how to write, name, and place Ardalis Specifications. A Specification encapsulates a named, reusable query descriptor — criteria, ordering, includes, and optional projection — keeping raw LINQ out of handlers. Without this pattern, query logic scatters into handlers as inline LINQ.

# Core Principles
- Specification encodes query intent — the repository executes it
- Simple single-condition specs live in `{Module}.Domain/Specifications` — reusable across features
- Complex multi-condition specs live in `{Module}.Application/Specifications` — feature-specific
- Specs never call the database — they describe what to fetch, not how
- `Specification<T>` for entity results, `Specification<T, TResult>` for DTO projections

# Placement Rules

| Spec type | Where | When |
|---|---|---|
| Single-condition filter | `{Module}.Domain/Specifications` | Reusable across features, scoped to one entity |
| Multi-condition or feature-specific | `{Module}.Application/Specifications` | Combines criteria or belongs to one feature |
| Cross-module JOIN projection | `App.Queries/Specifications` | Requires data from multiple modules |

# Simple Spec — Domain
Single condition. Reusable across multiple features and event handlers.
```csharp
// Task.Domain/Specifications/TaskByIdSpec.cs
public class TaskByIdSpec : Specification<Task>
{
    public TaskByIdSpec(int id)
    {
        Query.Where(t => t.Id == id);
    }
}

// Task.Domain/Specifications/TaskByGuidSpec.cs
public class TaskByGuidSpec : Specification<Task>
{
    public TaskByGuidSpec(Guid guid)
    {
        Query.Where(t => t.Guid == guid);
    }
}
```

# Complex Spec — Application
Multiple conditions or feature-specific. Lives in `/Specifications`.
```csharp
// Task.Application/Specifications/ActiveTasksByAssigneeSpec.cs
public class ActiveTasksByAssigneeSpec : Specification<Task>
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

# Idempotency Spec — Application
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

# Projection Spec — Application
`Specification<T, TResult>` projects entity to DTO inside the query.
```csharp
// Task.Application/Specifications/TaskSummarySpec.cs
public class TaskSummarySpec : Specification<Task, TaskSummaryDto>
{
    public TaskSummarySpec(int assigneeId)
    {
        Query
            .Where(t => t.AssigneeId == assigneeId)
            .Select(t => new TaskSummaryDto(t.Id, t.Title, t.Status));
    }
}
```

# Naming Rules

| Pattern | Example |
|---|---|
| By single property | `TaskByIdSpec`, `TaskByGuidSpec` |
| By condition | `ActiveTasksSpec` |
| By owner + condition | `ActiveTasksByAssigneeSpec` |
| Idempotency check | `TimeEntryByEventIdSpec` |
| Projection | `TaskSummarySpec` |

# Rules
MUST:
- All repository queries use named specifications — no inline LINQ in handlers
- Simple single-condition specs in `{Module}.Domain/Specifications`
- Complex specs in `{Module}.Application/Specifications`
- Spec name reflects query intent, not implementation detail
MUST NOT:
- Spec call the database or reference DbContext
- Spec contain business logic — filtering only
- Handler contain inline `Where(...)` LINQ

# Anti-patterns
- Inline LINQ in handler instead of spec
- Generic `GetByIdSpec` reused for every entity — name per entity: `TaskByIdSpec`
- Business rule inside spec: `Where(t => t.Price * 0.9m > discount)` — belongs in domain
- Complex multi-condition spec placed in Domain — belongs in Application

# Checklist
- [ ] All handler queries use named specifications
- [ ] Simple specs in `{Module}.Domain/Specifications`
- [ ] Complex and idempotency specs in `{Module}.Application/Specifications`
- [ ] Spec name reflects intent
- [ ] No business logic inside specs

# Unittest TestCases
Specs tested via integration tests against a real DB.
- [ ] When entity matches spec criteria Then returned in results
- [ ] When entity does not match Then not returned
- [ ] When projection spec used Then DTO fields correctly mapped
- [ ] When idempotency spec used Then duplicate event detected

# Relations
- repository.skill — IRepository and IReadRepository execute specifications
- feature-command-handler.skill — command handlers load entities via specs
- feature-query-handler.skill — query handlers project results via specs
- event-handler.skill — idempotency specs used in check-before-act guard
