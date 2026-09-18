---
name: solution-go-transactional-outbox
description: Writes an outgoing message to a transactional outbox table in the same transaction as the persisted business change, then relays it to Kafka — required, not merely optional, once a publication is triggered by a change solution-persistent-db already persists
whenToUse: when a Go web-service both persists data (solution-persistent-db) and publishes to Kafka (solution-go-kafka-producer), and a specific publication is triggered by a change to that persisted data
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/transactional-outbox
  - stack/go
  - concern/architecture
creates:
  - "internal/infrastructure/outbox/"
extends:
  - "internal/infrastructure/{store}/store.go"
depends_on:
  - "[[skills/go/architecture/solutions/solution-go-kafka-producer.skill/solution-go-kafka-producer.skill.md|solution-go-kafka-producer]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
built_on_plateau:
adr:
---

> Draft contract — no consumer yet. No plateau in this catalog's first build realizes VP4 (see
> `skills/go/architecture/variability-map.md`). Full authoring is deferred until a real plateau
> composes this solution.

# Goal
- Guarantee a Kafka publication triggered by a persisted-data change is never lost or duplicated relative to that change — by writing both inside the same database transaction, then relaying the outbox row to Kafka asynchronously.

# Core Principle
- The business write and the outbox-row write happen in one PostgreSQL transaction, via [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]'s own connection pool — never as two separate, independently-committed operations ("dual write"), which is exactly the failure mode this pattern exists to close.
- A separate relay process/goroutine polls the outbox table and calls [[skills/go/architecture/solutions/solution-go-kafka-producer.skill/solution-go-kafka-producer.skill.md|solution-go-kafka-producer]]'s own publish port — this solution does not reimplement publishing, it sequences it.

# Boundaries
- Per [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]'s own Boundaries, this solution's own outbox-table schema is likewise created with `CREATE TABLE IF NOT EXISTS`, not real migration tooling.

# Template Skill Mutations
FILES:
- [[./Implementation/internal/infrastructure/outbox/Package.create.md|internal/infrastructure/outbox]] - create - outbox table + relay loop, shape only

# Check list
- [ ] Applied only when both [[skills/go/architecture/solutions/solution-go-kafka-producer.skill/solution-go-kafka-producer.skill.md|solution-go-kafka-producer]] and [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] are present on the same plateau, per the Variability Map's VP4 constraint.
