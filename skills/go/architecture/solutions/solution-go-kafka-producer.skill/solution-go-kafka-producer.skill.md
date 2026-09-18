---
name: solution-go-kafka-producer
description: Publishes domain events to Kafka directly, through an outbound port — the direct-publish realization of AsyncOutboundApi (VP3), superseded by the outbox pattern for a publication triggered by a persisted-data change once PersistentDb is also present
whenToUse: when a Go web-service needs to publish an event for other services to react to, and the publication is not itself triggered by a persisted-data change (otherwise see solution-go-transactional-outbox)
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/kafka-producer
  - stack/go
  - concern/architecture
creates:
  - "internal/domain/interfaces/{publisher-port}.go"
  - "internal/infrastructure/kafkaproducer/"
extends:
  - "internal/domain/services/{service}.go"
depends_on:
  - "[[skills/go/architecture/solutions/solution-go-messaging-infrastructure.skill/solution-go-messaging-infrastructure.skill.md|solution-go-messaging-infrastructure]]"
  - "[[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]"
built_on_plateau:
adr:
---

> Draft contract — no consumer yet. No plateau in this catalog's first build realizes VP3 (see
> `skills/go/architecture/variability-map.md`). Full authoring — including the AS IS/TO BE
> extension of `{service}.go` and a real ground-truth example — is deferred until a real plateau
> composes this solution.

# Goal
- Give the domain a business-named outbound port for publishing one kind of event directly to Kafka.

# Core Principle
- Same port/adapter shape as every other outbound solution in this catalog ([[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]], [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]) — the port is named for the event published, never for Kafka itself.

# Boundaries
- Does not decide *whether* a given publication must instead go through [[skills/go/architecture/solutions/solution-go-transactional-outbox.skill/solution-go-transactional-outbox.skill.md|solution-go-transactional-outbox]] — that determination (VP4's constraint: required once [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] is also present and the publication is triggered by the persisted change) is recorded in `skills/go/architecture/variability-map.md`, not repeated here.

# Template Skill Mutations
FILES:
- [[./Implementation/internal/domain/interfaces/{publisher-port}.go.create.md|internal/domain/interfaces/{publisher-port}.go]] - create - the outbound publish port, shape only

# Check list
- [ ] Before real authoring: confirm the Kafka client library via [[skills/go/architecture/solutions/solution-go-messaging-infrastructure.skill/solution-go-messaging-infrastructure.skill.md|solution-go-messaging-infrastructure]]'s own open Boundary.
