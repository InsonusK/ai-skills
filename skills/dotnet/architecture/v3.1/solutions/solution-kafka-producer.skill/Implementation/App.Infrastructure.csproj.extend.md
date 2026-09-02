---
description: Add the IMessagePublisher contract and its Kafka implementation
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/kafka-producer
  - element/app-infrastructure-csproj
---

> Draft — shape only; topic naming and direct-vs-outbox default finalized with the first real publisher.

# Structure
```
/Shared/Messaging/IMessagePublisher.cs
/App.Infrastructure/Messaging/KafkaMessagePublisher.cs
```

# Implementation changes (sketch)

```csharp
// Shared/Messaging/IMessagePublisher.cs
public interface IMessagePublisher
{
    Task Publish<T>(T @event, string topic, CancellationToken ct) where T : notnull;
}

// App.Infrastructure/Messaging/KafkaMessagePublisher.cs
public sealed class KafkaMessagePublisher(/* producer factory */, ISerializer serializer) : IMessagePublisher
{
    public Task Publish<T>(T @event, string topic, CancellationToken ct) where T : notnull
    {
        var envelope = Envelope.For(@event);   // type / id / occurredAt / traceId
        return /* produce serializer.Serialize(envelope) to topic */;
    }
}
```

# Rules

## MUST
- `IMessagePublisher` in `Shared`; the Kafka impl in `App.Infrastructure`.
  - Risk: the contract in `App.Infrastructure` forces handlers to reference infrastructure.
  - Fix: contract in `Shared`, impl behind DI.

# Check list
- [ ] `IMessagePublisher` in `Shared/Messaging`.
- [ ] `KafkaMessagePublisher` wraps payloads in the shared envelope.
