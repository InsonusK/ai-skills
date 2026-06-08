---
uid: 7ab5cacf-a198-4814-9a20-3c7f370b5f52
status: draft
name: outbox-pattern
description: rules for implementing the transactional outbox — interceptor, OutboxMessage, and background dispatcher
domain: skill
type: template
tags:
  - dotnet
  - infrastructure
  - outbox
  - events
  - mediatR
triggers:
  - outbox implementation
  - transactional outbox
  - domain event persistence
  - background event dispatcher
---
# Goal
Define how domain events collected on entities are atomically persisted to the outbox table and dispatched to MediatR by a background service. The outbox guarantees that no event is lost even if the application crashes after a transaction commits. See [[skills/dotnet/skill-graph/developing/Architecture/solution/domain-event-architecture.skill]] for the full system flow and the decision behind this approach.

# Core Principles
- OutboxMessage written in same transaction as state change — atomicity is the guarantee
- Background dispatcher is the only component that publishes to MediatR
- `ProcessedAt` is set only after successful MediatR publish — enables safe retry
- `OutboxMessage` and base interceptor contract live in BuildingBlocks — reusable across repos
- Concrete interceptor and dispatcher implementation live in App.Infrastructure

# Structure / Contracts
## OutboxMessage — BuildingBlocks
```csharp
// BuildingBlocks/Outbox/OutboxMessage.cs
public class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; }      // assembly-qualified type name
    public string Payload { get; init; }   // JSON-serialized event
    public DateTime OccurredAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
}
```

## IHasDomainEvents — BuildingBlocks
Allows the interceptor to work across all entity types without listing them.
```csharp
// BuildingBlocks/Outbox/IHasDomainEvents.cs
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

## EF SaveChanges interceptor — App.Infrastructure
```csharp
// App.Infrastructure/Outbox/DomainEventInterceptor.cs
public class DomainEventInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct)
    {
        var dbContext = eventData.Context;
        if (dbContext is null) return await base.SavingChangesAsync(eventData, result, ct);

        var entities = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .ToList();

        var outboxMessages = entities
            .SelectMany(e => e.DomainEvents)
            .Select(e => new OutboxMessage
            {
                Id = e.EventId,
                Type = e.GetType().AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(e, e.GetType()),
                OccurredAt = e.OccurredAt
            }).ToList();

        await dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages, ct);
        entities.ForEach(e => e.ClearDomainEvents());

        return await base.SavingChangesAsync(eventData, result, ct);
    }
}
```

## Background dispatcher — App.Infrastructure
```csharp
// App.Infrastructure/Outbox/OutboxDispatcher.cs
public class OutboxDispatcher : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var messages = await _dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.OccurredAt)
                .Take(20)
                .ToListAsync(ct);

            foreach (var message in messages)
            {
                var eventType = Type.GetType(message.Type)!;
                var domainEvent = (IDomainEvent)JsonSerializer
                    .Deserialize(message.Payload, eventType)!;

                await _mediator.Publish(domainEvent, ct);
                message.ProcessedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
```

## EF Configuration — App.Infrastructure
```csharp
// App.Infrastructure/Persistence/Configurations/OutboxMessageConfig.cs
public class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.OccurredAt).IsRequired();
    }
}
```

# Rules
MUST:
- `OutboxMessage` and `IHasDomainEvents` live in BuildingBlocks
- Interceptor writes outbox rows in same SaveChanges call as state change
- `Type` stores assembly-qualified name — required for safe cross-version deserialization
- `ProcessedAt` set only after successful `_mediator.Publish`
- `DomainEventInterceptor` registered on DbContext in App.Infrastructure
- `OutboxDispatcher` registered as hosted service in App.Host
- Entity implements `IHasDomainEvents` — see [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-event-pattern.skill]] 
MUST NOT:
- Dispatch events directly from application layer after SaveChanges
- Set `ProcessedAt` before publish — prevents retry on failure
- Use `Type.FullName` — use `AssemblyQualifiedName` for safe deserialization

# Anti-patterns
- Dispatching events in application layer after SaveChanges without outbox — crash loses events
- `ProcessedAt` set before MediatR publish — failed handlers silently skipped
- `Type` stored as short name — breaks deserialization after namespace changes
- Single dispatcher processing all messages in one unbounded query — use `Take(N)` batching

# Checklist
- [ ] `OutboxMessage` defined in BuildingBlocks
- [ ] `IHasDomainEvents` defined in BuildingBlocks
- [ ] Entity implements `IHasDomainEvents`
- [ ] `DomainEventInterceptor` registered on DbContext
- [ ] `OutboxMessage` EF configuration applied
- [ ] `OutboxDispatcher` registered as `BackgroundService` in DI
- [ ] `Type` uses `AssemblyQualifiedName`
- [ ] `ProcessedAt` set after successful publish only

# Unittest TestCases
- [ ] When SaveChanges called with entity that has domain events Then OutboxMessage rows created
- [ ] When SaveChanges called Then entity DomainEvents list is cleared
- [ ] When OutboxDispatcher runs Then MediatR Publish called for each unprocessed message
- [ ] When MediatR Publish throws Then ProcessedAt remains null
- [ ] When same message processed twice Then handler receives event twice (idempotency is handler's responsibility)

# Relations
- [[skills/dotnet/skill-graph/developing/Architecture/solution/domain-event-architecture.skill]] — system-level flow and architecture decisions
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-event-pattern.skill]] — event definition and entity collection
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/domain-event-handler-pattern.skill]] — handler implementation and idempotency
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-configuration.class.skill]] — OutboxMessage EF table configuration pattern