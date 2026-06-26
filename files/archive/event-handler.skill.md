---
name: event-handler
description: rules for implementing domain event handlers and ensuring idempotency
domain: skill
type: pattern
tags:
  - dotnet
  - application
  - events
  - mediatr
  - idempotency
triggers:
  - implement event handler
  - domain event handler
  - INotificationHandler
  - idempotent handler
---
# Goal
Define how to implement a MediatR notification handler that reacts to a domain event. Because the outbox may re-deliver events on failure, every handler must be idempotent — processing the same event twice must produce the same result as processing it once.

# Core Principles
- Handler reacts to a fact — it never assumes the event arrives exactly once
- Idempotency is the handler's responsibility — the outbox guarantees delivery, not exactly-once
- Handler never calls back into the publishing module's domain directly
- Cross-module handler depends only on `{SourceModule}.Interfaces` — never on source Domain or Application

# File Location
```
/{ModuleName}.Application
  /EventHandlers
    TaskAssignedEventHandler.cs   ← not inside /Features
```

# Handler
```csharp
// TimeLog.Application/EventHandlers/TaskAssignedEventHandler.cs
public class TaskAssignedEventHandler : INotificationHandler<TaskAssignedEvent>
{
    private readonly IRepository<TimeEntry> _repository;

    public TaskAssignedEventHandler(IRepository<TimeEntry> repository)
        => _repository = repository;

    public async Task Handle(TaskAssignedEvent notification, CancellationToken ct)
    {
        // idempotency guard — return early if already processed
        var alreadyProcessed = await _repository
            .AnyAsync(new TimeEntryByEventIdSpec(notification.EventId), ct);

        if (alreadyProcessed) return;

        var entry = new TimeEntry(notification.TaskId, notification.AssigneeId);
        await _repository.AddAsync(entry, ct);
        // no SaveChangesAsync — UnitOfWorkBehavior commits
    }
}
```

# Idempotency Strategies

**Check-before-act** — query for evidence the event was already processed. Use `EventId` as the key.
```csharp
var exists = await _repository.AnyAsync(new TimeEntryByEventIdSpec(notification.EventId), ct);
if (exists) return;
```

**Natural idempotency** — some operations are inherently safe to repeat.
```csharp
// Setting a status twice has no additional effect
entity.SetStatus(TaskStatus.Assigned);
```

**Upsert** — insert or ignore on conflict — naturally idempotent at database level.

# Rules
MUST:
- Implement `INotificationHandler<TEvent>`
- Be idempotent — same event twice produces same result
- Live in `{ModuleName}.Application/EventHandlers`
- Use `EventId` from `IDomainEvent` as idempotency key
- Return early (not throw) when duplicate detected
MUST NOT:
- Reference publishing module's Domain or Application directly
- Throw on duplicate event
- Assume exactly-once delivery
- Contain complex orchestration — one handler, one side effect

# Anti-patterns
- Non-idempotent handler — creates duplicate records on retry
- Handler throws on duplicate — blocks outbox retry, marks message as failed
- Handler depends on `{OtherModule}.Domain` — use Interfaces contract only
- Event handler placed inside `/Features` — belongs in `/EventHandlers`

# Checklist
- [ ] Handler implements `INotificationHandler<TEvent>`
- [ ] Idempotency guard present
- [ ] Handler in `/EventHandlers`
- [ ] Depends only on own repositories and source module's Interfaces
- [ ] Duplicate event returns early, does not throw

# Unittest TestCases
- [ ] When event handled first time Then side effect applied
- [ ] When same event handled twice Then side effect applied only once
- [ ] When duplicate detected Then handler returns without throwing

# Relations
- domain-event.skill — event definition this handler subscribes to
- ardalis-specification.skill — idempotency spec used in check-before-act guard
- repository.skill — IRepository used to stage side effect changes
- solution-domain-events.skill — full outbox delivery flow
