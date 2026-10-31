---
name: a store saves at once
description: When EF Core's changes reach the database - each store method saves, or the unit of work saves everything at the end of the command
problem: EF Core lets a store only stage changes and one SaveChanges send them all at the end of a command, in one short transaction. Should the stores stage, as first agreed, or save at once?
decision: A store calls SaveChangesAsync itself. The unit of work variation point only wraps those saves in one transaction, opened on the first write. Replaces the decision of 2026-10-17, "pipeline behavior + EF change tracking, one short transaction at the end".
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The service over a store turns the store's outcomes into kinds, and a duplicate key is one of them. A store that only stages a row gets no outcome: the database answers later, in the middleware at the end of the command, past the service that knows what the key means. And a unit of work must stay optional: an application without one still needs its writes saved.

# Selected variant
[[#Each store saves]]
- Chosen by the owner (2026-10-31).

# Searched variants

## Each store saves

**Selected.**

### Description
`PostgresListStore.InsertAsync` calls `SaveChangesAsync`. Without a unit of work each save is its own transaction; with one, all the saves of a command run in its transaction, which EF Core protects with a savepoint per save.

### Benefits
- The service catches `DuplicateKeyException` in both cases; attaching the unit of work changes no store.
- A command reads its own writes. Go and Python behave the same.

### Costs
- With a unit of work the transaction stays open from the first write to the end of the command, so calls of another service belong before the first write.
- `ExecuteUpdate`, `ExecuteDelete`, and raw SQL do not pass through `SaveChanges`.

## The unit of work saves at the end

### Description
A store adds to the change tracker; `UnitOfWorkMiddleware` calls `SaveChangesAsync` once at the end of the outermost command.

### Benefits
- The shortest transaction; the usual form of EF Core.

### Costs
- A service must ask the database first and the middleware must answer the race with a text of its own.
- A command does not see its own writes.
- The stores work only with the middleware attached, so the unit of work stops being optional.
