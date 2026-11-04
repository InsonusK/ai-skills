---
name: vp-postgres-in-python
description: The PostgreSQL variation point in Python — src/{package}/infrastructure/postgres copied from templates (Config of DATABASE_DSN and MIGRATE_ON_START, Database with its session per call and ping, the generic Repository and Projections over SQLAlchemy 2's async ORM on asyncpg with their selectors, the translation of SQLSTATE into outcomes, migrate over Alembic, postgrestest), rows and specification classes per table in tables/, Alembic migrations checked by alembic check, the job todo-migrate, and the shared scenarios at TEST_DATABASE_DSN
whenToUse: when a Python application built on the base plateau must keep its data in PostgreSQL, when you add a table, a specification, a repository method, or a migration in Python, or when you review a Python repository or migration
updated: 20261103
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/repository-over-sqlalchemy.md
  - adr/alembic-migrations-checked-against-the-rows.md
---

# Goal
- **The PostgreSQL variation point in Python** - A Python application that keeps every property of [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md); this skill details its implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it replaces the in-memory store of [vp-http-api-in-python](library/architecture/python/vp-http-api-in-python.skill/vp-http-api-in-python.skill.md)'s proto example and changes only the files [attach.allow](./attach.allow) names, which the repository's attach check verifies.

# Core Principle
- **SQLAlchemy behind one generic repository** - SQLAlchemy maps the rows and builds the statements; `Repository[TRow]`, the selectors, and the translation of failures are ours. Decision recorded in [repository-over-sqlalchemy](./adr/repository-over-sqlalchemy.md).
- **A write is saved when it returns** - Every call of a repository runs in a session and a transaction of its own, committed when it returns and rolled back when it raises.
- **Migrations generated from the rows, checked against them** - Alembic writes a migration from the changed rows; a scenario runs `alembic check`. Decision recorded in [alembic-migrations-checked-against-the-rows](./adr/alembic-migrations-checked-against-the-rows.md).

```
alembic.ini                            the alembic command of make migration
src/{package}/migrate.py               copied: the job todo-migrate, /app/migrate of the image
src/{package}/infrastructure/postgres/ copied, never edited
├── config.py                          Config: DATABASE_DSN hidden from every print, MIGRATE_ON_START
├── database.py                        Database (no statement): session per call, use (a router), ping, engine, close
├── repository.py                      Statements.run, Repository[TRow], Projections[TRow, TResult]
├── selector.py                        Selector, OrderedSelector, ResultSelector, OrderedResultSelector
├── specification.py                   Specification, Projection, Key, Columns
├── outcomes.py                        DuplicateKeyError, MissingReferenceError, RowChangedError, UnavailableError, RefusedError
├── migrate.py                         migrate(database, migrations.LOCATION)
├── postgrestest/                      copied: create (a database per scenario), not_answering, Failing
├── tables/                            per table: the row, its keys, its specifications; Row, their base
├── migrations/                        env.py copied; versions/: per change, one file
└── features/  test/                   the copies of the shared scenarios, the Python ones, the steps
```

# Rule

## MUST

### Copy the fixed code from templates
Copy [templates/](./templates/) to the project, replacing `{package}` with the package name in the paths and the imports, add `sqlalchemy[asyncio]`, `asyncpg`, and `alembic` at the example's versions, the script `todo-migrate`, the mypy override of `asyncpg.*`, `alembic.ini`, and the Makefile targets `migrate` and `migration`.
- Violation: a second `Database` per process; a fixed file edited for a table; an engine created in a service.
- Risk: two pools mean two settings to keep; an edited copy loses the next fix of the templates.
- Fix: a table adds its row, its specifications, and its migration; searching the copies for `{package}` returns nothing.

### Open the database in the composition root
Load `postgres.load_config()`, build `postgres.Database` over it, apply `postgres.migrate` only when `migrate_on_start` is set, build each service over `postgres.Repository(database, {Thing}Row)`, all in `# [services]` of `main.py`, and close the database after `run_all`.

```python
# [services]
database_config = postgres.load_config()   # a ValueError: logger.error("startup failed", ...), return 1
database = postgres.Database(database_config)
if database_config.migrate_on_start:
    await postgres.migrate(database, migrations.LOCATION)
item_lists = ItemLists(postgres.Repository(database, ItemRow))
```

- Violation: `create_async_engine` in a service; a repository built per call; `print(database_config.url())`.
- Risk: each pool holds its own connections; the URL's text names the host and the user, and a DSN printed whole carries the password.
- Fix: one `Database` per process, its repositories built once; `Config` prints `dsn=(hidden)` for `str`, `repr`, and f-strings, and has no `__dict__`.

### Write a row per table
Declare a `{Thing}Row` class of `tables.Row` in `tables/`, with `__tablename__`, the primary key of one column, a `UniqueConstraint` for every rule of uniqueness named as the migration names it, and its keys (`{Name}: Key[{Thing}Row] = Key({Thing}Row.column)`).

```python
class ItemRow(Row):
    __tablename__ = "list_items"
    __table_args__ = (UniqueConstraint("list_id", "title", name="list_items_list_id_title_key"),)

    id: Mapped[int] = mapped_column(Identity(), primary_key=True)
    list_id: Mapped[str]
    title: Mapped[str]
    done: Mapped[bool]

ItemPosition: Key[ItemRow] = Key(ItemRow.id)
```

- Violation: a domain entity mapped; a row of another base; uniqueness checked with a read before the insert only.
- Risk: the domain depends on SQLAlchemy; `alembic check` skips the row; two concurrent inserts both pass a check that has no index behind it.
- Fix: the constraint is the rule; the repository reports its violation as `DuplicateKeyError`.

### Write a specification per set of rows
Declare a frozen dataclass of `postgres.Specification[TRow]`, or `postgres.Projection[TRow, TResult]` for a projection, and apply only `where`, `join`, and, for a projection, `with_only_columns` with every column labelled by a field of the result in its `pick`.

```python
@dataclass(frozen=True)
class ItemsOfList(Specification[ItemRow]):
    list_id: str

    def pick(self, query: Select[ItemRow]) -> Select[ItemRow]:
        return query.where(ItemRow.list_id == self.list_id)
```

- Violation: `order_by`, `limit`, or `offset` in `pick`, which the selector refuses; a statement run in `pick`; a condition built as text from a value.
- Risk: a statement in `pick` runs on every call of the selector; a value in the text is an injection.
- Fix: a join follows a relation of the row, `query.join(TaskRow.owner)`; a projection is read by `postgres.Projections(database, TaskRow, OwnerAndTitle).select_by`.

### Use the repository from the service
Declare in the service a `Protocol` of the methods it needs of `postgres.Repository[TRow]`, ask `select_by` for a selector, and turn its outcomes into kinds as [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md#report-a-repositorys-outcomes-in-its-own-terms) requires, raising the kind `from` the outcome.

```python
class ItemRows(Protocol):
    async def insert(self, row: ItemRow) -> None: ...
    def select_by(self, specification: Specification[ItemRow]) -> Selector[ItemRow]: ...

try:
    rows = await self._rows.select_by(ItemsOfList(list_id)).order_by(ItemPosition).all()
except UnavailableError as error:
    raise ListsUnavailableError() from error   # KindError(Kind.UNAVAILABLE, "the lists are unavailable")
```

- Violation: an `AsyncSession` in a service; a selector returned by a service method; `KindError(Kind.UNAVAILABLE, str(error))`.
- Risk: a query built above the repository depends on SQLAlchemy; the message of a `KindError` is answered to the caller, and the driver's text names the database's host and user.
- Fix: the message is fixed; the outcome stays the `__cause__`, which a traceback shows and the answer does not.

### Pass a statement of its own through run
Declare a `{Thing}Repository` class of `postgres.Repository[TRow]` with its methods past the contract, each running its statement in `self.run(what, statement)`, and `write=False` for one that only reads.

```python
async def rename(self, title: str, to: str) -> None:
    async def run(session: AsyncSession) -> None:
        await session.execute(text("UPDATE tasks SET title = :to WHERE title = :title"), {"to": to, "title": title})

    await self.run("rename in tasks", run)
```

- Violation: an engine or a session kept beside the repository; a statement run outside `run`.
- Risk: the rules of [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md#go-past-the-contract-with-a-method-of-its-own) break: its failure is no outcome, and a unit of work cannot reach it.
- Fix: [statements.feature](./examples/src/todo/infrastructure/postgres/features/statements.feature) shows an upsert and a bulk update.

### Write every migration with its row
Run `make migration NAME={what}` for every change of a row, read the file it writes, and commit both together; apply with `make migrate` locally and with `todo-migrate`, the image's `/app/migrate`, in a deployment.
- Violation: a migration edited after it was applied somewhere; `Row.metadata.create_all` in the application; the scenarios of [model_changes.feature](./examples/src/todo/infrastructure/postgres/features/model_changes.feature) removed.
- Risk: the rows and the schema drift, and the first statement reading a new column fails in production; a rename written as a drop and an add loses the column's data.
- Fix: a new file for every further change; `alembic check` compares columns, types, nullability, keys, and indexes.

### Pass the readiness check as a function
Pass `database.ping` where the health variation point takes a check, `management.Check("database", database.ping)` with [vp-health-in-python](library/architecture/python/vp-health-in-python.skill/vp-health-in-python.skill.md), and nothing with no health entry point.
- Violation: an import of `api.management` in package `postgres`; a check that sends a command.
- Risk: the database stops building without the health entry point; a check through a command fails on a business rule and holds traffic back for nothing.
- Fix: `ping` runs `SELECT 1` on a connection of the pool and raises while the database does not answer.

### Run the shared scenarios
Name the shared scenarios in [conformance.copy](./conformance.copy), run `make conformance-sync` at the repository root, and implement their steps in `infrastructure/postgres/test/conftest.py`, each comparing the fields of the row objects.
- Violation: a copy edited; a step comparing titles joined into one text.
- Risk: the contract is proved in another stack and not in this one.
- Fix: the sample tables, their specifications, and `TaskRepository` are in `test/sample.py`, of a declarative base of their own, created by SQL for the scenario. What only Python has stays beside the copies, in `config_forms.feature`, `model_changes.feature`, `repository_in_python.feature`, and `statements.feature`.

### Create the database of each scenario
Create a database with `postgrestest.create()` in a step or fixture of each scenario that needs one, run every step of the scenario on one event loop, and drop it at the end; give a service's scenario an outcome through `postgrestest.Failing`.
- Violation: a database shared by the scenarios of a package; `asyncio.run` per step over one `Database`; a fake repository that ignores the specification.
- Risk: scenarios see each other's rows; a pool's connections belong to the loop that opened them and fail on the next one; a wrong condition passes.
- Fix: `create` fails without `TEST_DATABASE_DSN`, a URL whose user may create databases; `drop` closes the connections still open.

# Check list
- [ ] The fixed files are filled copies of the templates; one `Database` per process, closed after `run_all`.
- [ ] Every row derives from `tables.Row`, with one key column and a named `UniqueConstraint` for every rule of uniqueness.
- [ ] A specification applies only `where` and `join`, a projection also `with_only_columns`.
- [ ] A statement past the contract runs in `run`; no session leaves package `postgres` but through `pick` and `run`.
- [ ] No `KindError` carries the repository's failure in its message.
- [ ] Every change of a row has its migration; the scenarios of `model_changes.feature` pass.
- [ ] `todo-migrate` is the image's `/app/migrate`; `MIGRATE_ON_START` stays `false` beyond one instance.
- [ ] `make test` and `make lint` pass with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
One call of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the proto example with its lists in PostgreSQL: `make migrate && make run`, then the `curl` lines of its readme. Stop it, run it again, and the items are still there.
