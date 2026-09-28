---
name: registry-internal-domain-interfaces-store-port-go
description: Conflict Detection result for the `internal-domain-interfaces-store-port-go` element
tags:
  - concern/architecture
  - stack/go
  - element/internal-domain-interfaces-store-port-go
---

# Element
`internal-domain-interfaces-store-port-go` — the business-named durable-storage port (`LinkHistory` in the examples).

# Involved solutions
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.create`)
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] (`.extend`)

# Classification
`TMN` — **T**: VP-C003 requires a store; `solution-taskbox-in-go` `depends_on` `solution-persistent-db`. **M**: the write method gains `tasks ...Task`. **N**: one creator, one extender; a variadic parameter leaves every existing call site compiling.

# Ordering
`source: constraint`.

# Resolution
Canonical — no resolver. Every stub of the port must record the tasks (see the extension's check list).

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `gw009-001` (GW009.001) | 2 | First real: `Record(ctx, entry, tasks ...Task)` | Domain scenarios assert the tasks passed with `Record` via the stub |
