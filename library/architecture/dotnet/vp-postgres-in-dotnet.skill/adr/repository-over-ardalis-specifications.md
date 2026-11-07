---
name: repository over Ardalis specifications
description: How a dotnet handler reaches its entities in PostgreSQL - Ardalis's RepositoryBase over the domain's entities, our repository and selector over rows, our repository over entities, or plain EF Core
problem: The handler picks an entity, calls its methods, and saves it, and the owner wants specifications as classes. What comes from a library, and what is ours?
decision: Ardalis.Specification's IRepositoryBase (IRepository<T> of Shared.Persistence, T : class), implemented by PostgresRepository<T> : RepositoryBase<T>; the specifications are Ardalis's, in the module's Application. Replaces our repository and selector over rows of 2026-11-02.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
EF Core maps the domain's entities ([entities-mapped-by-ef-core](./entities-mapped-by-ef-core.md)) and a handler takes the repository with no service between them. The repository must load and save an entity, take a specification as a class, and leave the translation of failures to [failures-told-by-a-command-middleware](./failures-told-by-a-command-middleware.md). A repository of our own existed for rows, with a selector shared with Go and Python.

# Selected variant
[[#Ardalis's RepositoryBase over the entities]]
- Chosen by the owner (2026-11-06): once entities are mapped and handlers take the repository, the library's contract as it is.
- Before: [[#Our repository and selector over rows]], chosen by the owner on 2026-11-02, when a service stood between the handler and rows.

# Searched variants

## Ardalis's RepositoryBase over the entities

**Selected.**

### Description
`IRepository<T> : IRepositoryBase<T>` and `IReadRepository<T> : IReadRepositoryBase<T>` in the project `Shared.Persistence`, `T : class`. `PostgresRepository<T> : RepositoryBase<T>` implements both, registered open generic. Specifications derive from `Specification<T>` in `{Module}.Application/Specifications`, with their order and page: the domain holds the entities and their rules, the application the questions it asks of the store. No `IAggregateRoot` mark (owner, 2026-11-07): with one entity per table it guards nothing.

### Benefits
- No contract of ours to keep: about 250 lines of repository, selectors, and statements go.
- The form an agent and a dotnet developer know; the repository in memory of the handlers' scenarios (`Postgres.Testing`) implements the same interfaces with Ardalis's in-memory evaluator.

### Costs
- dotnet leaves the selector's shared scenarios (`conformance/postgres-selector/` of `vp-postgres`); the scenarios of the repository and its outcomes stay shared.
- An order and a page live in the specification again, so a count or a delete over one with a page works on other roots than its list: `PostgresRepository` refuses that (`SpecificationPage`).
- The package targets net8 and net9, with no net10 build: it works on EF Core 10, proved by this example's scenarios.

## Our repository and selector over rows

### Description
`PostgresRepository<TRow>` with `InsertAsync`, `UpdateAsync`, `DeleteAsync`, `SelectBy`, and a selector that orders and pages, over Ardalis's specifications, under a service per table.

### Benefits
- The same contract and scenarios as Go and Python.

### Costs
- A row per table and a service that turns rows into entities, so each table is mapped by hand twice.
- A selector stops at the service; with no service its reason is gone.

## Our repository over entities

### Description
The contract of the rows form, `IRepository<TEntity>` with the selector, over mapped entities.

### Benefits
- The shared scenarios of the selector keep running in dotnet.

### Costs
- Our contract to keep beside a library that already has one, for no gain a handler sees.

## Plain EF Core with no repository

### Description
A handler takes `AppDbContext` and queries `Set<T>()`.

### Benefits
- No package and no layer.

### Costs
- Every handler depends on EF Core, no repository in memory can stand in for the database in its scenarios, and a query is not an object to name and reuse.
