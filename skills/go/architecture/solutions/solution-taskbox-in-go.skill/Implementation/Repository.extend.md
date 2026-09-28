---
description: go.mod gains google/uuid; the test gate needs a real PostgreSQL through TEST_DATABASE_DSN
element_kind: repository
change_kind: extend
tags:
  - solution/taskbox-in-go
  - element/repo-root
---

# Structure

## Repository Structure
```
go.mod                          ← + github.com/google/uuid (direct)
internal/
  taskbox/                      ← this solution
  api/tasks/                    ← this solution
```

## Directory and package skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| internal/taskbox | taskbox.go, handler.go, worker.go | [[./internal/taskbox/Package.create.md|TaskBox mechanism]] |
| internal/taskbox/pgstore | store.go | [[./internal/taskbox/pgstore/Package.create.md|PostgreSQL store]] |
| internal/api/tasks | handlers.go | [[./internal/api/tasks/Package.create.md|task handlers]] |

# Rules

## MUST

### Run the unit-test gate against a real PostgreSQL
Set `TEST_DATABASE_DSN` for `make unit-test` and `make test-and-report`, locally and in CI.
- Risk: without it the TaskBox runner fails the gate (by design, it never skips).
- Fix: point it at a throwaway database, e.g. `postgres://postgres:postgres@localhost:5432/taskbox_test`; CI starts a PostgreSQL service container for the job.

# Check list
- [ ] `go.mod` lists `github.com/google/uuid` as a direct requirement.
- [ ] `TEST_DATABASE_DSN=… make unit-test` passes.
