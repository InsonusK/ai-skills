---
name: solution-go-messaging-infrastructure
description: Shared Kafka connection/config plumbing that solution-go-kafka-producer and solution-go-kafka-consumer both depend on
whenToUse: when a Go web-service needs to publish or consume Kafka messages for the first time and no shared broker configuration exists yet
domain: skill
type: architecture
version: 20261128000000
updated: 20261128
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

This contract supplies shared broker configuration shapes; the application selects the Kafka client and implements its connection wiring.

# Goal
- Give both the producer and consumer solutions one shared place for the broker address and any connection-wide settings, mirroring [[library/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]'s shared-prerequisite shape for the outbound-port packages.

# Core Principle
- Holds connection/config plumbing only — no publish or consume logic, which belong to the producer/consumer solutions themselves.

# Boundaries
- Select the project's Kafka client library (e.g. `github.com/twmb/franz-go` or `github.com/segmentio/kafka-go`) and record the choice as a project ADR before implementing the connection and messaging adapters.

# Template Skill Mutations
FILES:
- [[./Implementation/internal/infrastructure/kafka/config.go.create.md|internal/infrastructure/kafka/config.go]] - create - shared broker-address config, shape only

# Check list
- [ ] A Kafka client library has been chosen and recorded as an ADR before this solution is applied for real.
