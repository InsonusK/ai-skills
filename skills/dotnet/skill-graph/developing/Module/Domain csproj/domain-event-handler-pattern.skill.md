---
uid: 
status: draft 
name: domain-event-handler-pattern 
description: rules for implementing domain event handlers and ensuring idempotency 
domain: skill 
type: pattern 
tags:
- dotnet
- application
- events
- mediatR
- idempotency 
triggers:
- implement event handler
- domain event handler
- INotificationHandler
- idempotent handler
---
# Goal
Define how to implement MediatR notification handlers that react to domain events. Handlers are the only place where cross-module side effects are implemented. Because the outbox may re-deliver events on failure, every handler must be idempotent — processing the same event twice must produce the same result as processing it once. See [[skills/dotnet/skill-graph/developing/Architecture/solution/domain-event-architecture.skill]] for the full system flow.

# Core Principles
- Handler reacts to a fact — it never assumes the event will arrive exactly once
- Idempotency is the handler's responsibility — the outbox guarantees delivery, not exactly-once
- Handler never calls back into the publishing module's domain directly
- Cross-module handler depends only on `{SourceModule}.Interfaces` — never on Domain or Application
- Handler lives in `{Module}.Application/EventHandlers`

# Structure / Contracts
## Handler — {Module}.Application/EventHandlers
```csharp
// TimeLog.Application/EventHandlers/TaskAssignedEventHandler.cs
public class TaskAssignedEventHandler : INotificationHandler<TaskAssignedEvent>
{
    private readonly IRepository<TimeEntry> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task Handle(TaskAssignedEvent notification, CancellationToken ct)
    {
        // idempotency guard — check if already processed
        var alreadyProcessed = await _repository
            .AnyAsync(new TimeEntryByEventSpec(notification.EventId), ct);

        if (alreadyProcessed) return;

        // side effect
        var entry = new TimeEntry(notification.TaskId, notification.AssigneeId);
        await _repository.AddAsync(entry, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

## Idempotency strategies

**Check-before-act** — query for evidence the event was already processed:

```csharp
var exists = await _repository.AnyAsync(new ByEventIdSpec(notification.EventId), ct);
if (exists) return;
```

**Upsert** — use database upsert semantics, naturally idempotent:

```csharp
await _repository.UpsertAsync(entity, ct); // insert or ignore on conflict
```

**Natural idempotency** — some operations are inherently safe to repeat:

```csharp
// Setting a status is safe — setting it twice has no additional effect
entity.SetStatus(TaskStatus.Assigned);
```

# Rules
MUST:
- Implement `INotificationHandler<TEvent>`
- Be idempotent — same event delivered twice must produce the same outcome
- Live in `{Module}.Application/EventHandlers` 
SHOULD:
- Use `EventId` from `IDomainEvent` as the idempotency key
- Return early (not throw) when duplicate detected 
6MUST NOT:
- Reference publishing module's Domain or Application directly
- Throw on duplicate event — return early instead
- Assume exactly-once delivery

# Anti-patterns

- Handler with non-idempotent side effect — creates duplicate records on retry
- Handler throws exception on duplicate — blocks outbox retry, marks message as failed
- Handler depends on `{OtherModule}.Domain` directly — use Interfaces contract only
- Handler performs complex orchestration — keep handlers focused on one side effect

# Checklist

- [ ] Handler implements `INotificationHandler<TEvent>`
- [ ] Handler has explicit idempotency guard
- [ ] Handler lives in `{Module}.Application/EventHandlers`
- [ ] Handler depends only on own module's repositories and `{SourceModule}.Interfaces`
- [ ] Duplicate event returns early — does not throw

# Unittest TestCases

- [ ] When event handled for first time Then side effect applied
- [ ] When same event handled twice Then side effect applied only once
- [ ] When duplicate detected Then handler returns without throwing
- [ ] When handler throws Then no partial state change persisted

# Relations

- [[skills/dotnet/skill-graph/developing/Architecture/solution/domain-event-architecture.skill]] — system-level flow and architecture decisions
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-event-pattern.skill]] — event definition that this handler subscribes to
- [[skills/dotnet/skill-graph/developing/App/Infrastructure Layer/outbox-pattern.skill]] — delivers events to this handler via MediatR
- [[skills/dotnet/skill-graph/developing/Architecture/cross-module-interaction.skill]] — handlers are the cross-module reaction point