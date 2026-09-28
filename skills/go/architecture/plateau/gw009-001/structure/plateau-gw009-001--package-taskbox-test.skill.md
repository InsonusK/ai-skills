---
name: plateau-gw009-001--package-taskbox-test
description: internal/taskbox/test package of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when running or extending the TaskBox conformance run, or adding a store to it
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/package
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
---

# Goal
Run solution-taskbox's conformance feature on every store this service's TaskBox supports — here PostgreSQL.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/Package.create.md#MUST|internal/taskbox/test]]

# Core Principles
- `internal/taskbox/features/taskbox-conformance.feature` is a verbatim copy; this package only implements its step vocabulary.
- `TEST_DATABASE_DSN` is required; an empty value fails the run.

# Structure
## Package Structure
```
internal/taskbox/test/
  runner_test.go
  world_test.go
  steps_test.go
  postgres_test.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| runner_test.go | `TestFeatures`, one suite per store | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-taskbox-test-runner-test.skill.md]] |
| world_test.go | per-scenario state, `storeUnderTest` | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-taskbox-test-world-test.skill.md]] |
| steps_test.go | the step vocabulary | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-taskbox-test-steps-test.skill.md]] |
| postgres_test.go | PostgreSQL store under test | [[skills/go/architecture/plateau/gw009-001/structure/plateau-gw009-001--file-taskbox-test-postgres-test.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/cucumber/godog | >= 0.16 | the runner |

# Allowed Dependencies
- `internal/taskbox`, `internal/taskbox/pgstore`, `internal/infrastructure/linkstore` (its `Migrate`)

# Rules
MUST:
- Never skip the feature when the database is missing.
- Never edit the feature copy locally.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/Package.create.md#MUST|internal/taskbox/test]]

# Check list
- [ ] 30 scenarios pass on PostgreSQL; the `@store-transient` one is `missing` in the report (no VP-C002 TaskBox store yet).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/Package.create.md#MUST|internal/taskbox/test]]
