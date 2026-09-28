---
description: TaskBox schema v1 joins the service's own migration history as one more goose migration
name: internal/infrastructure/{store}
element_kind: package
change_kind: extend
tags:
  - solution/taskbox-in-go
  - element/internal-infrastructure-store
---

# Goals
- Create the TaskBox tables through the service's own migrations, as contract §7 requires.

# Core Principles
- The migration's DDL is the contract's §6 PostgreSQL schema v1, copied **verbatim**; only the goose markers are added. A later contract version is a later migration file.
- `solution-go-db-migrations` owns the migration runner (`Migrate`, `cmd/migrate`, `MIGRATE_ON_START`); this solution adds a file to its `migrations/` directory.

# Structure

## Package Structure
```
internal/infrastructure/{store}/
  migrations/
    {NNNNN}_create_{table}.sql     ← solution-go-db-migrations
    {NNNNN+1}_taskbox_v1.sql       ← this solution
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| migrations/{NNNNN}_taskbox_v1.sql | `-- +goose Up` = contract §6 schema v1 DDL verbatim; `-- +goose Down` = `DROP TABLE IF EXISTS taskbox_group; DROP TABLE IF EXISTS taskbox_task;` | [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox.contract|contract §6]] |

# Rules

## MUST

### Copy the contract DDL verbatim
Put the contract's schema v1 DDL into the migration unchanged, next number after the service's latest migration.
- Violation: adding an index, renaming a column, or using `CREATE TABLE IF NOT EXISTS` at startup instead of a migration.
- Risk: a service moved to another stack finds a schema its migrations do not recognise (contract §7).
- Fix: copy the DDL; any schema change is a new contract version first.

# Check list
- [ ] `{NNNNN}_taskbox_v1.sql`'s Up section equals the contract's §6 DDL.
- [ ] `Migrate` against an empty database creates `taskbox_task` and `taskbox_group`.
