---
name: registry-internal-infrastructure-store-store-go
description: Conflict Detection result for the `internal-infrastructure-store-store-go` element
tags:
  - concern/architecture
  - stack/go
  - element/internal-infrastructure-store-store-go
---

# Element
`internal-infrastructure-store-store-go` — the PostgreSQL data adapter (`internal/infrastructure/linkstore/store.go` in the examples).

# Involved solutions
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.create`)
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] (`.extend`)
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] (`.extend`)

# Classification
`TMN` — **T**: `solution-go-db-migrations` `depends_on` `solution-persistent-db`, and VP-C003 TaskBox requires a store (`solution-taskbox-in-go` `depends_on` both). **M**: `go-db-migrations` removes the inline DDL from `New`; `taskbox-in-go` changes `New` again (pool and TaskBox store injected) and wraps `Record`'s insert in a `pgx.Tx` that also enqueues. **N**: both touch `New`, but each later delta is authored against the earlier one's TO BE as its AS IS, and the constraints admit only that order — the later author merges alone, which is the [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md#the-wraprelocate-footnote-fmc-vs-fmn|wrap/relocate test]] passed with the order fixed by `depends_on`.

# Ordering
`source: constraint` — `solution-persistent-db` → `solution-go-db-migrations` → `solution-taskbox-in-go`.

# Resolution
Canonical — no resolver. `solution-taskbox-in-go`'s `store.go.extend.md` states the merged result in full.

# Architectural signal
N=3 on one adapter struct, all three changing `New`. The element is the seam where a data write meets its schema and its follow-up tasks; a later mechanism writing through this adapter (Outbox, VP-C010) will land here too — keep its delta authored against the latest TO BE, or split `Record`'s transaction into a reusable helper before N reaches 4.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `gw009-001` (GW009.001) | 3 | First real: `solution-go-db-migrations` was never composed before; it and `solution-taskbox-in-go` join `solution-persistent-db` | Conformance feature on PostgreSQL 18; an enqueue error rolls back the insert (one `BeginFunc`); smoke test: history row and task committed together |
