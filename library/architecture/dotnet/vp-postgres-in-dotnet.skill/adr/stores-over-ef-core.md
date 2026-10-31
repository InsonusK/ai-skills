---
name: stores over EF Core
description: What a store is in dotnet - a hand-written class per table over EF Core and rows of its own, a generic repository with specifications, or the domain's entities mapped directly
problem: The old catalog gives handlers a generic IRepository<T> with Ardalis specifications over entities mapped by EF Core. The new layers keep the domain out of infrastructure and let handlers reach only services. What does a store look like?
decision: A hand-written store per table in src/Infrastructure/Postgres/Postgres/Stores, over one AppDbContext and row classes of its own, behind an interface in Postgres.Abstractions.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`{Infra}` may not reference `{Module}.Domain` (app-layers-in-dotnet), and every service speaks the application's language over one infrastructure interface in the technique's terms.

# Selected variant
[[#A store per table, with rows]]
- Chosen by the agent under the layer rules (2026-10-31); the agreed choice of EF Core stands (2026-10-17).

# Searched variants

## A store per table, with rows

**Selected.**

### Description
`IListStore` with `InsertAsync` and `SelectAsync`, `PostgresListStore` over `AppDbContext`, `ItemRow` with its `IEntityTypeConfiguration`.

### Benefits
- The compiler keeps the domain out; an entity's rules run on what is read.
- The interface is the one of the in-memory store, so the service keeps its methods.

### Costs
- A mapping from rows to entities in every service.

## A generic repository with specifications

### Description
`IRepository<T>` over Ardalis.Specification in `Shared`, used by handlers.

### Benefits
- No store code per table.

### Costs
- Handlers would reach infrastructure past the services, against app-layers.
- A query is written in the module, so the module depends on the technique.

## Entities mapped directly

### Description
EF Core configurations for the domain's entities.

### Benefits
- No row classes.

### Costs
- `Postgres` would reference the domain, or the domain EF Core.
