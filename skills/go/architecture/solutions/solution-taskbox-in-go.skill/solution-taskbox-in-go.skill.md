---
name: solution-taskbox-in-go
description: Pointer to taskbox-go, the Go library realizing TaskBox (VP-C003), and its usage skill for wiring a Go service; until its first release the proven pre-release code lives in plateau GW009.001's example
whenToUse: when a Go web-service with PostgreSQL must run work later, retry it, or keep it in order per key — in particular work enqueued atomically with a data change — or when reviewing a Go service's TaskBox wiring
domain: skill
type: architecture
version: 20260929000000
updated: 20260929
tags:
  - skill/architecture/solution
  - solution/taskbox-in-go
  - stack/go
  - concern/architecture
  - taskbox
creates:
extends:
depends_on:
  - "[[skills/common-workflow/architecture/solutions/solution-taskbox.skill/solution-taskbox.skill.md|solution-taskbox]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
built_on_plateau:
adr:
---

# Goal
- The Go service wired to `taskbox-go` exactly as the library's usage skill describes.

# Core Principle
- **The library owns mechanism and wiring** - `taskbox-go` (mock URL until published: `https://github.com/InsonusK/taskbox-go`) holds the code and `doc/skills/taskbox-go-usage` — the seams a service writes (the `Task` domain value, a data port taking tasks, the adapter's `pgx.Tx`, task handlers as an inbound adapter, the worker in `main`, the migration). This skill adds nothing to them.
- **Pre-release** - Until `taskbox-go` v0.1.0 exists, the proven code is `internal/taskbox` of [[skills/go/architecture/plateau/gw009-001/plateau-gw009-001.skill/plateau-gw009-001.skill.md|GW009.001]]'s example; the plateau switches to the module dependency at that release.

# Rule

## MUST

### Apply the base first
Apply [[skills/common-workflow/architecture/solutions/solution-taskbox.skill/solution-taskbox.skill.md|solution-taskbox]] (the spec's `taskbox-usage` skill) before the Go wiring.
- Risk: correct Go wiring around a wrong use — a Critical task in Redis, a handler that swallows failures.
- Fix: check the service's task types against `taskbox-usage` first.

### Follow the library's usage skill
Wire the service by `taskbox-go`'s `doc/skills/taskbox-go-usage` at the library release in `go.mod`; never copy the library's code into the service.
- Risk: copied mechanism code loses the guarantees its conformance run proved.
- Fix: depend on the module; before v0.1.0, take the pre-release code from GW009.001's example unchanged.

# Check list
- [ ] `taskbox-go`'s usage skill was followed; the data write and its tasks share one `pgx.Tx`.
- [ ] The service's `go.mod` pins a `taskbox-go` release (or, before v0.1.0, `internal/taskbox` equals GW009.001's example).
