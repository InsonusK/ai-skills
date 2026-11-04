---
name: vp-unit-of-work-in-go
description: The unit of work variation point in Go — package unitofwork of internal/infrastructure/postgres, a GORM plugin added by Attach through Database.Use that opens the transaction on a begun unit's first write, behind a savepoint per write, and commandunitofwork.Middleware listed last in the command middleware, which begins the unit in the command's context and ends it; nested commands join through the context, repositories left unchanged
whenToUse: when a Go application with vp-postgres-in-go has a command that writes in several places or runs nested commands, or when you review a Go unit of work
updated: 20261103
tags:
  - stack/go
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/statements-routed-by-a-gorm-plugin.md
---

# Goal
- **The unit of work variation point in Go** - A Go application that keeps every property of [vp-unit-of-work](library/architecture/spec/vp-unit-of-work.skill/vp-unit-of-work.skill.md); this skill details its implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-postgres-in-go](library/architecture/go/vp-postgres-in-go.skill/vp-postgres-in-go.skill.md)'s example: no repository, service, or handler changes.

# Core Principle
- **The context is the unit** - `Begin` puts the unit into the command's `context.Context`. Every nested command, service, and repository called with that context joins it.
- **The plugin routes the statements** - GORM's callbacks put a write of a begun unit into its transaction, and a read once the transaction is open, so a repository stays unaware of it. Decision recorded in [statements-routed-by-a-gorm-plugin](./adr/statements-routed-by-a-gorm-plugin.md).

```
internal/
├── infrastructure/postgres/unitofwork/      copied: Attach, UnitOfWork (Begin, Commit, Rollback), the plugin
│   └── features/  test/                     the copy of the shared scenarios, the Go ones, the steps
└── command/
    ├── middleware/unitofwork/               copied: Middleware(units), Units
    │   └── features/  test/                 the middleware; nested commands over the database
    └── import_items.go                      the sample: AddItem once per title
```

# Rule

## MUST

### Copy the fixed code from templates
Fill and copy [templates/](./templates/) to the module, replacing `{module-path}` with the module path from `go.mod`.
- Violation: a transaction begun in a handler or a service; `Commit` called outside the middleware.
- Risk: a commit inside a nested command ends the transaction of its parent; a service that commits breaks the unit.
- Fix: only `commandunitofwork.Middleware` calls `Commit` and `Rollback`; searching the copies for `{module-path}` returns nothing.

### Attach it before the first statement and list its middleware last
Call `unitofwork.Attach(db)` right after `postgres.Open` in `// [services]`, and add `commandunitofwork.Middleware(units)` as the last of the command middleware.

```go
// [services]
db, err := postgres.Open(dbConfig)
defer db.Close()
units, err := unitofwork.Attach(db)   // a failure: logger.Error("startup failed", ...), return 1

// [handlers]
handlers := command.NewHandlers([]pipeline.Middleware{
	commandlogging.Middleware(logger),
	commandunitofwork.Middleware(units),
}, itemLists)
```

- Violation: `Attach` after the migrations or a first request; the middleware before the logging or an access check.
- Risk: a statement before `Attach` runs on the old pool; a failed commit is missing from the command's log record; a rejected command opens a unit.
- Fix: the middleware package shares its name with the unit's and is imported as `commandunitofwork`.

### Pass the command's context on, and run nested commands one at a time
Pass a handler's `ctx` to every nested command, service, and repository it calls, and run nested commands one after another, each through its decorated handler, as [base-plateau-extension-in-go](library/architecture/go/base-plateau-extension-in-go.skill/base-plateau-extension-in-go.skill.md#pass-a-nested-commands-decorated-handler) requires.
- Violation: `context.Background()` inside a handler; nested commands run in goroutines; a goroutine that writes after its command returned.
- Risk: a write with another context runs outside the unit, and a rollback does not undo it. The statements of one unit share one connection. A write after the end is refused (`the command has ended its unit`).
- Fix: each write has a savepoint, so a duplicate key the handler handles leaves the transaction usable; work after the command goes through an outbox.

### Write past the contract by Exec or a write method
Run a statement past the repository's contract in `Statements.Run` with `Exec`, `Create`, `Updates`, or `Delete`, as [vp-postgres-in-go](library/architecture/go/vp-postgres-in-go.skill/vp-postgres-in-go.skill.md) requires of every statement past the contract.
- Violation: `session.Raw("UPDATE ... RETURNING id").Scan(&ids)` as the first write of a command.
- Risk: `Raw(...).Scan` is a read to GORM. As the first write it runs on the pool and stays after a rollback.
- Fix: `Exec`, then a read of what it changed; once the transaction is open, a read runs in it.

### Run the shared scenarios
Name both `conformance/` folders in [conformance.copy](./conformance.copy): the example is a full copy of the PostgreSQL one, so it runs those scenarios too.
- Risk: a change of the repository breaks the unit of work, and only a run of the two together shows it.
- Fix: `make conformance-sync` writes the copies; the steps of `unit-of-work.feature` are in `internal/infrastructure/postgres/unitofwork/test/steps_test.go`.

# Check list
- [ ] `internal/infrastructure/postgres/unitofwork` and `internal/command/middleware/unitofwork` are unchanged copies of the templates.
- [ ] `unitofwork.Attach(db)` follows `postgres.Open`; `commandunitofwork.Middleware` is last in the command middleware.
- [ ] Only the middleware ends a unit; handlers pass their `ctx` on, and run nested commands one at a time through their decorated handlers.
- [ ] Every write past the contract uses `Exec` or a write method in `Statements.Run`.
- [ ] `make test` passes with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
One import of the example, with its real types:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the PostgreSQL example with the unit of work attached. `ImportItems` keeps all its titles or none, proven in `internal/command/middleware/unitofwork/features/nested_commands.feature`. It has no entry point; its scenarios send it.
