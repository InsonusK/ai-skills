---
name: vp-postgres-in-go
description: The PostgreSQL variation point in Go — internal/infrastructure/postgres copied from templates (Config of DATABASE_DSN and MIGRATE_ON_START, Database with Ping, the generic Repository and Projections over GORM with their selectors, the translation of pgx's codes into outcomes, Migrate over goose, postgrestest), rows and specification types per table in package tables, SQL migrations embedded by package migrations and checked against the rows, the job cmd/migrate, and the shared scenarios at TEST_DATABASE_DSN
whenToUse: when a Go application built on the base plateau must keep its data in PostgreSQL, when you add a table, a specification, a repository method, or a migration in Go, or when you review a Go repository or migration
updated: 20261104
tags:
  - stack/go
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/repository-over-gorm.md
  - adr/goose-migrations-checked-against-the-rows.md
  - adr/service-scenarios-on-the-real-engine.md
---

# Goal
- **The PostgreSQL variation point in Go** - A Go application that keeps every property of [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md); this skill details its implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it replaces the in-memory store of [vp-http-api-in-go](library/architecture/go/vp-http-api-in-go.skill/vp-http-api-in-go.skill.md)'s proto example and changes only the files [attach.allow](./attach.allow) names, which the repository's attach check verifies.

# Core Principle
- **GORM behind one generic repository** - GORM maps the rows and builds the statements; `Repository[TRow]`, the selectors, and the translation of failures are ours. Decision recorded in [repository-over-gorm](./adr/repository-over-gorm.md).
- **A write is saved when it returns** - No transaction of its own, no session state: GORM's default transaction is off, and the next call starts on a clean statement.
- **Migrations are SQL, checked against the rows** - goose applies the embedded files; a scenario compares their schema with the rows. Decision recorded in [goose-migrations-checked-against-the-rows](./adr/goose-migrations-checked-against-the-rows.md).

```
cmd/migrate/main.go                    copied: the job, /app/migrate of the image
internal/infrastructure/postgres/      copied, never edited
├── config.go                          Config: DATABASE_DSN hidden from every print, MIGRATE_ON_START
├── database.go                        Open (no statement), Ping (the readiness check), Use (a GORM plugin), SQL, Close
├── repository.go                      Statements.Run, Repository[TRow], Projections[TRow, TResult]
├── selector.go                        Selector, OrderedSelector, ResultSelector, OrderedResultSelector
├── specification.go                   Specification, SpecificationOf, Projection, ProjectionOf, Key
├── outcomes.go                        ErrDuplicateKey, ErrMissingReference, ErrRowChanged, ErrUnavailable, ErrRefused
├── migrate.go                         Migrate(ctx, db, migrations.Files)
├── postgrestest/                      copied: Create (a database per scenario), NotAnswering, Failing
├── tables/                            per table: the row, its keys, its specifications
├── migrations/                        per change: 0000N_{what}.sql, embedded
└── features/  test/                   the copies of the shared scenarios, the Go ones, the steps
```

# Rule

## MUST

### Copy the fixed code from templates
Fill and copy [templates/](./templates/) to the module, replacing `{module-path}` with the module path from `go.mod`, add `gorm.io/gorm`, `gorm.io/driver/postgres`, `github.com/jackc/pgx/v5`, and `github.com/pressly/goose/v3` at the example's versions, and the Makefile target `migrate`.
- Violation: a second `Database` per module; a fixed file edited for a table; `TranslateError` turned on.
- Risk: two pools mean two settings to keep; an edited copy loses the next fix of the templates; GORM's translation hides the codes `translate` reads.
- Fix: a table adds its row, its specifications, and its migration; searching the copies for `{module-path}` returns nothing.

### Open the database in the composition root
Load `postgres.LoadConfig()`, `postgres.Open` it, apply `postgres.Migrate` only when `MigrateOnStart` is set, and build each service over `postgres.NewRepository[TRow](db)`, all in `// [services]` of `cmd/{app}/main.go`; pass each service to `command.NewHandlers` in `// [handlers]`, which hands it to the handlers that take its interface.

```go
import (
	"{module-path}/internal/infrastructure/postgres"
	"{module-path}/internal/infrastructure/postgres/migrations"
	"{module-path}/internal/infrastructure/postgres/tables"
	"{module-path}/internal/service"
)

// [services]
dbConfig, err := postgres.LoadConfig()
if err != nil {
	logger.Error("startup failed", "error", err) // names the variable, never the DSN
	return 1
}
db, err := postgres.Open(dbConfig)
if err != nil {
	logger.Error("startup failed", "error", err)
	return 1
}
defer db.Close() // after entry.RunAll returns: every entry point has stopped
if dbConfig.MigrateOnStart {
	if err := postgres.Migrate(ctx, db, migrations.Files); err != nil {
		logger.Error("startup failed", "error", err)
		return 1
	}
}
itemLists := service.NewItemLists(postgres.NewRepository[tables.ItemRow](db))

// [handlers]
handlers := command.NewHandlers([]pipeline.Middleware{
	commandlogging.Middleware(logger),
}, itemLists)
```

A second service over the database is one more line in `// [services]` and one more argument of `command.NewHandlers`, the same `db`: `planner := service.NewPlanner(postgres.NewRepository[tables.PlanRow](db))`.

- Violation: `gorm.Open` in a service; a repository built per call; `fmt.Println(dbConfig.DSN())`.
- Risk: each pool holds its own connections; the DSN, with its password, reaches the log.
- Fix: one `Database` per process, its repositories built once; `Config` prints `(hidden)` for `%v`, `%+v`, `%#v`, and slog.

### Write a row per table
Declare a `{Thing}Row` struct in package `tables` with a `TableName`, the primary key, and a `uniqueIndex` tag for every rule of uniqueness, named as the migration names it, its keys (`postgres.KeyOf[{Thing}Row]("column")`), and add it to `tables.Rows`.

```go
type ItemRow struct {
	ID     int64  `gorm:"primaryKey"`
	ListID string `gorm:"not null;uniqueIndex:list_items_list_id_title_key"`
	Title  string `gorm:"not null;uniqueIndex:list_items_list_id_title_key"`
	Done   bool   `gorm:"not null"`
}

func (ItemRow) TableName() string { return "list_items" }

var ItemPosition = postgres.KeyOf[ItemRow]("id")
```

- Violation: a domain entity mapped; a row missing from `tables.Rows`; uniqueness checked with a read before the insert only.
- Risk: the domain depends on GORM; the check of the migrations skips the row; two concurrent inserts both pass a check that has no index behind it.
- Fix: the index is the rule; the repository reports its violation as `ErrDuplicateKey`.

### Write a specification per set of rows
Declare a type that embeds `postgres.SpecificationOf[TRow]`, or `postgres.ProjectionOf[TRow, TResult]` for a projection, and apply only `Where`, `Joins`, `Preload`, and, for a projection, `Select` in its `Pick`.

```go
type ItemsOfList struct {
	postgres.SpecificationOf[ItemRow]
	ListID string
}

func (s ItemsOfList) Pick(query *gorm.DB) *gorm.DB {
	return query.Where("list_id = ?", s.ListID)
}
```

- Violation: `Order`, `Limit`, or `Offset` in `Pick`, which `SelectBy` refuses; `Find`, `Count`, or `Exec` in `Pick`; `Joins("Owner")` by a relation's name; a condition with a value written into the text instead of `?`.
- Risk: a statement in `Pick` runs on every call of the selector; a join by a relation's name selects its columns, and the subquery of `Update` and `Delete` fails; a value in the text is an injection.
- Fix: a join is `Joins("JOIN owners ON owners.id = tasks.owner_id")` and its columns are qualified; a related row is loaded by `Preload`; a projection names each column of its result, `Select("owners.name AS owner, tasks.title")`, and is read by `postgres.NewProjections[TRow, TResult](db).SelectBy`.

### Use the repository from the service
Declare in the service the methods it needs of `postgres.Repository[TRow]`, ask `SelectBy` for a selector, and turn its outcomes into kinds as [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md#report-a-repositorys-outcomes-in-its-own-terms) requires, with `errorkind.Wrap` and the repository's failure as the cause, never in the message.

```go
// internal/service/item_lists.go

// itemRows is what ItemLists needs of its infrastructure;
// postgres.Repository[tables.ItemRow] has it.
type itemRows interface {
	Insert(ctx context.Context, row *tables.ItemRow) error
	SelectBy(specification postgres.Specification[tables.ItemRow]) postgres.Selector[tables.ItemRow]
}

type ItemLists struct {
	rows itemRows
}

func NewItemLists(rows itemRows) *ItemLists {
	return &ItemLists{rows: rows}
}

func listsUnavailable(cause error) error {
	return errorkind.Wrap(errorkind.ErrUnavailable, "the lists are unavailable", cause)
}

func (s *ItemLists) Items(ctx context.Context, listID string) ([]entity.Item, error) {
	rows, err := s.rows.SelectBy(tables.ItemsOfList{ListID: listID}).OrderBy(tables.ItemPosition).All(ctx)
	if errors.Is(err, postgres.ErrUnavailable) {
		return nil, listsUnavailable(err) // the driver's text goes to the log as the cause
	}
	if err != nil {
		return nil, err
	}
	// ... rows to entities: the selector stops here
}
```

- Violation: `*gorm.DB` in a service; a selector returned by a service method; `fmt.Errorf("%w: %v", errorkind.ErrUnavailable, err)`; the driver's text in the message of an `errorkind.Wrap`.
- Risk: a query built above the repository depends on GORM; the message of an error with a kind is answered to the caller (`rpcstatus.Of`), and the driver's text names the database's host and user.
- Fix: the service sees package `postgres`'s types and `tables` only; the repository's failure is the cause of the error with a kind, which the log writes as `cause`.

### Pass a statement of its own through Run
Declare a `{Thing}Repository` struct that embeds `*postgres.Repository[TRow]`, with an interface that adds its methods to the contract, and run a statement past the contract in `Run`.

```go
func (r TaskRepository) Rename(ctx context.Context, from, to string) (int64, error) {
	return r.Run(ctx, "rename tasks", func(session *gorm.DB) *gorm.DB {
		return session.Exec("UPDATE tasks SET title = ? WHERE title = ?", to, from)
	})
}
```

- Violation: a `*gorm.DB` kept beside the repository; a statement run outside `Run`.
- Risk: the rules of [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md#go-past-the-contract-with-a-method-of-its-own) break: its failure is no outcome, and a unit of work cannot reach it.
- Fix: build it in `// [services]` as `TaskRepository{postgres.NewRepository[TaskRow](db)}`; [statements.feature](./examples/internal/infrastructure/postgres/features/statements.feature) shows an upsert and a bulk update.

### Write every migration with its row
Write `migrations/0000N_{what}.sql` with `-- +goose Up` and `-- +goose Down` for every change of a row, in the same commit, naming the unique keys as the row's tags do; apply with `make migrate` locally and with `cmd/migrate`, built into the image as `/app/migrate`, in a deployment.
- Violation: a migration edited after it was applied somewhere; `AutoMigrate` in the application; the scenario of [model-changes-in-go.feature](./examples/internal/infrastructure/postgres/features/model-changes-in-go.feature) removed.
- Risk: the rows and the schema drift, and the first statement reading a new column fails in production.
- Fix: a new file for every further change; the scenario compares columns, types, nullability, and unique keys, and a foreign key, a default, or a check is read in review.

### Pass the readiness check as a function
Pass `db.Ping` where the health variation point takes a check, `management.Check{Name: "database", Check: db.Ping}` with [vp-health-in-go](library/architecture/go/vp-health-in-go.skill/vp-health-in-go.skill.md), and nothing with no health entry point.
- Violation: an import of `internal/api/management` in package `postgres`; a check that sends a command.
- Risk: the database stops building without the health entry point; a check through a command fails on a business rule and holds traffic back for nothing.
- Fix: `Ping` runs `SELECT 1` through the same translation, with the shape `func(context.Context) error`.

### Run the shared scenarios
Name the shared scenarios in [conformance.copy](./conformance.copy), run `make conformance-sync` at the repository root, and implement their steps in `internal/infrastructure/postgres/test`, each comparing the fields of the row objects.
- Violation: a copy edited; a step comparing titles joined into one text.
- Risk: the contract is proved in another stack and not in this one.
- Fix: the sample tables, their specifications, and `TaskRepository` are in `test/world_test.go`, created by SQL for the scenario; what only Go has stays in `config-forms-in-go.feature`, `model-changes-in-go.feature`, and `statements.feature` beside the copies.

### Create the database of each scenario
Create a `postgrestest.Database` in a step or `Before` of each scenario that needs one, open it, migrate it where the scenario needs the schema, and drop it in `After`; give a service's scenario an outcome through `postgrestest.Failing`. Decision recorded in [service-scenarios-on-the-real-engine](./adr/service-scenarios-on-the-real-engine.md).
- Violation: a database shared by the scenarios of a package; a fake repository that ignores the specification.
- Risk: scenarios see each other's rows; a wrong condition passes.
- Fix: `postgrestest.Create` fails without `TEST_DATABASE_DSN`, which needs `CREATEDB`; `Drop` closes the connections still open.

# Check list
- [ ] The fixed files are filled copies of the templates; one `Database` per process.
- [ ] Every row is in `tables.Rows`, with a `TableName` and a unique index for every rule of uniqueness.
- [ ] A specification embeds `SpecificationOf` or `ProjectionOf` and uses only `Where`, `Joins` by SQL, `Preload`, and `Select`.
- [ ] A statement past the contract runs in `Run`; no `*gorm.DB` leaves package `postgres` but through `Pick` and `Run`.
- [ ] No error with a kind carries the repository's failure in its text.
- [ ] Every change of a row has its migration; the scenario of `model-changes-in-go.feature` passes.
- [ ] `cmd/migrate` is built as `/app/migrate`; `MIGRATE_ON_START` stays `false` beyond one instance.
- [ ] `make test` passes with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
One call of the example, with its real types:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the proto example with its lists in PostgreSQL: `make migrate && make run`, then the `curl` lines of its readme; stop it, run it again, and the items are still there.
