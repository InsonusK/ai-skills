---
name: service scenarios on the real engine
description: How the scenarios of a Go service over a repository get their rows - the real engine with a repository that fails as asked, or an in-memory repository evaluating the specifications
problem: The scenarios of a service over a repository need rows read back and the outcomes of the repository. dotnet evaluates Ardalis specifications in memory; a GORM specification is SQL. What does a Go service's scenario run on?
decision: On a migrated database of its own, as the repository's scenarios do; an outcome the scenario cannot cause on the engine comes from postgrestest.Failing, a repository answering a chosen failure.
tags:
  - stack/go
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`Pick` builds SQL, so no code can say in memory which rows a specification picks. A fake that ignores the specification answers rows of another list and lets a wrong condition pass.

# Selected variant
[[#The real engine and a failing repository]]
- Chosen by the agent (2026-11-03): the scenarios of the database already need `TEST_DATABASE_DSN`, so the service's scenarios cost nothing more and test the specification too.

# Searched variants

## The real engine and a failing repository

**Selected.**

### Description
The service's scenarios create a database with `postgrestest.Create`, migrate it, and build the service over `postgres.NewRepository`; "the store answers a duplicate key" builds it over `postgrestest.Failing{Err: ...}`.

### Benefits
- The specification the service uses runs on the engine in the service's own scenarios.
- One fake of about 50 lines, the same for every table.

### Costs
- The service's scenarios fail without `TEST_DATABASE_DSN`, as the repository's do.

## An in-memory repository evaluating the specifications

### Description
Each specification has a second method, a condition on a row in Go, which a fake evaluates.

### Benefits
- The service's scenarios need no database.

### Costs
- Every condition is written twice, and the two copies drift with no scenario to show it.
