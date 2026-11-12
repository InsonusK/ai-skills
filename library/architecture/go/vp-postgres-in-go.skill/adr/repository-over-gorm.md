---
name: repository over GORM
description: How the repository contract of vp-postgres is built in Go - our repository and selectors over GORM, pgx with a mapping of our own, or bun
problem: The contract of vp-postgres needs one generic repository over any row, specifications as types that hold conditions and joins, a refusal of a specification holding an order or a page, and the engine's failures by their codes. Which library maps the rows and builds the statements in Go?
decision: GORM maps the rows and builds the statements; a specification is a type with Pick(*gorm.DB) that embeds SpecificationOf[TRow]; Repository, the selectors, and the translation of failures are ours, over pgx's error codes.
tags:
  - stack/go
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go has no library like Ardalis.Specification. A generic `Repository[TRow]` must insert, update, and delete any row, so something maps a struct to its columns; a specification must hold conditions and joins and nothing else, and `SelectBy` must see that it holds no order and no page; a selector's `Update` and `Delete` must work on the rows a specification with a join picks.

# Selected variant
[[#Our repository over GORM]]
- Chosen by the owner (2026-11-03), after a prototype on PostgreSQL 17.5: `SelectBy` read `ORDER BY` and `LIMIT` from the statement a specification built and refused it; `Count`, `First`, and a projection with a join worked; `Update` and `Delete` ran as one statement on `id IN (the specification's subquery)`, with a join.

# Searched variants

## Our repository over GORM

**Selected.**

### Description
A row is a struct GORM maps by its tags and `TableName`. A specification is a type of the application with `Pick(query *gorm.DB) *gorm.DB`; `Repository[TRow]`, `Projections[TRow, TResult]`, the selectors, and `translate` are ours, about 600 lines with comments. GORM's own translation of errors is off; ours reads pgx's codes.

### Benefits
- The most used ORM of Go, the closest to EF Core: `Joins`, `Preload` (Include), `Select` into a struct of the result.
- The refusal reads `Statement.Clauses`, built by a statement that never runs (`DryRun`).
- A type that embeds `SpecificationOf[TRow]` cannot be handed to another table's repository: the compiler stops it.

### Costs
- Conditions are SQL fragments with placeholders, not expressions the compiler checks; a wrong column fails at run time, in the scenarios.
- `Joins("Relation")` selects the related columns too, which breaks the subquery of `Update` and `Delete`; a join is written as `Joins("JOIN owners ON ...")`, a relation is loaded by `Preload`.
- `Save` inserts a row it does not find, so `Update` uses `Select("*").Updates` and reports no row as `ErrRowChanged`.

## pgx with a mapping of our own

### Description
Only the driver: a mapping of struct to columns and a builder of conditions, written by us.

### Benefits
- No ORM; every statement is visible.

### Costs
- Several hundred lines more of fixed code, a mapping and a builder, copied into every project and kept by us.

## bun

### Description
A query builder with relations and migrations of its own.

### Benefits
- Close to SQL, with a typed builder.

### Costs
- The state of a query is unexported: the refusal of an order or a page would parse the text of the SQL.
