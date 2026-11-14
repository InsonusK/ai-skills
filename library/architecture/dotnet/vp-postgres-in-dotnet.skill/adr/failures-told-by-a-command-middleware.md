---
name: failures told by a command middleware
description: Where a failure of PostgreSQL becomes an error kind in dotnet - one command middleware, a SaveChanges interceptor, the repository, or the EntityFramework.Exceptions package
problem: With no service between a handler and its repository, nothing turns a taken key into a conflict, a changed row into a concurrent change, or a lost connection into unavailable. The translation must cover reads, saves, and statements past SaveChanges, and no handler may read an engine's code.
decision: StoreFailuresMiddleware, listed after the logging and before the unit of work, asks IStoreFailures of Shared; PostgresFailures reads the SQLSTATE over the chain of causes. No EntityFramework.Exceptions.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A failure reaches a handler from a query (a lost connection inside an `InvalidOperationException`), from a save (inside a `DbUpdateException`), and from a statement past SaveChanges (the driver's error alone). Every one must become the kind of [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md#report-a-repositorys-outcomes-in-its-own-terms), the failure kept as the cause the log writes and the caller never sees.

# Selected variant
[[#One command middleware]]
- Chosen by the owner (2026-11-05): one middleware, so no failure is missed.
- Without `EntityFramework.Exceptions`: chosen by the agent (2026-11-06), for the owner's review.

# Searched variants

## One command middleware

**Selected.**

### Description
`StoreFailuresMiddleware<,>` in `Middleware/StoreFailures` catches what a command threw and asks `IStoreFailures.KindOf`; `PostgresFailures` of the Postgres project answers `Conflict` (23505), `NotFound` (23503), `ConcurrentChange` (`DbUpdateConcurrencyException`), `Unavailable` (`NpgsqlException.IsTransient`), or nothing. A handler that must tell a rule apart says it before the write (`AddItemHandler` asks `AnyAsync` first); the index stays the guard under a race.

### Benefits
- Every path of every command passes it, a nested command's and a failed commit's too.
- The same pattern as the unit of work: an interface in Shared, a middleware in Middleware, the implementation in the store's project.

### Costs
- A message names the entity of the failed save ("item already exists"), not the rule: a handler wanting the rule's words checks first.
- Code outside a command (a hosted service, a notification handler) gets no translation unless it sends a command.

## A SaveChanges interceptor

### Description
`SaveChangesFailedAsync` throws the translated error.

### Benefits
- Every save, whatever its caller.

### Costs
- No query and no statement past SaveChanges: a lost connection on a read reaches the caller untranslated.

## The repository

### Description
`PostgresRepository` overrides every method of `RepositoryBase` with a `catch`.

### Benefits
- The outcome is known at the call.

### Costs
- Twenty overrides, and a statement of a derived repository can still pass untranslated.

## EntityFramework.Exceptions under the middleware

### Description
`UseExceptionProcessor()` turns a failed save into `UniqueConstraintException`, `ReferenceConstraintException`, and others; the middleware reads those types.

### Benefits
- Typed exceptions for the constraints, without a SQLSTATE in our code.

### Costs
- It covers SaveChanges only, so the reading by SQLSTATE stays for a query and a statement past SaveChanges: two ways to read one failure, and one package more.
