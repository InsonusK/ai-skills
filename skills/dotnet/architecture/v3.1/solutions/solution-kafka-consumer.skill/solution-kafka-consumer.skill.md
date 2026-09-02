---
name: solution-kafka-consumer
description: Skeleton — realizes AsyncInboundApi (VP12). A hosted background consumer per subscribed topic that deserializes each message and dispatches it into MediatR as a Command or Notification, with offset commit only after the handler succeeds.
whenToUse: when a module must react to asynchronous messages from another service — subscribing to a Kafka topic and turning each message into a MediatR dispatch
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - messaging
  - framework/kafka
  - framework/mediatr
  - solution/kafka-consumer
  - stack/dotnet
creates:
  - "App.Host.Messaging.{Topic}Consumer.cs"
extends:
  - "App.Host.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-messaging-infrastructure.skill/solution-messaging-infrastructure.skill.md|solution-messaging-infrastructure]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
---

> **Draft contract — no consumer yet.** VP12 has no v3 prior art. This skeleton fixes the shape (hosted consumer → deserialize → dispatch → commit-after-success). The retry/dead-letter policy, the envelope-to-request mapping convention, and the poison-message handling are finalized with the first real subscription.

# Goal
- Turn each message on a subscribed topic into exactly one MediatR dispatch: a Command when the module must act, a Notification when it is a fact to observe.
- Commit the consumer offset only after the handler returns success — at-least-once delivery, no message lost on a crash.
- Keep the mapping (topic + envelope `type` → request record) declarative and in one place per topic.

# Core Principle
- One hosted `BackgroundService` per topic (`{Topic}Consumer`), living in `App.Host` — it is composition, not module logic.
- The consumer deserializes the envelope, resolves the target request type, and calls `ISender.Send` / `IPublisher.Publish`. It contains no business logic.
- Offset commit is manual and follows a successful handler; a failed handler retries per policy, then routes to a dead-letter topic — never a silent drop, never an infinite hot loop.
- The request the consumer dispatches is a normal `{Module}.Interfaces` Command/Notification — the module's handler cannot tell it came from Kafka rather than an API.

# Boundaries
- The Kafka client, connection, and serializer are `solution-messaging-infrastructure`.
- Idempotency of the dispatched handler is the handler's responsibility (at-least-once means a message may be delivered twice) — this solution guarantees delivery, not exactly-once processing.
- Producing messages is `solution-kafka-producer` (VP13); the reliable-publish outbox is `solution-transactional-outbox` (VP14).

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-messaging-infrastructure.skill/solution-messaging-infrastructure.skill.md|solution-messaging-infrastructure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-messaging-infrastructure.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - the Kafka client + `KafkaOptions`
- [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs]] - the request kinds a consumer dispatches

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-kafka-consumer.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - add one `{Topic}Consumer : BackgroundService` per subscribed topic

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-kafka-consumer.skill/Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj]]
- Commit the offset only after the dispatched handler returns success.
  - Risk: committing before the handler runs loses the message if the process crashes mid-handle.
  - Fix: manual commit, after `Send`/`Publish` succeeds; on failure, retry then dead-letter.
- Keep the consumer free of business logic — deserialize, map, dispatch.
  - Risk: logic in the consumer is untestable through the module's own tests and duplicates handler code.
  - Fix: the consumer dispatches a `{Module}.Interfaces` request; the handler holds the logic.

# Check list
- [ ] One `{Topic}Consumer : BackgroundService` per subscribed topic, in `App.Host`.
- [ ] Offset commit follows a successful handler; failures retry then dead-letter.
- [ ] No business logic in the consumer.
- [ ] The dispatched request is a normal `{Module}.Interfaces` Command/Notification.
