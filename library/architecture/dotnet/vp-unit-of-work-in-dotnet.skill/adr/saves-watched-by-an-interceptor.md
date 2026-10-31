---
name: saves watched by an interceptor
description: How the transaction of a unit of work opens on the first write with no change to a store - an interceptor of SaveChanges, a call in every store, or a transaction at the start of each command
problem: A store calls SaveChangesAsync and knows nothing of a unit of work, and a command that only reads must hold no transaction. Who opens the transaction?
decision: PostgresUnitOfWork is a SaveChangesInterceptor, scoped with the context; AddPostgresUnitOfWork adds it to the context with ConfigureDbContext, so vp-postgres is not edited.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The variation point must attach with no change to a store, and a transaction must not stay open through a read-only command that calls another service.

# Selected variant
[[#An interceptor of SaveChanges]]
- Chosen by the agent under the owner's decision that a store saves at once (2026-10-31).

# Searched variants

## An interceptor of SaveChanges

**Selected.**

### Description
`SavingChangesAsync` opens the transaction when the unit has begun and none is open.

### Benefits
- No store changes; removing the variation point removes one registration and one list entry.

### Costs
- `ExecuteUpdate`, `ExecuteDelete`, and raw SQL pass no `SaveChanges`, so a store must not use them for a write.

## A call in every store

### Description
Each write of a store asks the unit of work for a transaction first.

### Costs
- Every store changes when the variation point attaches, and a forgotten call writes outside the transaction unnoticed.

## A transaction at the start of the command

### Description
The middleware opens it before the handler runs.

### Benefits
- Covers every statement of the command, also a bulk one.

### Costs
- A read-only command holds a connection in a transaction for as long as it runs, also through its calls of another service.
