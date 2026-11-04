---
name: repository over SQLAlchemy
description: How the repository contract of vp-postgres is built in Python - our repository and selectors over SQLAlchemy 2's async ORM on asyncpg, the asyncpg driver with a mapping of our own, or SQLModel
problem: The contract of vp-postgres needs one generic repository over any row, specifications as classes that hold conditions and joins, a refusal of a specification holding an order or a page, and the engine's failures by their codes. Which library maps the rows and builds the statements in Python?
decision: SQLAlchemy 2's async ORM on asyncpg maps the rows and builds the statements. A specification is a class with pick(query) -> query. Repository, Projections, the selectors, and the translation of failures are ours, over the SQLSTATE asyncpg reports.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Python has no library like Ardalis.Specification. A generic `Repository[TRow]` must write any row. A specification must hold conditions and joins and nothing else, and `select_by` must see that it holds no order and no page. A selector's `update` and `delete` must work on the rows a specification with a join picks.

# Selected variant
[[#Our repository over SQLAlchemy]]
- Chosen by the owner (2026-11-03): SQLAlchemy 2 async with asyncpg, and Alembic for the migrations ([alembic-migrations-checked-against-the-rows](./alembic-migrations-checked-against-the-rows.md)).

# Searched variants

## Our repository over SQLAlchemy

**Selected.**

### Description
- **Rows and specifications.** A row is a class of the application's `DeclarativeBase`. A specification is a class of the application with `pick(query: Select[TRow]) -> Select[TRow]`.
- **Our code.** `Repository[TRow]`, `Projections[TRow, TResult]`, the selectors, and `translate` are ours.
- **Writes.** Each call runs in an `AsyncSession` of its own, committed when it returns. `insert`, `update`, and `delete` of one row are Core statements by the row's key, so a relation of the row is never written with it.

### Benefits
- The most used ORM of Python, with typed `Mapped` columns that mypy reads. A specification of another table fails the type check at `select_by`.
- The refusal reads the order and the limit from the select a specification built, which runs nothing.
- `Database.session` runs a call on a connection a router gives, behind a savepoint (`join_transaction_mode="create_savepoint"`). That is how the unit of work joins with no edit of a repository.

### Costs
- The refusal reads private fields of `Select` (`_order_by_clauses`, `_limit_clause`); an upgrade of SQLAlchemy is proved by the scenarios.
- `asyncpg` reads only the URL form of a DSN; Go's key=value form is refused at start.
- The pool lives on the event loop that opened it: a test runs every step of a scenario on one loop.

## The asyncpg driver with a mapping of our own

### Description
SQL text, and a mapping of rows to classes written by us.

### Costs
- A builder of conditions and joins of our own, copied into every project and kept by us.

## SQLModel

### Description
SQLAlchemy with Pydantic models as rows.

### Costs
- A second layer over SQLAlchemy with its own releases. Its async support lags SQLAlchemy's, and the rows carry validation the service does already.
