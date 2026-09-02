---
name: solution-kafka-producer
description: Skeleton — realizes AsyncOutboundApi (VP13). An IMessagePublisher abstraction in Shared plus a Kafka implementation in App.Infrastructure that serializes a domain event into the shared envelope and publishes it to a topic. A handler publishes by calling IMessagePublisher, not the Kafka client.
whenToUse: when a module must publish an asynchronous message for other services to react to — emitting a domain event to a Kafka topic from a handler
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - messaging
  - framework/kafka
  - solution/kafka-producer
  - stack/dotnet
creates:
  - "Shared.Messaging.IMessagePublisher.cs"
  - "App.Infrastructure.Messaging.KafkaMessagePublisher.cs"
extends:
  - "Shared.csproj"
  - "App.Infrastructure.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-messaging-infrastructure.skill/solution-messaging-infrastructure.skill.md|solution-messaging-infrastructure]]"
---

> **Draft contract — no producer yet.** VP13 has no v3 prior art. This skeleton fixes the shape (`IMessagePublisher` in `Shared`, Kafka impl in `App.Infrastructure`, envelope from `solution-messaging-infrastructure`). The topic-naming convention and the "publish directly vs via outbox" default are finalized with the first real publisher.

# Goal
- Give a handler one way to publish an outbound message — `IMessagePublisher.Publish(evt, topic)` — with no reference to the Kafka client.
- Wrap the payload in the shared envelope (`type`, `id`, `occurredAt`, `traceId`) so consumers on any stack can process it.

# Core Principle
- `IMessagePublisher` is a `Shared` contract; `KafkaMessagePublisher` is its only implementation, in `App.Infrastructure`.
- A handler publishes a `{Module}.Interfaces` event record; the publisher serializes it into the envelope and sends it — the handler never sees Kafka types.
- **Direct publish is best-effort.** A publish inside a business transaction that must not be lost if the commit rolls back needs `solution-transactional-outbox` (VP14), not this solution alone.

# Boundaries
- The Kafka client, connection, and serializer are `solution-messaging-infrastructure`.
- Reliable, transaction-consistent publishing (write-then-relay) is `solution-transactional-outbox` (VP14), which `depends_on` this solution and persistence.
- Consuming messages is `solution-kafka-consumer` (VP12).

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-messaging-infrastructure.skill/solution-messaging-infrastructure.skill.md|solution-messaging-infrastructure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-messaging-infrastructure.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - the Kafka client + serializer + envelope

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-kafka-producer.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - add `Messaging/KafkaMessagePublisher.cs` and the `Shared` contract

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-kafka-producer.skill/Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj]]
- Publish through `IMessagePublisher`, never the Kafka client, from a handler.
  - Risk: a handler referencing the client couples application code to a transport library and to `App.Infrastructure`.
  - Fix: inject `IMessagePublisher` (a `Shared` contract).
- Wrap every payload in the shared envelope.
  - Risk: a bare payload gives consumers no type discriminator, id, or trace context.
  - Fix: `type` / `id` / `occurredAt` / `traceId` on every message.

# Check list
- [ ] `Shared/Messaging/IMessagePublisher.cs` + `App.Infrastructure/Messaging/KafkaMessagePublisher.cs` exist.
- [ ] Handlers publish via `IMessagePublisher` only.
- [ ] Every message carries the shared envelope.
- [ ] A publish that must be transaction-consistent uses `solution-transactional-outbox`.
