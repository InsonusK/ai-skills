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
A handler saves an aggregate and goes on: it may send a nested command, or answer from what it saved. A repository that only stages a change gets no outcome: the database answers later, at the end of the command, after the handler has answered as if it were saved. And a unit of work must stay optional: an application without one still needs its writes saved.

# Selected variant
[[#Each repository write saves]]
- Chosen by the owner (2026-10-31).

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
