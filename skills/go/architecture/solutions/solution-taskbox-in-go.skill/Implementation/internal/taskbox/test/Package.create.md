---
description: The godog package that runs solution-taskbox's conformance feature against every store this service's TaskBox supports
name: internal/taskbox/test
element_kind: package
change_kind: create
tags:
  - solution/taskbox-in-go
  - element/internal-taskbox-test
---

# Goals
- Prove this service's TaskBox conforms to the contract by running the shared [[skills/common-workflow/architecture/solutions/solution-taskbox.skill/Implementation/features/taskbox-conformance.feature.create.md|taskbox-conformance.feature]] on every supported store.

# Core Principles
- The feature file is copied **verbatim** to `internal/taskbox/features/taskbox-conformance.feature`; this package only implements its step vocabulary.
- One `TestFeatures` runner per package (cucmber-testing-in-go), one godog suite per store, tag filter `~@store-transient` for a VP-C001 store and `~@store-persistent` for a VP-C002 store.
- The run needs a real database: `TEST_DATABASE_DSN` (PostgreSQL) — see [[../../../../adr/conformance-on-a-real-database.md|adr/conformance-on-a-real-database]].

# Structure

## Package Structure
```
internal/taskbox/
  features/
    taskbox-conformance.feature   ← verbatim copy
  test/
    runner_test.go
    world_test.go
    steps_test.go
    postgres_test.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| runner_test.go | `TestFeatures`, one suite per store | [[./runner_test.go.create.md|runner_test.go]] |
| world_test.go | per-scenario state, `storeUnderTest` | [[./world_test.go.create.md|world_test.go]] |
| steps_test.go | the step vocabulary | [[./steps_test.go.create.md|steps_test.go]] |
| postgres_test.go | PostgreSQL store under test | [[./postgres_test.go.create.md|postgres_test.go]] |

# Rules

## MUST

### Copy the feature verbatim
Copy `taskbox-conformance.feature` byte for byte from solution-taskbox, per [[skills/common-workflow/architecture/solutions/solution-taskbox.skill/Implementation/features/taskbox-conformance.feature.create.md#MUST|its rules]].
- Risk: a locally edited feature no longer proves conformance to the shared contract.
- Fix: change the feature in solution-taskbox first, then copy it again.

# Check list
- [ ] `internal/taskbox/features/taskbox-conformance.feature` is identical to solution-taskbox's.
- [ ] `TEST_DATABASE_DSN=… make unit-test` runs the feature and every non-excluded scenario passes; the `@store-transient` scenario is `missing` in the report until a VP-C002 store is supported.
