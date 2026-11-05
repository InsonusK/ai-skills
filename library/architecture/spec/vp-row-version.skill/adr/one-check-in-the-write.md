---
name: one check in the write
description: Where the version is compared - in the write itself, or by a middleware that reads the current version before the handler
problem: The old catalog compared versions in a pipeline behavior with one resolver per entity, before the handler, and kept the database's token as a last guard. Is the first check still needed?
decision: Only the write compares: the service puts the caller's version on the row and calls the repository, whose outcome is a changed row.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`ConcurrencyBehavior` of the old catalog needed `IEntityVersionResolverFactory`, a resolver per entity, and a query per version, and the token in the database had to stay, because a write can arrive after the check.

# Selected variant
[[#Only the write compares]]
- Chosen by the agent (2026-11-02); a stale `Update` and `Delete` reported a changed row in the prototype.

# Searched variants

## Only the write compares

**Selected.**

### Description
`UPDATE ... WHERE id = ? AND version = ?`; no row updated is a changed row.

### Benefits
- No gap between the check and the save, one statement, and no class per entity.

### Costs
- A handler runs up to the write before a stale version is found, so what it did before is undone only by a unit of work.

## A check before the handler

### Description
A middleware reads the current versions and compares them before the handler runs.

### Benefits
- A stale call stops before any work; it can compare several rows named by one call.

### Costs
- One more query per version, a resolver per entity, and the check in the write is still needed.
