---
name: test database from the project
description: Where the scenarios of a store get their PostgreSQL - the project gives it by a DSN or its dev container, a package starts one, or a container per test run
problem: A store is tested on the real engine (app-layers), and the example must stay runnable in any project and in a dev container with no Docker; what starts that engine?
decision: The project provides the server - TEST_DATABASE_DSN, or a PostgreSQL run in its dev container; the scenarios create and drop a database of their own on it and fail without one. The examples carry no way of starting a server.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Scenarios on a store need a real PostgreSQL. The ways to start one differ per stack and per machine, and this repository's dev container has neither Docker nor PostgreSQL.

# Selected variant
[[#A server the project provides]]
- Chosen by the owner (2026-10-31).

# Searched variants

## A server the project provides

**Selected.**

### Description
`TEST_DATABASE_DSN` names a server the scenarios may create databases on; a project's CI passes its own, and a project's dev container may run one. The example's `make test` stops with a message when it is not set.

### Benefits
- One mechanism for every stack.
- CI uses the server it already has; nothing in the example downloads anything.

### Costs
- A project must arrange the server before its first test run. In this repository, whose dev container has none, the examples are verified against prebuilt PostgreSQL binaries started by hand.

## An embedded package

### Description
A package that downloads and starts PostgreSQL from the test process: `MysticMind.PostgresEmbed` in dotnet, `embedded-postgres` in Go.

### Benefits
- `dotnet test` alone is enough.

### Costs
- A different mechanism per stack; for Python 3.13 there is no such package.

## Testcontainers

### Description
A PostgreSQL container started per test run.

### Benefits
- The common choice where Docker is available.

### Costs
- Needs Docker, so the scenarios could not run in this repository's dev container, and every example must have green tests.
