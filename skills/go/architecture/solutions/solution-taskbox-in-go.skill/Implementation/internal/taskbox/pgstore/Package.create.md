---
description: The PostgreSQL realization of the TaskBox contract, schema v1
name: internal/taskbox/pgstore
element_kind: package
change_kind: create
tags:
  - solution/taskbox-in-go
  - element/internal-taskbox-pgstore
---

# Goals
- Store, claim, and finish TaskBox tasks in PostgreSQL exactly as contract §6 states, enqueuing inside the caller's `pgx.Tx`.

# Core Principles
- `pgx/v5` directly — the same driver `solution-persistent-db` uses, so a data write and its task share one `pgx.Tx`; no job-queue library with its own schema (contract, VP-C003 concept).
- Claims rely on `FOR UPDATE SKIP LOCKED` — see [[../../../../glossary/skip-locked.md|glossary/skip-locked]].

# Structure

## Repository place
```
internal/
  taskbox/
    pgstore/
```

## Package Structure
```
internal/taskbox/pgstore/
  store.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| store.go | `Store`: `Enqueue(ctx, tx, task)`, `Claim`, `Finish`, `Requeue`, `Cancel`, `Cleanup`, `Get` | [[./store.go.create.md|store.go]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/jackc/pgx/v5 | >= 5.11 | `pgx.Tx` for enqueue, `pgxpool.Pool` for claims and outcomes |

# Allowed Dependencies
- `internal/taskbox`, `pgx/v5`.

# Check list
- [ ] `pgstore.Store` satisfies `taskbox.Store` (compile-time assertion).
- [ ] The conformance feature passes against it on a real PostgreSQL.
