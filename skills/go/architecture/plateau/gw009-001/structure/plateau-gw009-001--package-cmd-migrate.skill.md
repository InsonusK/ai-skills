---
name: plateau-gw009-001--package-cmd-migrate
description: cmd/migrate package of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when running or changing the one-shot migration job of this plateau
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/package
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
---

# Goal
A one-shot binary that applies the service's migrations before the service starts (Job mode).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/migrate/Package.create.md#MUST|cmd/migrate]]

# Core Principles
- Same composition-root shape as `cmd/linkcheck`: `main` logs and exits, `run` returns an error.

# Structure
## Package Structure
```
cmd/migrate/
  main.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| main.go | loads config, calls `linkstore.Migrate` | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-cmd-migrate-main.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| — | — | standard library only |

# Allowed Dependencies
- `internal/config`, `internal/infrastructure/linkstore`

# Rules
MUST:
- Never let `cmd/migrate` and `MIGRATE_ON_START=true` both run for one deployment.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/migrate/Package.create.md#MUST|cmd/migrate]]
