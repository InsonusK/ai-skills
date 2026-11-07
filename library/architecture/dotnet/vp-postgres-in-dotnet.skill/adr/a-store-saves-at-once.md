---
name: a store saves at once
description: When EF Core's changes reach the database - each repository write saves, or the unit of work saves everything at the end of the command
problem: EF Core lets a repository only stage changes and one SaveChanges send them all at the end of a command, in one short transaction. Should writes stage, as first agreed, or save at once?
decision: A repository calls SaveChangesAsync itself. The unit of work variation point only wraps those saves in one transaction, opened on the first write. Replaces the decision of 2026-10-17, "pipeline behavior + EF change tracking, one short transaction at the end".
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A handler saves an entity and goes on: it may send a nested command, or answer from what it saved. A repository that only stages a change gets no outcome: the database answers later, at the end of the command, after the handler has answered as if it were saved. And a unit of work must stay optional: an application without one still needs its writes saved.

# Selected variant
[[#Each repository write saves]]
- Chosen by the owner (2026-10-31), confirmed on 2026-11-07 after comparing it with the form of `plateau-offline-sync-service` (one SaveChanges at the end): [[#Comparison of the two]].

# Comparison of the two
Both commit once, at the end of the outermost command. They differ in when the SQL reaches the database and how long the transaction lives.

| | Each write saves (selected) | The unit of work saves at the end |
|---|---|---|
| `AddAsync` / `UpdateAsync` | calls `SaveChanges`: the `INSERT` or `UPDATE` is sent at once, inside the transaction the first save opened | marks the change in EF Core's tracker; no SQL is sent |
| The transaction | opened by the first write, open until the end of the command | opened by the one `SaveChanges` at the end, closed by it |
| The commit | at the end of the command, by `UnitOfWorkMiddleware` | at the end of the command, by that `SaveChanges` |
| What the handler sees of a write | at once: the generated id, a duplicate key | nothing until the command ends: the id is 0, a failure arrives past the handler |
| A nested command that reads | sees the writes of its parent | does not see them: they are still in the tracker |
| With no unit of work attached | every write saves on its own | nothing is saved: the unit of work is mandatory |
| A statement past `SaveChanges` (`ExecuteUpdate`, raw SQL) | runs in the open transaction through `WriteAsync` | runs at once, before the tracked changes, outside their transaction |
| Locks and connection | the transaction holds its row locks and its connection from the first write: a call of another API belongs before the first write | the shortest transaction; the connection is taken only at the end |
| Ardalis's `RepositoryBase` | as it is: it calls `SaveChangesAsync` in every write | must override every write so it does not save (the plateau's example claims so, but its `Repository<T>` overrides nothing and saves at once) |

# Searched variants

## Each repository write saves

**Selected.**

### Description
Every write of Ardalis's `RepositoryBase` (`AddAsync`, `UpdateAsync`, `DeleteAsync`, and their ranges) calls `SaveChangesAsync`, which `PostgresRepository` keeps. Without a unit of work each save is its own transaction; with one, all the saves of a command run in its transaction, which EF Core protects with a savepoint per save.

### Benefits
- A failure reaches the handler at the write that caused it, and the store failures middleware tells it, with a unit of work or without; attaching the unit of work changes no repository.
- A command reads its own writes. Go and Python behave the same.

### Costs
- With a unit of work the transaction stays open from the first write to the end of the command, so calls of another service belong before the first write.
- `ExecuteUpdate`, `ExecuteDelete`, and raw SQL do not pass through `SaveChanges`, so they run in `PostgresRepository.WriteAsync`.

## The unit of work saves at the end

### Description
A repository adds to the change tracker; `UnitOfWorkMiddleware` calls `SaveChangesAsync` once at the end of the outermost command.

### Benefits
- The shortest transaction; the usual form of EF Core.

### Costs
- A handler answers before the database has, and a failure arrives past it.
- A command does not see its own writes.
- A repository works only with the middleware attached, so the unit of work stops being optional.
