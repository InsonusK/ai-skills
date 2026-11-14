# todo

The base plateau with the CLI and PostgreSQL variation points: EF Core maps
entities in their modules, and handlers use Ardalis repositories. Every action
runs in a new process; a list outlives it.

```bash
export DATABASE_DSN='Host=localhost;Database=todo;Username=todo;Password=todo'
make init
make migrate
make run ARGS='items plan --title "buy milk"'
make run ARGS='lists add --list home --title "buy milk"'
make run ARGS='lists show --list home'
make migration NAME=AddItemDueDate
TEST_DATABASE_DSN='Host=localhost;Username=postgres' make test
```

`--output text` prints a checklist; the default output is one JSON line. A
successful action exits 0, a rejected request exits 2, and an unavailable
database exits 1. Errors go to stderr, with no result on stdout.

| Setting | Default | Meaning |
|---|---|---|
| `DATABASE_DSN` | required | the database, a connection string of Npgsql |
| `MIGRATE_ON_START` | `false` | apply the migrations before the entry points start: only for a deployment pinned to one instance |
| `TEST_DATABASE_DSN` | required by `make test` | the PostgreSQL the scenarios create and drop their databases on |

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the CLI (`Api/Cli`), base settings |
| `src/App/App.Migrate` | the migration job, `/app/migrate` of the image |
| `src/Modules/Todo/Todo.Commands` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers, each over `IRepository<Item>`, and `Specifications/` |
| `src/Modules/Todo/Todo.Domain` | entities, `Configurations/` (the tables the module owns), domain services, the rule errors |
| `src/Infrastructure/Postgres/Postgres` | `AppDbContext` (a line per module's configurations), `Repositories/`, `PostgresFailures`, `Migrations/`, settings |
| `src/Infrastructure/Postgres/Postgres.Testing` | the database of one scenario, and `InMemoryRepository<T>` for the handlers' scenarios |
| `src/Middleware/StoreFailures` | the command middleware that tells a failure of the database in the error kinds |
| `src/Shared` | the error kinds — fixed code, no packages |
| `src/Shared.Persistence` | the repository's contract (`IRepository<T>`, `IStoreFailures`), with Ardalis.Specification |
| `src/BuildingBlocks` | fixed code |

Every `{Project}.Tests` sits beside the project it tests.
