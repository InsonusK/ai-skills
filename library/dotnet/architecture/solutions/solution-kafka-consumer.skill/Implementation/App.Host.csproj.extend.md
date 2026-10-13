---
description: Add one hosted consumer per subscribed Kafka topic
name: "App.Host.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/kafka-consumer
  - element/app-host-csproj
---

> Draft — shape only; retry/dead-letter policy and envelope mapping finalized with the first real subscription.

# Structure
```
/App.Host
  /Messaging
    {Topic}Consumer.cs
```

# Implementation changes (sketch)

```csharp
public sealed class OrderEventsConsumer(ISender sender, IPublisher publisher, /* consumer factory */) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var msg = /* consume one */;
            var request = Map(msg.Envelope);          // envelope type -> {Module}.Interfaces request
            var ok = request switch
            {
                ICommandBase c => (await sender.Send(c, ct)).IsSuccess,
                INotificationEvent n => await Publish(n, ct),
                _ => false
            };
            if (ok) Commit(msg);
            else await RetryOrDeadLetter(msg, ct);
        }
    }
}
```

# Rules

## MUST
- Commit only after a successful dispatch.
  - Risk: pre-commit loses the message on a crash.
  - Fix: manual commit after success; retry/dead-letter on failure.

# Check list
- [ ] One `{Topic}Consumer : BackgroundService` per topic.
- [ ] Commit-after-success; no silent drop.
