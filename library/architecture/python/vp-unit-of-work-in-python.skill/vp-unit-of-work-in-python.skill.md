---
name: vp-unit-of-work-in-python
description: The unit of work variation point in Python — package unit_of_work of infrastructure/postgres, the router of the database's connections added by attach through Database.use, which opens the transaction on a begun unit's first write with a savepoint per call, and UnitOfWorkMiddleware listed last in the command middleware, which begins the unit in a context variable and ends it shielded from cancellation; nested commands join through the context, repositories left unchanged
whenToUse: when a Python application with vp-postgres-in-python has a command that writes in several places or runs nested commands, or when you review a Python unit of work
updated: 20261103
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/calls-routed-by-a-context-variable.md
---

# Goal
- **The unit of work variation point in Python** - A Python application that keeps every property of [vp-unit-of-work](library/architecture/spec/vp-unit-of-work.skill/vp-unit-of-work.skill.md); this skill details its implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-postgres-in-python](library/architecture/python/vp-postgres-in-python.skill/vp-postgres-in-python.skill.md)'s example: no repository, service, or handler changes.

# Core Principle
- **The context is the unit** - `begin` puts the unit into a context variable of the command's task. Every nested command, service, and repository awaited there joins it, and so does a task the handler starts.
- **The router gives the connection** - `UnitOfWork` is the database's `ConnectionRouter`. A write of a begun unit gets the unit's connection and opens its transaction; a read gets it once the transaction is open. So a repository stays unaware of it. Decision recorded in [calls-routed-by-a-context-variable](./adr/calls-routed-by-a-context-variable.md).

```
src/{package}/
├── infrastructure/postgres/unit_of_work/   copied: attach, UnitOfWork (begin, connection_for), Unit (commit, rollback)
│   └── features/  test/                    the copy of the shared scenarios, the Python ones, the steps
└── command/
    ├── middleware/unit_of_work/            copied: UnitOfWorkMiddleware(units), Units, Unit
    │   └── features/  test/                the middleware; nested commands over the database
    └── import_items.py                     the sample: AddItem once per title
```

# Rule

## MUST

### Copy the fixed code from templates
Copy [templates/](./templates/) to the project, replacing `{package}` with the package name in the paths and the imports.
- Violation: a transaction begun in a handler or a service; `unit.commit()` called outside the middleware.
- Risk: a commit inside a nested command ends the transaction of its parent; a service that commits breaks the unit.
- Fix: only `UnitOfWorkMiddleware` ends a unit; searching the copies for `{package}` returns nothing.

### Attach it before the first statement and list its middleware last
Call `attach(database)` right after `postgres.Database` in `# [services]`, and add `UnitOfWorkMiddleware(units)` as the last of the command middleware.

```python
# [services]
database = postgres.Database(database_config)
units = attach_unit_of_work(database)   # from {package}.infrastructure.postgres.unit_of_work import attach as attach_unit_of_work

# [handlers]
handlers = build_handlers(
    [
        LoggingMiddleware(logger),
        UnitOfWorkMiddleware(units),
    ],
    lists=item_lists,
)
```

- Violation: `attach` after the migrations or a first request; the middleware before the logging or an access check.
- Risk: a call before `attach` runs with no unit; a failed commit is missing from the command's log record; a rejected command opens a unit.
- Fix: the middleware's package sees the unit only through its `Units` protocol, with no import of the database.

### Await nested commands one at a time
Await each nested command through its decorated handler, one after another, as [base-plateau-extension-in-python](library/architecture/python/base-plateau-extension-in-python.skill/base-plateau-extension-in-python.skill.md#pass-a-nested-commands-decorated-handler) requires, and start no task that writes after its command returned.
- Violation: `asyncio.gather` over nested commands; `asyncio.create_task` for a write the handler does not await.
- Risk: the calls of one unit share one connection, which serves one call at a time; a write after the end is refused (`UnitEndedError`).
- Fix: each call has a savepoint, so a duplicate key the handler handles leaves the transaction usable; work after the command goes through an outbox.

### Pass every write past the contract through run
Run a statement past the repository's contract in `Statements.run`, as [vp-postgres-in-python](library/architecture/python/vp-postgres-in-python.skill/vp-postgres-in-python.skill.md) requires of every statement past the contract, with `write=False` only for one that reads.
- Violation: a write through `run(..., write=False)`; a session made from `database.engine()`.
- Risk: a write marked as a read runs on the pool as the first write of a command and stays after a rollback; a session of its own is outside the unit.
- Fix: `write=True` is the default of `run`.

### Run the shared scenarios
Name both `conformance/` folders in [conformance.copy](./conformance.copy): the example is a full copy of the PostgreSQL one, so it runs those scenarios too.
- Risk: a change of the repository breaks the unit of work, and only a run of the two together shows it.
- Fix: `make conformance-sync` writes the copies; the steps of `unit-of-work.feature` are in `infrastructure/postgres/unit_of_work/test/conftest.py`, every step of a scenario run in one context.

# Check list
- [ ] `infrastructure/postgres/unit_of_work` and `command/middleware/unit_of_work` are unchanged copies of the templates.
- [ ] `attach(database)` follows `postgres.Database`; `UnitOfWorkMiddleware` is last in the command middleware.
- [ ] Only the middleware ends a unit; nested commands are awaited one at a time through their decorated handlers.
- [ ] Every write past the contract runs in `Statements.run` with `write=True`.
- [ ] `make test` and `make lint` pass with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
One import of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the PostgreSQL example with the unit of work attached. `ImportItems` keeps all its titles or none, proven in `src/todo/command/middleware/unit_of_work/features/nested_commands.feature`. It has no entry point; its scenarios send it.
