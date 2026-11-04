---
name: statements routed by a GORM plugin
description: How the transaction of a Go unit of work opens on the first write with no change to a repository - a GORM plugin added through Database.Use, a file of the unit of work inside package postgres, an executor every repository method takes from the context, or a transaction at the start of each command
problem: A Go repository runs its statements on the pool through GORM and knows nothing of a unit of work, and a command that only reads must hold no transaction. Who puts a write of a begun unit into its transaction?
decision: A GORM plugin of package unitofwork, added by unitofwork.Attach through postgres.Database.Use. Its callbacks open the transaction before a write of a context that carries a begun unit, behind a savepoint, and run a read there once it is open. Its pool hands GORM's Transaction (a selector's Update) a savepoint of the unit instead of a transaction of its own.
tags:
  - stack/go
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The variation point must attach with no change to a repository. Go has no scope of a command, as dotnet's DI has: what the command's writes share is its `context.Context`. A selector's `Update` already opens a GORM transaction of its own, which a unit must join.

# Selected variant
[[#A GORM plugin added through Database.Use]]
- Chosen by the agent (2026-11-03), under the owner's decisions that a repository saves at once and that the unit opens on the first write (2026-10-31). vp-postgres gained one method, `Database.Use`, in its own pull request.

# Searched variants

## A GORM plugin added through Database.Use

**Selected.**

### Description
`unitofwork.Attach(db)` adds a `gorm.Plugin`, which does two things:
- **Callbacks.** It registers a callback first in Create, Update, Delete, and Raw. When the context carries a begun unit, the callback takes a connection, runs BEGIN on the first write, and sets a SAVEPOINT. It then sets the statement's `ConnPool` to the transaction. A callback last in the same processors releases the savepoint, or rolls back to it on a failure. Query and Row only join a transaction that is already open.
- **The pool.** It replaces the pool with a `ConnPoolBeginner`, so GORM's `Transaction` inside a unit becomes a savepoint.

### Benefits
- No repository and no statement change. Removing the variation point removes one call and one list entry.
- A write that fails behind its savepoint leaves the transaction usable. So does a selector's `Update` that fails halfway: a service that handles a duplicate key goes on.
- The connection is taken with the caller's context, while the transaction lives in a context of its own: a caller who leaves stops the wait, never a commit.

### Costs
- Every write of a unit adds two round trips, its SAVEPOINT and its RELEASE, as EF Core's savepoints do in dotnet.
- The plugin depends on GORM's callbacks and pool interfaces (`ConnPoolBeginner`, `TxCommitter`, `GetDBConnector`). An upgrade of GORM is proved by the scenarios.
- `Raw(...).Scan` goes through Row, so as the first write of a unit it runs outside the transaction. A write through `Statements.Run` uses `Exec` or a write method.

## A file of the unit of work inside package postgres

### Description
`internal/infrastructure/postgres/unit_of_work.go` reads the unexported `*gorm.DB` and `translate` directly.

### Benefits
- No new method in vp-postgres.

### Costs
- Two variation points write one package, which vp-postgres says is never edited. The unit's code depends on unexported names that vp-postgres may rename.

## An executor every repository method takes from the context

### Description
`Statements.session(ctx)` reads a transaction from the context, if there is one.

### Costs
- vp-postgres knows the unit of work, and removing the unit leaves code that reads a context no one fills. The unit still has to open the transaction before the first write, which `session` cannot tell from a read.

## A transaction at the start of the command

### Description
The middleware runs BEGIN before the handler.

### Costs
- A command that only reads holds a connection in a transaction for as long as it runs, also through its calls of another service, which [vp-unit-of-work](library/architecture/spec/vp-unit-of-work.skill/vp-unit-of-work.skill.md) forbids.
