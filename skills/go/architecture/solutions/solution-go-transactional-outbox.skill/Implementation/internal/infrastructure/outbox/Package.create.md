---
description: Shape-only outbox table + relay
name: internal/infrastructure/outbox
element_kind: package
change_kind: create
tags:
  - solution/transactional-outbox
  - element/internal-infrastructure-outbox
---

> Draft contract — shape only, not yet grounded in a runnable example.

# Goals
- Write an outgoing message in the same transaction as the business change it reports; relay it to Kafka afterward.

# Structure

## Package Structure
```
internal/infrastructure/outbox/
  outbox.go   (table access: insert-within-transaction, select-pending)
  relay.go    (polls the table, calls solution-go-kafka-producer's publish port, marks relayed)
```

# What Does NOT Belong Here
- The business write itself — belongs to [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]'s own adapter; this package only adds the outbox-row write to the same transaction.

# Check list
- [ ] A real application of this solution grounds `outbox.go`/`relay.go` against a runnable example before relying on this shape.
