---
name: registry-internal-infrastructure-store
description: Conflict Detection result for the `internal-infrastructure-store` element
tags:
  - concern/architecture
  - stack/go
  - element/internal-infrastructure-store
---

# Element
`internal-infrastructure-store` — the PostgreSQL adapter package and its `migrations/` directory.

# Involved solutions
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.create`)
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] (`.extend`)
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] (`.extend`)

# Classification
`TMN` — **T**: the same `depends_on` chain as [[./internal-infrastructure-store-store-go.md|internal-infrastructure-store-store-go]]. **M**: `go-db-migrations` adds `migrations.go` and `migrations/`; `taskbox-in-go` adds one file to `migrations/`. **N**: new files only; migration numbers are assigned at application time (next free number).

# Ordering
`source: constraint` — the TaskBox migration takes the next number after the service's latest.

# Resolution
Canonical — no resolver.

# Architectural signal
None at N=3: additions are new files. The migration directory is the service's one schema history; every later mechanism schema (Outbox, Inbox) is another numbered file here.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `gw009-001` (GW009.001) | 3 | First real: `migrations.go` + `00001_create_link_checks.sql` (`go-db-migrations`), `00002_taskbox_v1.sql` (`taskbox-in-go`) | `cmd/migrate` applied versions 1–2 on an empty PostgreSQL 18; `agent/taskbox/check.sh` confirms `00002`'s Up section equals the contract DDL |
