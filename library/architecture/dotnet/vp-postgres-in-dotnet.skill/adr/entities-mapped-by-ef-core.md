---
name: entities mapped by EF Core
description: What EF Core maps in dotnet and where the mapping lives - the domain's entities, one per table, configured in their module's Domain; aggregates with owned children; or rows of the infrastructure below a service
problem: The database is the service's own store, not another system's API. Rows below a service add a model per table between the domain and its tables. Where does the mapping live, so that a module adds a table without editing the store's project?
decision: EF Core maps the domain's entities, one per table; each module configures its tables in {Module}.Domain/Configurations (EF Core's relational model API, no provider), and AppDbContext applies each module's configurations, one line per module. A handler takes the repository with no service between. Go and Python keep rows.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The rows form of `vp-postgres` puts a row class per table below a service that builds entities from rows: three models (the API's, the domain's, the row) for one thing, and a hand mapping per table. A first form of the entities (2026-11-06) mapped an aggregate root with owned children and kept every configuration in the Postgres project, because `app-layers-in-dotnet` forbade EF Core in the domain: the example invented a list aggregate whose table held only its id, and every module that added a table edited the store's project.

# Selected variant
[[#Entities, one per table, configured in their module]]
- Chosen by the owner (2026-11-05: the domain's entities mapped; 2026-11-07: one entity per table, the configuration in the module's Domain, after the form of `plateau-offline-sync-service`).

# Searched variants

## Entities, one per table, configured in their module

**Selected.**

### Description
An entity of `{Module}.Domain/Entities` is a row of its table; its `IEntityTypeConfiguration<TEntity>` lives in `{Module}.Domain/Configurations`. `{Module}.Domain` references `Microsoft.EntityFrameworkCore.Relational` (the model API: tables, columns, indexes), no provider. `AppDbContext` in the Postgres project applies its own configurations (a table of no module) and those of each module's assembly, one line per module. A rule over several rows (a title unique in its list) is a unique index, checked by the handler before the write for its message.

### Benefits
- A module owns its tables: a new table edits only the module.
- One model of the domain, as the dotnet teams know it.

### Costs
- The domain references EF Core's model API; the rule "the domain references no EF Core" of `app-layers-in-dotnet` becomes "no provider and no I/O".
- `AppDbContext` is written per application: a line per module.
- dotnet and the other stacks differ in form; only the behaviour scenarios stay shared.

## An aggregate root with owned children, mapped in the Postgres project

### Description
`ItemList` with its `Item`s mapped `OwnsMany`, private fields by `HasField`, every configuration in `Postgres/Configurations/{Module}/`; a repository only for an `IAggregateRoot`.

### Benefits
- A rule over the children lives in the root's method; EF Core saves a changed child with its root.

### Costs
- An entity with no table of its own was invented for the example, and its mapping read as no table the team would write.
- The store's project references every module and holds their mappings.

## Rows below a service

### Description
A row class and its configuration per table in `Postgres.Abstractions`, a service per table that turns rows into entities.

### Benefits
- The same form as Go and Python; the infrastructure sees no domain.

### Costs
- A hand mapping per table in both directions, and a service between every handler and its own store.
