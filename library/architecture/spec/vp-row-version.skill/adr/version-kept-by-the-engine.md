---
name: version kept by the engine
description: What the version of a row is - the engine's own value, a time set by the application, a time set by a trigger, or a counter in the code
problem: A version must move on every write of a row, or a write leaves the check blind. What moves it?
decision: The engine's own value, xmin in PostgreSQL; it needs no column and no code. A time a reader needs is a separate column of vp-edit-timestamp.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Checked on PostgreSQL 17.5 with 300 pairs of saves running at once on one row: with no version both saves succeeded every time. A version must also hold when a row is changed by a path that knows nothing of it.

# Selected variant
[[#The engine's own value]]
- Chosen by the owner (2026-11-02); the old catalog's `solution-entity-concurrency-change` used the same value.

# Searched variants

## The engine's own value

**Selected.**

### Description
`xmin`, a system column PostgreSQL moves on every write.

### Benefits
- No lost edit in 300 races, and a raw `UPDATE` between a read and a save was detected.
- No column, no trigger, and no code to forget.

### Costs
- It says nothing about when the row changed.
- SQLite has no such value, so `vp-sqlite` needs a version of its own.
- 32 bits: a value may repeat after billions of transactions, a risk only inside the time between one read and its save.

## A time set by the application

### Description
`updated_at` set by the code and used as the version.

### Benefits
- One column answers "when" and is the version.

### Costs
- No lost edit in 300 races, but a raw `UPDATE` between a read and a save was not detected: that write was lost silently.

## A time set by a trigger

### Description
`updated_at` set by a trigger on every update.

### Benefits
- A raw `UPDATE` was detected; one column answers "when" too.

### Costs
- A column and a trigger for every table, in every migration.

## A counter in the code

### Description
A number the code increments on each write.

### Benefits
- Works on any engine.

### Costs
- Blind to any write that does not increment it, as a time set by the application is.
