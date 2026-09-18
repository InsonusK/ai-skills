---
description: Shape-only shared Kafka connection config
project_name: internal/infrastructure/kafka
name: kafka
element_kind: functions
change_kind: create
tags:
  - solution/messaging-infrastructure
  - element/internal-infrastructure-kafka-config-go
---

> Draft contract — shape only, not yet grounded in a chosen client library or a runnable example.

# Goals
- Give the producer and consumer adapters one shared broker-address setting.

# Implementation changes
```go
// Package kafka holds connection settings shared by the Kafka producer and
// consumer adapters. Which client library backs it is not yet decided —
// see this solution's own Boundaries.
package kafka

type Config struct {
	Brokers []string
}
```

# Check list
- [ ] A real application of this solution replaces this shape with the chosen client library's actual connection type before relying on it.
