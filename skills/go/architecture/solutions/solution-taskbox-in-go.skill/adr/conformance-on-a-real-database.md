---
name: conformance on a real database
description: How the Go TaskBox conformance run gets its PostgreSQL
problem: The conformance feature must run against a real PostgreSQL (SKIP LOCKED, the group lock, and commit order cannot be faked) — how does make unit-test get one?
decision: The runner reads TEST_DATABASE_DSN and fails when it is empty; the developer or CI provides the database (a local server or a CI service container).
tags:
  - solution/taskbox-in-go
  - stack/go
  - concern/testing
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Everything the conformance feature proves on PostgreSQL — row locks, `FOR UPDATE SKIP LOCKED`, commit order under concurrent transactions — exists only in a real server. Until now this catalog's `make unit-test` needed no network. The runner needs a database, and a missing one must not turn into a silent pass.

# Selected variant
[[#DSN from the environment, fail when missing (selected)]]

# Searched variants

## DSN from the environment, fail when missing (selected)

### Description
`TEST_DATABASE_DSN` names a throwaway database; the runner migrates it with the service's own `Migrate` and truncates the TaskBox tables before each scenario. An empty value fails the run with the setting's name.

### Benefits
- No extra dependency; works with any PostgreSQL the developer or CI already has.
- The run also proves the service's migration creates the contract schema.
- A missing database is a red run, never a fake green.

### Costs
- `make unit-test` is no longer self-contained: every developer and every CI job needs a PostgreSQL.

## testcontainers-go

### Description
The test starts a PostgreSQL container itself.

### Benefits
- Self-contained `make unit-test` wherever Docker runs.

### Costs
- Requires a Docker daemon in every environment (not available in this repository's dev container).
- A new, heavy test dependency.

## Skip when no database is configured

### Description
Skip the conformance suite when the DSN is missing.

### Benefits
- `make unit-test` stays green offline.

### Costs
- Violates solution-taskbox's "Never skip a scenario for lack of a store": a CI job without the variable reports conformance it never tested.
