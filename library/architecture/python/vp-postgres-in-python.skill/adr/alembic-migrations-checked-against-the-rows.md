---
name: alembic migrations checked against the rows
description: How the schema of a Python application with vp-postgres is migrated - Alembic with migrations generated from the rows and checked by alembic check, or SQL files applied by a runner of our own
problem: vp-postgres asks for every schema change as a migration in the repository, applied by a one-shot job or behind MIGRATE_ON_START. What writes and checks the migrations in Python?
decision: Alembic. make migration generates a file from the changed rows, the developer reads it, postgres.migrate applies it through the job todo-migrate and MIGRATE_ON_START. A scenario runs alembic check against a migrated database, so a row no migration matches fails make test.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The rows are SQLAlchemy classes, so the schema is written twice: in the rows and in the migrations. Without a check, a field added to a row with no migration fails only when a statement first reads it.

# Selected variant
[[#Alembic with a check of the rows]]
- Chosen by the owner (2026-11-03).

# Searched variants

## Alembic with a check of the rows

**Selected.**

### Description
- **Files.** `migrations/versions/{revision}_{what}.py` holds `upgrade` and `downgrade`. `env.py` is copied and reads the rows' metadata.
- **Applying.** `postgres.migrate` applies the files on a connection of the application's engine and records them in `alembic_version`.
- **The check.** The scenario of `model_changes.feature` runs `alembic check` on a migrated database.

### Benefits
- Generated from the rows, as in dotnet, and one Python package, no external binary.
- The job exits 0 on a database already up to date, proved by a scenario.
- `alembic check` compares the columns, their types and nullability, the keys, and the indexes.

### Costs
- `make migration` compares the rows with a database at the latest migration: it needs `DATABASE_DSN`, where `dotnet ef` needs none.
- A generated file is read before it is committed: Alembic writes a rename as a drop and an add.

## SQL files applied by a runner of our own

### Description
`0001_{what}.sql` files and a table of applied versions, as goose does in Go.

### Costs
- A runner of our own to copy and keep, and no generation from the rows.
