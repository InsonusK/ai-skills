---
name: taskbox package outside infrastructure
description: Where the Go TaskBox code lives so that data adapters can enqueue inside their own transactions
problem: A data adapter under internal/infrastructure must call TaskBox's Enqueue inside its transaction, but this catalog forbids one internal/infrastructure package importing another — where does TaskBox live?
decision: TaskBox lives in internal/taskbox (the store-independent mechanism) with one subpackage per store (internal/taskbox/pgstore, …); it is a shared mechanism that adapters import, not an adapter itself.
tags:
  - solution/taskbox-in-go
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Go catalog's package rule says an `internal/infrastructure/*` adapter never imports a sibling adapter, so two technologies stay independently replaceable. TaskBox must be called by the data adapter (`internal/infrastructure/{store}`) inside that adapter's `pgx.Tx` ([[skills/common-workflow/architecture/solutions/solution-taskbox.skill/adr/enqueue-through-the-data-port.md|enqueue through the data port]]). If TaskBox were `internal/infrastructure/taskbox`, the data adapter would break the rule.

# Selected variant
[[#internal/taskbox with a subpackage per store (selected)]]

# Searched variants

## internal/taskbox with a subpackage per store (selected)

### Description
`internal/taskbox` holds task values, the handler registry, classification, and the worker; `internal/taskbox/pgstore` (and later `redisstore`) implements `taskbox.Store` for one store. Adapters import `taskbox` and the store subpackage; TaskBox imports nothing of the service.

### Benefits
- The sibling-import rule keeps its meaning: TaskBox is a mechanism like a library, not a replaceable adapter.
- A binary links only the drivers of the stores it uses.
- The package can be copied between services unchanged.

### Costs
- A new top-level `internal/` directory whose role a reader must learn: "shared mechanism".

## internal/infrastructure/taskbox, with an exception to the rule

### Description
Keep TaskBox beside the other adapters and allow data adapters to import it.

### Benefits
- No new directory kind.

### Costs
- The rule gains an exception every later mechanism (Outbox, Inbox) would cite.

## TaskBox behind a domain port

### Description
Declare a `TaskQueue` port in `internal/domain/interfaces` and implement it in infrastructure.

### Benefits
- Fits the existing ports-and-adapters shape.

### Costs
- A port method cannot take the adapter's `pgx.Tx`, so the enqueue cannot join the data change's transaction — the property TaskBox exists for.
