---
name: solution-transactional-outbox
description: Skeleton — realizes OutboxPattern (VP14). Writes an outgoing message to an outbox table in the same DbContext transaction as the business change, then a relay background service publishes committed rows via IMessagePublisher and marks them sent. Makes outbound publishing consistent with the business write.
whenToUse: when a module publishes an asynchronous message that must not be lost or duplicated relative to the business change that caused it — replacing a direct IMessagePublisher.Publish call in a handler with an outbox write
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - messaging
  - outbox
  - reliability
  - framework/ef-core
  - solution/transactional-outbox
  - stack/dotnet
creates:
  - "App.Infrastructure.Outbox.OutboxMessage.cs"
  - "App.Infrastructure.Outbox.OutboxRelay.cs"
  - "App.Infrastructure.Outbox.OutboxMessageConfig.cs"
  - "Shared.Messaging.IOutboxWriter.cs"
extends:
  - "App.Infrastructure.csproj"
  - "App.Host.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-kafka-producer.skill/solution-kafka-producer.skill.md|solution-kafka-producer]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
---

> **Draft contract — no realization yet.** VP14 has no v3 prior art. This skeleton fixes the shape (outbox row written in the business transaction; a relay publishes and marks sent). The relay's polling/CDC strategy, the ordering guarantee, and the retention/cleanup policy are finalized with the first real use.

# Goal
- Make an outbound message and the business change that produced it commit or roll back together: the handler writes an `OutboxMessage` row through `IOutboxWriter`, in the same `DbContext` as the entity change; the unit-of-work commit persists both atomically.
- A single `OutboxRelay` background service reads committed, unsent rows, publishes each via `IMessagePublisher`, and marks it sent — at-least-once, in write order per aggregate.

# Core Principle
- The handler calls `IOutboxWriter.Add(evt, topic)` **instead of** `IMessagePublisher.Publish(...)` — no direct publish from a handler once the outbox is in use.
- The outbox row is staged in the same `DbContext` change set as the entity mutation; `UnitOfWorkBehavior`'s commit is the single atomic boundary.
- The relay is the only publisher of outbox rows; it is idempotent on the consumer side (at-least-once) and never blocks a request.
- `Requires VP13 (AsyncOutboundApi) AND VP2 (Persistence)` — per the [Variability Map](skills/dotnet/architecture/v3.1/variability-map.md), jointly.

# Boundaries
- The publish transport is `solution-kafka-producer` (VP13); this solution reuses its `IMessagePublisher`.
- The transaction/commit boundary is `solution-unit-of-work`; this solution adds a row to its change set, it does not manage transactions itself.
- Exactly-once end-to-end is not promised — the outbox gives at-least-once with no loss; consumers must be idempotent.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-kafka-producer.skill/solution-kafka-producer.skill.md|solution-kafka-producer]]
  - `IMessagePublisher` - the relay publishes through it
- [[skills/dotnet/architecture/v3.1/solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]
  - `AppDbContext` - the outbox table lives here; the row is staged in the same change set
- [[skills/dotnet/architecture/v3.1/solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]
  - the commit that persists the outbox row atomically with the business change

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-transactional-outbox.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - add `Outbox/` (message, config, relay) and the `Shared` writer contract

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-transactional-outbox.skill/Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj]]
- Write the outbox row through `IOutboxWriter` into the same `DbContext` as the business change — never a direct publish from the handler once the outbox is in use.
  - Risk: a direct publish that succeeds while the business transaction rolls back emits an event for a change that never happened.
  - Fix: stage the row; the unit-of-work commit is the atomic boundary.
- Keep the relay the sole publisher of outbox rows, non-blocking, marking each row sent only after a successful publish.
  - Risk: publishing from two places, or marking sent before publish, drops or duplicates messages beyond at-least-once.
  - Fix: one relay `BackgroundService`, mark-sent-after-success.

# Check list
- [ ] `OutboxMessage` table configured in `AppDbContext`; row written via `IOutboxWriter` in the handler's `DbContext`.
- [ ] No direct `IMessagePublisher.Publish` in a handler that uses the outbox.
- [ ] One `OutboxRelay : BackgroundService`; marks sent only after a successful publish.
- [ ] Applied only when VP13 and VP2 are both present.
