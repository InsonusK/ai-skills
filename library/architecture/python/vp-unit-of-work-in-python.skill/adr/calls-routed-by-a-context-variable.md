---
name: calls routed by a context variable
description: How the transaction of a Python unit of work opens on the first write with no change to a repository - a router of connections added through Database.use, reading the unit from a context variable; SQLAlchemy events; or a session passed down by every call
problem: A Python repository runs each call in a session of its own and knows nothing of a unit of work, and a command that only reads must hold no transaction. Who puts a write of a begun unit into its transaction?
decision: The unit lives in a ContextVar, set by UnitOfWork.begin in the middleware of the outermost command. UnitOfWork is the database's ConnectionRouter, added by attach through Database.use, which vp-postgres gained for it. A write of a context with a begun unit gets the unit's connection, opening its transaction; a read gets it once it is open. Every call runs there behind a savepoint of its own.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The variation point must attach with no change to a repository. Python has no scope of a command as dotnet's DI has, and no context argument as Go's: what the calls of one command share is the asyncio task they run in, and its context.

# Selected variant
[[#A router of connections reading a context variable]]
- Chosen by the agent (2026-11-03), under the owner's decisions that a repository saves at once and that the unit opens on the first write (2026-10-31).

# Searched variants

## A router of connections reading a context variable

**Selected.**

### Description
- **The seam.** `Database.session(write)` asks its router for a connection. On one, it runs the call's session with `join_transaction_mode="create_savepoint"`.
- **The router.** `UnitOfWork.connection_for(write)` reads the unit of the current context. For a write it opens the unit's connection and BEGIN; for a read it answers the connection once it is open.
- **The end.** `commit` and `rollback` run under `asyncio.shield` and `END_TIMEOUT`.
- **After the end.** An ended unit stays in its context: a write there is refused, and the next command there begins a unit of its own.

### Benefits
- No repository changes; removing the variation point removes one call and one list entry.
- Each call has a savepoint, so a duplicate key a service handles, or a selector's update that fails halfway, leaves the transaction usable.
- A task a handler starts copies the context and joins the unit. Once the command ends, its write is refused instead of running outside any transaction.

### Costs
- A read inside the open transaction sets a savepoint too: two more round trips per call.
- The calls of one unit share one connection, so a handler that gathers nested commands concurrently breaks the unit.
- A test runs the steps of a scenario in one context (`loop.create_task(..., context=...)`), or a unit begun by one step is not the unit of the next.

## SQLAlchemy events

### Description
Listeners on the session, `do_orm_execute` and `before_flush`, rebind a session to the unit's connection.

### Costs
- By the first flush, a session may already hold a connection of the pool from a read; the rebinding rests on the order of SQLAlchemy's internals.

## A session passed down by every call

### Description
A handler creates a session, and every service and repository takes it as a parameter.

### Costs
- Every service and repository changes to attach the variation point, which [vp-unit-of-work](library/architecture/spec/vp-unit-of-work.skill/vp-unit-of-work.skill.md) forbids.
