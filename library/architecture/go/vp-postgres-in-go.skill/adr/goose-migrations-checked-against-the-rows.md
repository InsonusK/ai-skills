---
name: goose migrations checked against the rows
description: How the schema of a Go application with vp-postgres is migrated - goose with SQL files and a scenario comparing them with the rows, atlas generating migrations from the GORM rows, or GORM's AutoMigrate
problem: vp-postgres asks for every schema change as a migration in the repository, applied by a one-shot job or behind MIGRATE_ON_START. In dotnet `dotnet ef` generates them from the model and a scenario catches a model with no migration. What writes and checks the migrations in Go?
decision: goose applies SQL files embedded in the binary, through cmd/migrate and MIGRATE_ON_START; a developer writes each file, and a scenario compares the schema they make with the schema GORM would make of the rows.
tags:
  - stack/go
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The rows are GORM structs, so the schema is written twice: in the rows and in the migrations. Without a check, a field added to a row with no migration fails only when a statement first reads it.

# Selected variant
[[#goose with SQL files and a check against the rows]]
- Chosen by the owner (2026-11-03).

# Searched variants

## goose with SQL files and a check against the rows

**Selected.**

### Description
`migrations/00001_{what}.sql`, `-- +goose Up` and `-- +goose Down`, embedded by `package migrations`; `postgres.Migrate` applies them with goose's provider and records them in `goose_db_version`. The scenario of `model_changes.feature` migrates one database, lets GORM's `AutoMigrate` build another from `tables.Rows`, and compares their columns, types, nullability, and unique keys.

### Benefits
- One Go module, no external binary; the job exits 0 on a database already up to date, proved by a scenario.
- The SQL is what the database runs, read in review as it is.
- A field with no migration, or a migration the rows do not match, fails `make test`.

### Costs
- A migration is written by hand, where dotnet generates it.
- The check covers columns and unique keys, not foreign keys, defaults, or checks; those are read in review.

## atlas generating migrations from the GORM rows

### Description
`atlas migrate diff` with the GORM provider writes the SQL from the rows.

### Benefits
- Generated, as in dotnet.

### Costs
- A binary outside the module, installed in every project and in CI.

## GORM's AutoMigrate

### Description
The schema is changed at start to fit the rows.

### Benefits
- Nothing to write.

### Costs
- No history; every replica changes the schema at start, which [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md#apply-migrations-in-one-mode) forbids.
