---
description: Add the outbox table, writer contract, and relay
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/transactional-outbox
  - element/app-infrastructure-csproj
---

> Draft — shape only; relay polling/CDC strategy, ordering, and retention finalized with the first real use.

# Structure
```
/Shared/Messaging/IOutboxWriter.cs
/App.Infrastructure/Outbox/OutboxMessage.cs
/App.Infrastructure/Outbox/OutboxMessageConfig.cs
/App.Infrastructure/Outbox/OutboxRelay.cs
```

# Implementation changes (sketch)

```csharp
// Shared/Messaging/IOutboxWriter.cs
public interface IOutboxWriter { void Add<T>(T @event, string topic) where T : notnull; }

// App.Infrastructure/Outbox/OutboxMessage.cs
public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string Topic { get; init; } = "";
    public string Payload { get; init; } = "";     // serialized envelope
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset? SentAt { get; set; }
}

// App.Infrastructure/Outbox/OutboxRelay.cs
public sealed class OutboxRelay(/* db context factory */, IMessagePublisher publisher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // read committed rows where SentAt is null, in order; publish; set SentAt
    }
}
```

`IOutboxWriter.Add` stages the row into the request's `AppDbContext`, so `UnitOfWorkBehavior`'s commit persists it atomically with the business change.

# Rules

## MUST
- Stage the outbox row in the handler's `DbContext`, not a separate connection.
  - Risk: a separate write is not atomic with the business change.
  - Fix: `IOutboxWriter.Add` uses the same `AppDbContext`.
- The relay marks a row sent only after `IMessagePublisher.Publish` succeeds.
  - Risk: mark-then-publish loses the message on a crash between the two.
  - Fix: publish, then set `SentAt`.

# Check list
- [ ] `OutboxMessage` + `OutboxMessageConfig` + `OutboxRelay` exist; `IOutboxWriter` in `Shared`.
- [ ] Row staged in the handler's `DbContext`; relay is the only publisher; mark-sent-after-success.
