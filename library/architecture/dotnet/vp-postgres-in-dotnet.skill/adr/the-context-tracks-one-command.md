---
name: the context tracks one command
description: What the DbContext keeps between two calls of a repository in one command - the entities it read, tracked until the command ends, or nothing at all
problem: A command shares one context across its calls. A handler picks an entity, changes it, and saves it; a failed save must not go out again with the next one.
decision: Reads track, so EF Core writes only what changed in a picked entity; the context lives one command (ScopedSender). A failed save clears the tracker. Replaces "nothing tracked between calls" of 2026-11-02, made for rows.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
EF Core writes the difference between what it read and what the entity holds at the save. An entity read with no tracking and saved with `Update` is written whole, every column, and a related entity it holds is marked changed too. A failed save leaves its entries pending, and the next save of the command sends them again.

# Selected variant
[[#Tracked for one command, cleared on a failed save]]
- Chosen by the agent (2026-11-06), with the move to mapped entities ([entities-mapped-by-ef-core](./entities-mapped-by-ef-core.md)).
- Before: [[#Nothing tracked between calls]], chosen by the agent on 2026-11-02 for rows.

# Searched variants

## Tracked for one command, cleared on a failed save

**Selected.**

### Description
Ardalis's reads track by default; `PostgresRepository.SaveChangesAsync` calls `ChangeTracker.Clear()` when the save fails. [one-scope-per-command](./one-scope-per-command.md) ends the context with the command.

### Benefits
- A picked entity saves only what changed.
- A failed root is gone with any failure, so the shared scenario "A failed write leaves nothing pending" holds.

### Costs
- A second read of the same root in one command answers the tracked object, with the command's changes, not what another connection wrote meanwhile; [vp-row-version](library/architecture/spec/vp-row-version.skill/vp-row-version.skill.md) tells the save that it was stale.
- A handler that only reads takes `IReadRepository<T>`; a specification asks `AsNoTracking()` for a large read.

## Nothing tracked between calls

### Description
Reads use `AsNoTracking`, and every save empties the tracker in a `finally`.

### Benefits
- Every read sees the database; a row is a plain object between calls.

### Costs
- `Update` writes every column, and every related entity it reaches.
