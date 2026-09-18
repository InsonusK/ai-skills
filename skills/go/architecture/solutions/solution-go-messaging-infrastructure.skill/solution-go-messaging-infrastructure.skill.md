---
name: solution-go-messaging-infrastructure
description: Shared Kafka connection/config plumbing that solution-go-kafka-producer and solution-go-kafka-consumer both depend on
whenToUse: when a Go web-service needs to publish or consume Kafka messages for the first time and no shared broker configuration exists yet
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/messaging-infrastructure
  - stack/go
  - concern/architecture
creates:
  - "internal/infrastructure/kafka/config.go"
extends:
  - "internal/config/config.go"
depends_on:
built_on_plateau:
adr:
---

> Draft contract — no consumer yet. No plateau in this catalog's first build composes
> [[skills/go/architecture/solutions/solution-go-kafka-producer.skill/solution-go-kafka-producer.skill.md|solution-go-kafka-producer]] or
> [[skills/go/architecture/solutions/solution-go-kafka-consumer.skill/solution-go-kafka-consumer.skill.md|solution-go-kafka-consumer]]
> yet (see `skills/go/architecture/variability-map.md` VP3/VP5). This solution's shape is
> plausible, not verified against a runnable example — treat it as a starting point, not a
> finished contract, until a real plateau consumes it.

# Goal
- Give both the producer and consumer solutions one shared place for the broker address and any connection-wide settings, mirroring [[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]'s shared-prerequisite shape for the outbound-port packages.

# Core Principle
- Holds connection/config plumbing only — no publish or consume logic, which belong to the producer/consumer solutions themselves.

# Boundaries
- Which Kafka client library this catalog standardizes on is not yet decided — a real application of this solution should confirm a choice (e.g. `github.com/twmb/franz-go` or `github.com/segmentio/kafka-go`) and record it as an ADR before relying on this skeleton for real code.

# Template Skill Mutations
FILES:
- [[./Implementation/internal/infrastructure/kafka/config.go.create.md|internal/infrastructure/kafka/config.go]] - create - shared broker-address config, shape only

# Check list
- [ ] A Kafka client library has been chosen and recorded as an ADR before this solution is applied for real.
