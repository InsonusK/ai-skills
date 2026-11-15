# todo

The gRPC API and its HTTP form with the PostgreSQL and the row version variation points
attached: an item of a list changes (it is completed), so its row declares its version,
PostgreSQL's `xmin`, which the base repository compares on every write. Every item the API
answers carries its id and its version; a change gives the version back:

| rpc | HTTP | Shows |
|---|---|---|
| `PlanItems` | `POST /v1/items/plan` | no database |
| `AddItem` | `POST /v1/lists/{listId}/items` | `Insert`; the item with its id and first version; a taken title is 409, a database down 503 |
| `ListItems` | `GET /v1/lists/{listId}/items` | `SelectBy(ItemsOfList).OrderBy(ItemPosition).All`, each item with its version; no rows is 404 |
| `CompleteItem` | `POST /v1/lists/{listId}/items/{itemId}/complete` | `Update` under the caller's version: an item changed since is 409 `ABORTED`, no version is 400, done already 400 `FAILED_PRECONDITION` |
| `ClearList` | `POST /v1/lists/{listId}/clear` | a deprecated method: 501 / `UNIMPLEMENTED` |

```bash
export DATABASE_DSN='postgres://todo:todo@localhost:5432/todo'
make init       # once: module dependencies
make migrate    # bring the database up to the latest migration (cmd/migrate, /app/migrate of the image)
make run        # serve gRPC on :9090 and HTTP on :8080 (Swagger UI at /swagger/)
make proto-gen  # regenerate gen/api/ and todo.swagger.json after editing the proto file
TEST_DATABASE_DSN='host=localhost user=postgres' make test   # every scenario, each on a database of its own
```

| Setting | Default | Meaning |
|---|---|---|
| `DATABASE_DSN` | required | the database, in a form pgx reads: a URL or key=value pairs |
| `MIGRATE_ON_START` | `false` | apply the migrations before the entry points start: only for a deployment pinned to one instance |
| `TEST_DATABASE_DSN` | required by `make test` | the PostgreSQL the scenarios create and drop their databases on |

| Package | Holds |
|---|---|
| `cmd/todo` | the composition root: the database, the repository, the service over it |
| `cmd/migrate` | the migration job |
| `internal/infrastructure/postgres` | copied, never edited: settings, `Database`, `Repository`, the selectors, the outcomes, `Migrate` |
| `internal/infrastructure/postgres/postgrestest` | copied, never edited: a database per scenario, a repository that fails as asked |
| `internal/infrastructure/postgres/tables` | the application's rows, keys, and specifications; `marks.go`: `NeverChanges` |
| `internal/infrastructure/postgres/rowversion` | the shared scenarios of the version, on a table of notes of the test only |
| `internal/infrastructure/postgres/migrations` | the application's SQL migrations |
| `internal/service` | `ItemLists` over the repository: rows in, entities out, outcomes into kinds |
