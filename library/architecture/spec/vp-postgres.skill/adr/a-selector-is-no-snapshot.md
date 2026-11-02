---
name: a selector is no snapshot
description: What a selector guarantees between its calls - nothing by default with a row version where edits meet, a snapshot transaction for every selector, or locks on the picked rows
problem: A count and an update of one selector are two statements, and other writers run between them. Must a selector protect the picked rows, and at what cost?
decision: A selector guarantees nothing between calls and takes no lock; the number of rows written comes from the write. A row people edit at the same time takes a version, opt-in per table. A worker claiming rows is step 4, with a row lock of its own.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Checked on PostgreSQL 17.5: `Count` answered 2, a row arrived, `Update` changed 3, also inside a unit of work at the default isolation. A selector over unclosed tasks counts 0 after closing them. Two saves of one row with no version both succeed and the first edit is lost.

# Selected variant
[[#No guarantee by default, a version where edits meet]]
- Chosen by the agent on the owner's condition that throughput is not lowered by locks (2026-11-02).

# Searched variants

## No guarantee by default, a version where edits meet

**Selected.**

### Description
Each call asks the database again. A table whose rows meet concurrent edits adds the version the engine keeps (`xmin` in PostgreSQL); the second save reports a changed row.

### Benefits
- A selector costs one statement and holds no lock.
- A lost edit is a failure the caller sees, without a lock.

### Costs
- Two calls of a selector may disagree; the service must read the number from the write.
- A conflict is retried or answered as a concurrent change.

## A snapshot transaction for every selector

### Description
Every selector opens a `REPEATABLE READ` transaction, which kept a count at 2 while a row arrived.

### Benefits
- All calls of a selector agree.

### Costs
- A connection is held in a transaction for the selector's life, also through calls of another service.
- A row changed meanwhile fails the write with `40001`, so writers retry where nothing was wrong.

## Row locks on the picked rows

### Description
`SELECT ... FOR UPDATE` before the write. It holds only those rows: another owner's row was updated at once, and a new row of the same owner was inserted.

### Benefits
- The picked rows cannot change until the commit.

### Costs
- Writers of those rows wait, and new rows still arrive, so a count is not protected.
- It needs an open transaction: a lock taken in a unit of work before its first write ended with its statement. Claiming rows is the concern of step 4.

## A lock on the table

### Description
`LOCK TABLE` for the selector's life.

### Benefits
- Every call agrees, and nothing arrives.

### Costs
- Every writer of the table waits; rejected by the owner for its throughput.
