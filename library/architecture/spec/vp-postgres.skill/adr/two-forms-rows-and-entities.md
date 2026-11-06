---
name: two forms, rows and entities
description: Whether every stack reaches PostgreSQL in one form - rows under a service, with one contract and a selector - or a stack with an ORM that tracks aggregates maps the domain's entities and lets the handler take its library's repository
problem: Go and Python serve CLI scripts and small services; the master systems are in dotnet, with aggregates whose children change in one command. One form for all stacks maps every aggregate by hand in dotnet, beside an ORM that tracks it.
decision: Two forms. Go and Python keep rows under a service with the one contract and the selector; dotnet maps the domain's entities with EF Core and its handlers take Ardalis's repository. The behaviour scenarios (conformance/) stay shared; the selector's (conformance/postgres-selector/) are the rows form's.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The rule "Rows below, entities above" made the database an external system behind a service: three models (the API's, the domain's, the row), and a hand mapping per table. In dotnet EF Core already tracks an aggregate and writes what changed, a removed child included; GORM does not, and SQLAlchemy's ORM could, but the services in Python are small.

# Selected variant
[[#Two forms]]
- Chosen by the owner (2026-11-05, 2026-11-06): the database is the service's own infrastructure; dotnet maps its entities and uses Ardalis's repository as it is.

# Searched variants

## Two forms

**Selected.**

### Description
The spec names the rows form and the entities form; a stack extension states its form, and the rules of one form are marked as such. dotnet: [vp-postgres-in-dotnet](library/architecture/dotnet/vp-postgres-in-dotnet.skill/vp-postgres-in-dotnet.skill.md).

### Benefits
- Each stack uses what its ORM does well; dotnet's aggregates need no hand mapping.
- The outcomes, migrations, settings, and readiness stay one behaviour, proved by the same scenarios.

### Costs
- Two forms to describe, and a reader must know which one a stack takes.
- The selector's scenarios run in two stacks of three.

## One form, rows, in every stack

### Description
Every stack keeps rows under a service, with the one contract and the selector.

### Benefits
- One contract and one set of scenarios in every stack.

### Costs
- dotnet maps each aggregate by hand in its service, twice per child collection.

## One form, entities, in every stack

### Description
Go and Python map their entities too.

### Benefits
- One form.

### Costs
- GORM tracks no aggregate, so Go would write the change tracking itself; the small services gain nothing.
