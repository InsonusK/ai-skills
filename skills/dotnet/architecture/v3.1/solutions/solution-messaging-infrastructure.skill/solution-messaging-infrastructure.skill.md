---
name: solution-messaging-infrastructure
description: Skeleton — introduces the Kafka connection, serialization, and health-check wiring in App.Infrastructure that solution-kafka-consumer (VP12) and solution-kafka-producer (VP13) both build on. Adds no consumer or producer of its own.
whenToUse: when a module first needs asynchronous messaging (inbound or outbound) and the Kafka client + configuration are not wired yet, or when reviewing where broker connection settings and serializer registration live
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - messaging
  - framework/kafka
  - solution/messaging-infrastructure
  - stack/dotnet
creates:
  - "App.Infrastructure.Messaging.KafkaRegistration.cs"
  - "App.Infrastructure.Messaging.KafkaOptions.cs"
extends:
  - "App.Host.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]]"
---

> **Draft contract — no consumer yet.** VP12/VP13 have no realization in v3 and no service currently needs Kafka. This skeleton fixes the shape (one place for broker config, one serializer policy, one health check); the concrete `KafkaRegistration` body, the serializer choice, and the options schema are finalized when the first real consumer/producer is built.

# Goal
- Give async messaging one home for the Kafka client: connection settings (`KafkaOptions` bound from configuration), a single serializer policy, and a broker health check — all in `App.Infrastructure`.
- Be the shared prerequisite for `solution-kafka-consumer` (VP12) and `solution-kafka-producer` (VP13); add neither.

# Core Principle
- Broker connection details live in configuration bound to `KafkaOptions` — never hard-coded, never per-module.
- One serializer policy (JSON with a schema-version header, pending final decision) for every topic; a message envelope carries `type`, `id`, `occurredAt`, `traceId`.
- `App.Infrastructure` owns the client; a module never references the Kafka client library directly — it sees only the consumer/producer abstractions the VP12/VP13 solutions add.
- Registration is one `AddKafkaMessaging(configuration)` call from `App.Host`.

# Boundaries
- Consumers (`solution-kafka-consumer`), producers (`solution-kafka-producer`), and the transactional outbox (`solution-transactional-outbox`) are separate solutions that build on this one.
- Persistence (VP2) is not required by this solution, but the outbox that makes publishing reliable does require it.
- The message envelope/serialization contract is shared with any non-.NET service on the same topics — that cross-service contract is out of scope here and belongs with the topic's owning team.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj]] - hosts `Messaging/KafkaRegistration.cs`

NUGET (final choice pending):
- a Kafka client (`Confluent.Kafka` is the likely choice) — version in `Directory.Packages.props`

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-messaging-infrastructure.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - add `Messaging/` with `KafkaRegistration` + `KafkaOptions`

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-messaging-infrastructure.skill/Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj]]
- Bind all broker settings from configuration into `KafkaOptions`; never hard-code a bootstrap server or topic.
  - Risk: environment-specific values baked into code cannot be changed per environment and leak broker topology into the repo.
  - Fix: `services.Configure<KafkaOptions>(configuration.GetSection("Kafka"))`.
- Keep the Kafka client library referenced only by `App.Infrastructure`.
  - Risk: a module referencing the client directly couples domain/application code to a transport library.
  - Fix: modules see only the consumer/producer abstractions from the VP12/VP13 solutions.

# Check list
- [ ] `App.Infrastructure/Messaging/KafkaRegistration.cs` + `KafkaOptions.cs` exist.
- [ ] `AddKafkaMessaging(configuration)` is the only Kafka wiring in `Program.cs`.
- [ ] No project except `App.Infrastructure` references the Kafka client library.
- [ ] A broker health check is registered.
