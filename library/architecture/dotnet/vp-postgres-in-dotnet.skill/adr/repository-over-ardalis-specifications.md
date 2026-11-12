---
name: repository over Ardalis specifications
description: How the repository contract of vp-postgres is built in dotnet - our repository and selectors over Ardalis specifications, Ardalis's RepositoryBase, a specification class of our own, or plain EF Core with no specification
problem: The contract of vp-postgres needs a selector, translated failures, and writes that join a unit of work, and the owner wants specifications as classes. What comes from a library, and what is ours?
decision: Specifications are Ardalis.Specification's, evaluated by its SpecificationEvaluator; PostgresRepository, the selectors, and PostgresStatements are ours, over EF Core. Replaces the hand-written store per table of 2026-10-31.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`{Infra}` may not reference `{Module}.Domain` (app-layers-in-dotnet), so a repository works on rows of its own. The first form, a hand-written store per table, was chosen by the agent with no owner's decision and compared Ardalis only in the old catalog's form, handlers over `IRepository<Entity>`; that form breaks the layers, but Ardalis behind the service does not. The prototypes of 2026-11-01 showed that a class translating failures is needed with Ardalis and without it.

# Selected variant
[[#Our repository over Ardalis specifications]]
- Chosen by the owner (2026-11-02): specifications as classes, as in Ardalis, a repository with CRUD, and a selector. Ardalis stays for now; whether a dotnet class of about ten lines replaces it is reviewed after `vp-postgres` exists in Go and Python, when the shape of the contract is proved there.

# Searched variants

## Our repository over Ardalis specifications

**Selected.**

### Description
`Specification<TRow>` from `Ardalis.Specification` in `Postgres.Abstractions`, a package with no EF Core. `PostgresRepository<TRow>` turns one into a query with `SpecificationEvaluator` and runs it through `PostgresStatements`.

### Benefits
- The specification classes the owner knows, an in-memory evaluation for a service's scenarios, and no evaluator to write.
- One place translates every failure, so a read needs no override.
- The contract is the same as in the stacks with no such library.

### Costs
- A package that targets net8 and net9, with no net10 build: it works on EF Core 10, proved by the scenarios of this example.
- Only `Where`, `Include`, and `Select` of its builder are used: [order-and-page-on-the-selector](library/architecture/spec/vp-postgres.skill/adr/order-and-page-on-the-selector.md).
- About 130 lines of selectors and repository are ours to keep.

## Ardalis's RepositoryBase

### Description
`PostgresRepository<TRow> : RepositoryBase<TRow>`, the service on `IRepositoryBase<TRow>`.

### Benefits
- The least code; the form known from the old catalog.

### Costs
- It translates no failure: every read must be overridden for that, and the same class of catches is written anyway.
- No update by a specification, no selector, and `CountAsync(spec)` beside the contract of the other stacks.

## A specification class of our own

### Description
`abstract class Specification<TRow> { abstract IQueryable<TRow> Apply(IQueryable<TRow> rows); }`, in about ten lines.

### Benefits
- No package, and no build for an older framework.

### Costs
- The evaluation in memory and the `Include` handling are ours; the owner asked for what Ardalis offers, and the same work would be repeated.

## Plain EF Core with no specification

### Description
A store per table derives from a base class that translates failures; reusable conditions are extension methods on `IQueryable`.

### Benefits
- No package; the plainest EF Core.

### Costs
- A query is not an object to pass, so a count and an update of the same rows stay two methods of a store.

## A generic repository for handlers over entities

### Description
`IRepository<T>` over entities in `Shared`, used by handlers, as in the old catalog.

### Benefits
- No service over the repository.

### Costs
- Handlers reach infrastructure past the services, against app-layers, and `Postgres` would see the domain.
