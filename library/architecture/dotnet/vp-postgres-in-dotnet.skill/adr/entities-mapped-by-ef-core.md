---
name: entities mapped by EF Core
description: What EF Core maps in dotnet - the domain's entities and aggregates, or rows of the infrastructure below a service
problem: The database is the service's own store, not another system's API. Rows below a service add a model per table between the domain and its tables, and an aggregate with children is mapped by hand in both directions.
decision: EF Core maps the domain's entities; an aggregate's root with its children, the children owned by the root. The mapping lives in the Postgres project, which references each module's Domain; a handler takes the repository with no service between. Go and Python keep rows.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The rows form of `vp-postgres` puts a row class per table below a service that builds entities from rows. In dotnet the master systems live, with aggregates whose children change in one command; mapping them by hand costs more than EF Core's change tracking, and three models (the API's, the domain's, the row) describe one thing. A cache of another system's data or an outbox has no entity of the domain at all.

# Selected variant
[[#The domain's entities mapped]]
- Chosen by the owner (2026-11-05): the database is the service's own infrastructure; API and Client stay boundaries with models of their own.

# Searched variants

## The domain's entities mapped

**Selected.**

### Description
`IEntityTypeConfiguration<TEntity>` per aggregate in `Postgres/Configurations/{Module}/`, the children of a root mapped `OwnsMany`, private fields by `HasField`. `Postgres` references every `{Module}.Domain`; the domain references no package of EF Core. A technical table (a cache, an outbox) maps a class of the infrastructure in the same `AppDbContext`.

### Benefits
- One model of the domain; EF Core writes what changed in a loaded aggregate, a removed child included.
- Attaching the database changes no handler: the in-memory base and the database implement one contract.

### Costs
- `app-layers-in-dotnet` lets `Postgres` reference the domains it maps.
- An entity keeps what EF Core needs to build it: a constructor whose parameters name properties, a private setter for a key.
- dotnet and the other stacks differ in form; only the behaviour scenarios stay shared.

## Rows below a service

### Description
A row class and its configuration per table in `Postgres.Abstractions`, a service per table that turns rows into entities.

### Benefits
- The same form as Go and Python; the infrastructure sees no domain.

### Costs
- A hand mapping per table in both directions, and a second one for each child collection.
- A service between every handler and its own store.
