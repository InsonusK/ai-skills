---
name: solution-kafka-consumer
description: Reacts to asynchronous Kafka messages from other services — the AsyncInboundApi (VP5) realization, running as a long-running loop alongside any inbound API server
whenToUse: when a Go web-service needs to react to messages another service publishes to a Kafka topic
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/kafka-consumer
  - stack/go
  - concern/architecture
creates:
  - "internal/api/kafka/"
extends:
  - "cmd/{service}/main.go"
depends_on:
  - "[[skills/go/architecture/solutions/solution-messaging-infrastructure.skill/solution-messaging-infrastructure.skill.md|solution-messaging-infrastructure]]"
built_on_plateau:
adr:
---

> Draft contract — no consumer yet. No plateau in this catalog's first build realizes VP5 (see
> `skills/go/architecture/variability-map.md`). Full authoring is deferred until a real plateau
> composes this solution.

# Goal
- Give the module an inbound channel that reacts to Kafka messages, the same way [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] and [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] react to HTTP/gRPC requests — a thin translation into a call on the domain service.

# Core Principle
- Lives under `internal/api/kafka` (an inbound adapter), not `internal/infrastructure` — this is a *receiving* channel, structurally an inbound API like HTTP/gRPC, not an outbound port to another system.
- Runs as one more `g.Go` loop in `run()`'s `errgroup.Group`, per [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/cmd/{service}/main.go.extend.md|solution-grpc-api's own convention for a second concurrent server]].

# Template Skill Mutations
FILES:
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - add the consumer loop to the `errgroup.Group`, shape only

# Check list
- [ ] Before real authoring: confirm the Kafka client library via [[skills/go/architecture/solutions/solution-messaging-infrastructure.skill/solution-messaging-infrastructure.skill.md|solution-messaging-infrastructure]]'s own open Boundary.
