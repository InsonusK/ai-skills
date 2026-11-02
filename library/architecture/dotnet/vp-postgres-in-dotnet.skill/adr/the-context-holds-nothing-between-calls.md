---
name: the context holds nothing between calls
description: What the DbContext keeps between two calls of a repository - reads tracked for later saves, or nothing at all
problem: A command shares one context across its calls. A row inserted, then read and updated, must not clash in the tracker, a read must see the database, and a failed row must not go out again with the next save. What does the context keep?
decision: Reads of a selector use AsNoTracking and every save empties the tracker in a finally, so a row is a plain object between calls. Only Selector.Update tracks, for the length of its one save.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
EF Core allows one tracked instance per key: a row inserted and then read with no tracking cannot be passed to `Update` in the same context. A tracked read returns the remembered instance, not what another connection wrote. A failed save leaves its rows pending.

# Selected variant
[[#Nothing tracked between calls]]
- Chosen by the agent (2026-11-02).

# Searched variants

## Nothing tracked between calls

**Selected.**

### Description
`PostgresStatements.SaveAsync` calls `ChangeTracker.Clear()` in a `finally`; selectors read with `AsNoTracking`.

### Benefits
- One rule replaces detaching row by row, and a failed row is gone with any failure.
- Every read sees the database, which is what a selector promises.

### Costs
- `Update(row)` writes every column, since nothing remembers which one changed; a row people edit at the same time needs its version.
- A query in a table's own method must not leave tracked rows behind.

## Tracked reads

### Description
Reads track, and `Update(row)` sends only what changed.

### Benefits
- The usual form of EF Core; a smaller `UPDATE`.

### Costs
- A later read answers the remembered row, and a selector stops asking the database.
- A failed save needs a cleanup per kind of failure.
