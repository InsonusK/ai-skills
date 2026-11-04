package test

import (
	"context"
	"errors"
	"fmt"
	"log/slog"
	"os"
	"reflect"
	"strings"

	"github.com/cucumber/godog"
	"github.com/jackc/pgx/v5"

	"github.com/example/todo/internal/infrastructure/postgres"
	"github.com/example/todo/internal/infrastructure/postgres/migrations"
	"github.com/example/todo/internal/infrastructure/postgres/postgrestest"
	"github.com/example/todo/internal/infrastructure/postgres/tables"
)

// outcomes are the repository's outcomes the scenarios name.
var outcomes = map[string]error{
	"a duplicate key":             postgres.ErrDuplicateKey,
	"a missing reference":         postgres.ErrMissingReference,
	"a changed row":               postgres.ErrRowChanged,
	"the database is unavailable": postgres.ErrUnavailable,
}

func registerSteps(sc *godog.ScenarioContext) {
	w := &world{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = world{env: map[string]*string{}}
		// The settings scenarios begin with neither variable set.
		for _, name := range []string{"DATABASE_DSN", "MIGRATE_ON_START"} {
			w.setenv(name, "")
			os.Unsetenv(name)
		}
		return ctx, nil
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		w.restoreEnv()
		if w.db != nil {
			_ = w.db.Close()
		}
		if w.database != nil {
			return ctx, w.database.Drop(ctx)
		}
		return ctx, nil
	})
	registerDatabaseSteps(sc, w)
	registerSelectorSteps(sc, w)
	registerAnswerSteps(sc, w)
	registerSettingsSteps(sc, w)
	registerModelSteps(sc, w)
}

// registerDatabaseSteps: the database, its rows, and what another connection
// sees.
func registerDatabaseSteps(sc *godog.ScenarioContext, w *world) {
	sc.Step(`^a database with the sample tables$`, func(ctx context.Context) error {
		if err := w.create(ctx); err != nil {
			return err
		}
		return w.other(ctx, func(conn *pgx.Conn) error {
			_, err := conn.Exec(ctx, sampleTables)
			return err
		})
	})
	sc.Step(`^an empty database$`, func(ctx context.Context) error { return w.create(ctx) })
	sc.Step(`^a migrated database$`, func(ctx context.Context) error {
		if err := w.create(ctx); err != nil {
			return err
		}
		return postgres.Migrate(ctx, w.db, migrations.Files)
	})
	sc.Step(`^a migrated database with (\d+) rows in "([^"]*)"$`, func(ctx context.Context, n int, table string) error {
		if err := w.create(ctx); err != nil {
			return err
		}
		if err := postgres.Migrate(ctx, w.db, migrations.Files); err != nil {
			return err
		}
		items := postgres.NewRepository[tables.ItemRow](w.db)
		for i := range n {
			if err := items.Insert(ctx, &tables.ItemRow{ListID: "home", Title: fmt.Sprintf("item %d", i)}); err != nil {
				return err
			}
		}
		return nil
	})
	sc.Step(`^a database that does not answer$`, func() error {
		return w.open(postgrestest.NotAnswering())
	})
	sc.Step(`^one session for the whole scenario$`, func() error {
		// database/sql's pool holds the sessions: with one connection every
		// statement of the scenario runs on the same one.
		pool, err := w.db.SQL()
		if err != nil {
			return err
		}
		pool.SetMaxOpenConns(1)
		return nil
	})
	sc.Step(`^the owner "([^"]*)"$`, func(ctx context.Context, name string) error {
		return w.owners.Insert(ctx, &OwnerRow{Name: name})
	})
	task := func(done bool) func(context.Context, string, string) error {
		return func(ctx context.Context, title, owner string) error {
			id, err := w.ownerID(ctx, owner)
			if err != nil {
				return err
			}
			return w.tasks.Insert(ctx, &TaskRow{OwnerID: id, Title: title, Done: done})
		}
	}
	sc.Step(`^the task "([^"]*)" of "([^"]*)" is open$`, task(false))
	sc.Step(`^the task "([^"]*)" of "([^"]*)" is done$`, task(true))
	sc.Step(`^another connection adds the open task "([^"]*)" of "([^"]*)"$`, func(ctx context.Context, title, owner string) error {
		return w.other(ctx, func(conn *pgx.Conn) error {
			_, err := conn.Exec(ctx, "INSERT INTO tasks (owner_id, title, done) SELECT id, $1, false FROM owners WHERE name = $2", title, owner)
			return err
		})
	})
	sc.Step(`^another connection deletes the task "([^"]*)"$`, func(ctx context.Context, title string) error {
		return w.other(ctx, func(conn *pgx.Conn) error {
			_, err := conn.Exec(ctx, "DELETE FROM tasks WHERE title = $1", title)
			return err
		})
	})
	sc.Step(`^the table "([^"]*)" should hold (\d+) rows for any other connection$`, func(ctx context.Context, table string, want int) error {
		return w.expectCount(ctx, "SELECT count(*) FROM "+pgx.Identifier{table}.Sanitize(), want)
	})
	sc.Step(`^the table "([^"]*)" should hold (\d+) done rows for any other connection$`, func(ctx context.Context, table string, want int) error {
		return w.expectCount(ctx, "SELECT count(*) FROM "+pgx.Identifier{table}.Sanitize()+" WHERE done", want)
	})
	sc.Step(`^the task "([^"]*)" should be done for any other connection$`, func(ctx context.Context, title string) error {
		return w.expectCount(ctx, fmt.Sprintf("SELECT count(*) FROM tasks WHERE done AND title = '%s'", title), 1)
	})
	sc.Step(`^the table "([^"]*)" should exist with (\d+) rows$`, func(ctx context.Context, table string, want int) error {
		if w.err != nil {
			return fmt.Errorf("the migrations failed: %w", w.err)
		}
		return w.expectCount(ctx, "SELECT count(*) FROM "+pgx.Identifier{table}.Sanitize(), want)
	})
	sc.Step(`^no transaction should be open$`, func(ctx context.Context) error {
		if w.err != nil {
			return w.err
		}
		// A session of the pool idle in a transaction would hold the write
		// uncommitted.
		return w.expectCount(ctx, fmt.Sprintf(
			"SELECT count(*) FROM pg_stat_activity WHERE datname = '%s' AND state LIKE 'idle in transaction%%'", w.database.Name), 0)
	})
	sc.Step(`^the migrations are applied$`, func(ctx context.Context) error {
		w.err = postgres.Migrate(ctx, w.db, migrations.Files)
		logf("when: Migrate -> err=%v", w.err)
		return nil
	})
	sc.Step(`^the readiness check runs$`, func(ctx context.Context) error {
		w.err = w.db.Ping(ctx)
		logf("when: Ping -> err=%v", w.err)
		return nil
	})
	sc.Step(`^the readiness check should be "(healthy|unhealthy)"$`, func(want string) error {
		if (want == "healthy") != (w.err == nil) {
			return fmt.Errorf("readiness check answered %v, want %s", w.err, want)
		}
		return nil
	})
}

func (w *world) expectCount(ctx context.Context, query string, want int) error {
	return w.other(ctx, func(conn *pgx.Conn) error {
		var got int
		if err := conn.QueryRow(ctx, query).Scan(&got); err != nil {
			return err
		}
		if got != want {
			return fmt.Errorf("%s answered %d, want %d", query, got, want)
		}
		return nil
	})
}

// registerSelectorSteps: the calls of the repository and of its selectors.
func registerSelectorSteps(sc *godog.ScenarioContext, w *world) {
	selectBy := func(specification postgres.Specification[TaskRow]) {
		w.selector = w.tasks.SelectBy(specification)
	}
	sc.Step(`^I select the unclosed tasks of "([^"]*)"$`, func(owner string) { selectBy(UnclosedTasks{Owner: owner}) })
	sc.Step(`^I select the unclosed tasks of "([^"]*)" ordered by title$`, func(owner string) {
		w.ordered = w.tasks.SelectBy(UnclosedTasks{Owner: owner}).OrderBy(TaskTitle)
		w.selector = w.ordered
	})
	sc.Step(`^I select the unclosed tasks of "([^"]*)" ordered by title descending$`, func(owner string) {
		w.ordered = w.tasks.SelectBy(UnclosedTasks{Owner: owner}).OrderByDescending(TaskTitle)
		w.selector = w.ordered
	})
	sc.Step(`^I select every open task ordered by owner, then by title$`, func() {
		w.ordered = w.tasks.SelectBy(OpenTasks{}).OrderBy(TaskOwner).ThenBy(TaskTitle)
		w.selector = w.ordered
	})
	sc.Step(`^I select the task "([^"]*)" by its id$`, func(ctx context.Context, title string) error {
		row, err := w.tasks.SelectBy(TaskByTitle{Title: title}).Single(ctx)
		if err != nil || row == nil {
			return fmt.Errorf("no task %q: %v", title, err)
		}
		selectBy(TaskByID{ID: row.ID})
		return nil
	})
	sc.Step(`^I select a task by an id nobody has$`, func() { selectBy(TaskByID{ID: -1}) })
	sc.Step(`^I select the titles with their owners of "([^"]*)"$`, func(owner string) {
		w.results = w.titles.SelectBy(TitlesWithOwners{Owner: owner})
	})
	sc.Step(`^I select the titles with their owners of "([^"]*)" ordered by title descending$`, func(owner string) {
		w.ordering = w.titles.SelectBy(TitlesWithOwners{Owner: owner}).OrderByDescending(ResultTitle)
		w.results = w.ordering
	})
	sc.Step(`^I select through a specification that holds a page$`, func() { selectBy(WithPage{}) })
	sc.Step(`^I select through a specification that holds an order$`, func() { selectBy(WithOrder{}) })
	sc.Step(`^I give one specification of "([^"]*)" to two selectors$`, func(owner string) {
		specification := UnclosedTasks{Owner: owner}
		w.selector = w.tasks.SelectBy(specification)
		w.second = w.tasks.SelectBy(specification)
	})
	sc.Step(`^I order that selector by title and, apart, by title descending$`, func() {
		w.apart = [2]postgres.OrderedSelector[TaskRow]{w.selector.OrderBy(TaskTitle), w.selector.OrderByDescending(TaskTitle)}
	})
	sc.Step(`^I count the unclosed tasks of "([^"]*)"$`, func(ctx context.Context, owner string) {
		_, w.err = w.tasks.SelectBy(UnclosedTasks{Owner: owner}).Count(ctx)
	})
	sc.Step(`^I update them as done$`, func(ctx context.Context) {
		w.written, w.err = w.selector.Update(ctx, func(row *TaskRow) { row.Done = true })
		logf("when: Update -> %d err=%v", w.written, w.err)
	})
	sc.Step(`^I update them as done, renaming "([^"]*)" to "([^"]*)"$`, func(ctx context.Context, from, to string) {
		w.written, w.err = w.selector.Update(ctx, func(row *TaskRow) {
			row.Done = true
			if row.Title == from {
				row.Title = to
			}
		})
		logf("when: Update -> %d err=%v", w.written, w.err)
	})
	sc.Step(`^I delete them$`, func(ctx context.Context) {
		w.written, w.err = w.selector.Delete(ctx)
		logf("when: Delete -> %d err=%v", w.written, w.err)
	})
	sc.Step(`^I insert the task "([^"]*)" of "([^"]*)"$`, func(ctx context.Context, title, owner string) {
		id, err := w.ownerID(ctx, owner)
		if err != nil {
			w.err = err
			return
		}
		w.err = w.tasks.Insert(ctx, &TaskRow{OwnerID: id, Title: title})
		logf("when: Insert(%q) -> err=%v", title, w.err)
	})
	sc.Step(`^I insert the task "([^"]*)" of an owner nobody has$`, func(ctx context.Context, title string) {
		w.err = w.tasks.Insert(ctx, &TaskRow{OwnerID: -1, Title: title})
		logf("when: Insert(%q) -> err=%v", title, w.err)
	})
	read := func(ctx context.Context, title string) error {
		row, err := w.tasks.SelectBy(TaskByTitle{Title: title}).Single(ctx)
		if err != nil || row == nil {
			return fmt.Errorf("no task %q: %v", title, err)
		}
		w.row = row
		return nil
	}
	sc.Step(`^I read the task "([^"]*)"$`, read)
	sc.Step(`^I read the task "([^"]*)" and update it as done$`, func(ctx context.Context, title string) error {
		if err := read(ctx, title); err != nil {
			return err
		}
		w.row.Done = true
		w.err = w.tasks.Update(ctx, w.row)
		return nil
	})
	sc.Step(`^I read the task "([^"]*)" and rename it "([^"]*)"$`, func(ctx context.Context, title, to string) error {
		if err := read(ctx, title); err != nil {
			return err
		}
		w.row.Title = to
		w.err = w.tasks.Update(ctx, w.row)
		return nil
	})
	sc.Step(`^I read the task "([^"]*)" and delete it$`, func(ctx context.Context, title string) error {
		if err := read(ctx, title); err != nil {
			return err
		}
		w.err = w.tasks.Delete(ctx, w.row)
		return nil
	})
	sc.Step(`^I delete that row$`, func(ctx context.Context) {
		w.err = w.tasks.Delete(ctx, w.row)
		logf("when: Delete(row) -> err=%v", w.err)
	})
	sc.Step(`^the database answers the failure "([^"]*)"$`, func(ctx context.Context, state string) {
		w.err = w.tasks.FailWith(ctx, state)
		logf("when: FailWith(%s) -> err=%v", state, w.err)
	})
	// The stack's own scenarios of a statement past the contract.
	sc.Step(`^I insert the task "([^"]*)" of "([^"]*)" unless it is there$`, func(ctx context.Context, title, owner string) error {
		id, err := w.ownerID(ctx, owner)
		if err != nil {
			return err
		}
		return w.tasks.InsertUnlessThere(ctx, TaskRow{OwnerID: id, Title: title})
	})
	sc.Step(`^I rename the task "([^"]*)" to "([^"]*)" in one statement$`, func(ctx context.Context, from, to string) {
		_, w.err = w.tasks.Rename(ctx, from, to)
	})
}

// registerAnswerSteps: what a call answered.
func registerAnswerSteps(sc *godog.ScenarioContext, w *world) {
	sc.Step(`^All should answer the rows:$`, func(ctx context.Context, table *godog.Table) error {
		if w.results != nil {
			return expectRows(w.results.All(ctx))(table)
		}
		return expectRows(w.selector.All(ctx))(table)
	})
	sc.Step(`^All should answer an empty list$`, func(ctx context.Context) error {
		return expectEmpty(w.selector.All(ctx))
	})
	sc.Step(`^Count should answer (\d+)$`, func(ctx context.Context, want int64) error {
		got, err := w.selector.Count(ctx)
		if err != nil || got != want {
			return fmt.Errorf("Count answered %d, %v; want %d", got, err, want)
		}
		return nil
	})
	sc.Step(`^Any should answer (true|false)$`, func(ctx context.Context, want string) error {
		got, err := w.selector.Any(ctx)
		if err != nil || fmt.Sprint(got) != want {
			return fmt.Errorf("Any answered %v, %v; want %s", got, err, want)
		}
		return nil
	})
	sc.Step(`^Single should answer one row:$`, func(ctx context.Context, table *godog.Table) error {
		return expectOne(w.selector.Single(ctx))(table)
	})
	sc.Step(`^Single should answer no row$`, func(ctx context.Context) error {
		return expectNone(w.selector.Single(ctx))
	})
	sc.Step(`^Single should fail with no outcome of the repository$`, func(ctx context.Context) error {
		row, err := w.selector.Single(ctx)
		return expectNoOutcome(row, err)
	})
	sc.Step(`^First should answer one row:$`, func(ctx context.Context, table *godog.Table) error {
		if w.ordering != nil {
			return expectOne(w.ordering.First(ctx))(table)
		}
		return expectOne(w.ordered.First(ctx))(table)
	})
	sc.Step(`^First should answer no row$`, func(ctx context.Context) error {
		return expectNone(w.ordered.First(ctx))
	})
	sc.Step(`^First of the (first|second) order should answer one row:$`, func(ctx context.Context, which string, table *godog.Table) error {
		index := map[string]int{"first": 0, "second": 1}[which]
		return expectOne(w.apart[index].First(ctx))(table)
	})
	sc.Step(`^Take (-?\d+) should answer the rows:$`, func(ctx context.Context, n int, table *godog.Table) error {
		if w.ordering != nil {
			return expectRows(w.ordering.Take(ctx, n))(table)
		}
		return expectRows(w.ordered.Take(ctx, n))(table)
	})
	sc.Step(`^Take (-?\d+) should be refused$`, func(ctx context.Context, n int) error {
		_, err := w.ordered.Take(ctx, n)
		return expectRefused(err)
	})
	sc.Step(`^Page (-?\d+) of size (\d+) should answer the rows:$`, func(ctx context.Context, page, size int, table *godog.Table) error {
		return expectRows(w.ordered.Page(ctx, page, size))(table)
	})
	sc.Step(`^Page (-?\d+) of size (\d+) should answer an empty list$`, func(ctx context.Context, page, size int) error {
		return expectEmpty(w.ordered.Page(ctx, page, size))
	})
	sc.Step(`^Page (-?\d+) of size (\d+) should be refused$`, func(ctx context.Context, page, size int) error {
		_, err := w.ordered.Page(ctx, page, size)
		return expectRefused(err)
	})
	sc.Step(`^the selection should be refused$`, func(ctx context.Context) error {
		_, err := w.selector.All(ctx)
		return expectRefused(err)
	})
	sc.Step(`^the first should count (\d+)$`, func(ctx context.Context, want int64) error {
		got, err := w.selector.Count(ctx)
		if err != nil || got != want {
			return fmt.Errorf("the first counted %d, %v; want %d", got, err, want)
		}
		return nil
	})
	sc.Step(`^the second should delete (\d+)$`, func(ctx context.Context, want int64) error {
		got, err := w.second.Delete(ctx)
		if err != nil || got != want {
			return fmt.Errorf("the second deleted %d, %v; want %d", got, err, want)
		}
		return nil
	})
	written := func(want int64) error {
		if w.err != nil || w.written != want {
			return fmt.Errorf("the write answered %d, %v; want %d", w.written, w.err, want)
		}
		return nil
	}
	sc.Step(`^the update should answer (\d+)$`, written)
	sc.Step(`^the delete should answer (\d+)$`, written)
	sc.Step(`^no failure should be reported$`, func() error { return w.err })
	sc.Step(`^the repository should report (a duplicate key|a missing reference|a changed row|the database is unavailable)$`, func(outcome string) error {
		if !errors.Is(w.err, outcomes[outcome]) {
			return fmt.Errorf("the repository reported %v, want %s", w.err, outcome)
		}
		return nil
	})
	sc.Step(`^the failure should reach the caller with no outcome of the repository$`, func() error {
		return expectNoOutcome[TaskRow](nil, w.err)
	})
}

// registerSettingsSteps: DATABASE_DSN and MIGRATE_ON_START.
func registerSettingsSteps(sc *godog.ScenarioContext, w *world) {
	sc.Step(`^a usable DATABASE_DSN with the password "([^"]*)"$`, func(password string) {
		w.setenv("DATABASE_DSN", "postgres://todo:"+password+"@db:5432/todo")
	})
	sc.Step(`^an unusable DATABASE_DSN holding "([^"]*)"$`, func(secret string) {
		w.setenv("DATABASE_DSN", "postgres://todo:"+secret+"@db:no-port/todo")
	})
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, func(name, value string) {
		w.setenv(name, value)
	})
	sc.Step(`^the settings are loaded$`, func() {
		w.config, w.err = postgres.LoadConfig()
		logf("when: LoadConfig -> %v err=%v", w.config, w.err)
	})
	sc.Step(`^the application should migrate on start: "(true|false)"$`, func(want string) error {
		if w.err != nil || fmt.Sprint(w.config.MigrateOnStart) != want {
			return fmt.Errorf("MigrateOnStart %v, %v; want %s", w.config.MigrateOnStart, w.err, want)
		}
		return nil
	})
	sc.Step(`^the settings printed should not mention "([^"]*)"$`, func(secret string) error {
		var logged strings.Builder
		slog.New(slog.NewJSONHandler(&logged, nil)).Info("settings", "database", w.config)
		for _, printed := range []string{fmt.Sprint(w.config), fmt.Sprintf("%+v", w.config), fmt.Sprintf("%#v", w.config), logged.String()} {
			if strings.Contains(printed, secret) {
				return fmt.Errorf("printed settings %q mention %q", printed, secret)
			}
		}
		return nil
	})
	sc.Step(`^loading should fail mentioning "([^"]*)"$`, func(text string) error {
		if w.err == nil || !strings.Contains(w.err.Error(), text) {
			return fmt.Errorf("loading answered %v, want a failure mentioning %q", w.err, text)
		}
		return nil
	})
	sc.Step(`^the failure should not mention "([^"]*)"$`, func(secret string) error {
		if w.err == nil || strings.Contains(w.err.Error(), secret) {
			return fmt.Errorf("failure %v mentions %q", w.err, secret)
		}
		return nil
	})
	// The stack's own scenarios of the forms of a DSN.
	sc.Step(`^the DSN should be "([^"]*)"$`, func(want string) error {
		if w.err != nil || w.config.DSN() != want {
			return fmt.Errorf("the DSN read is another, or loading failed: %v", w.err)
		}
		return nil
	})
}

// expectRows compares a list of row objects with the table, field by field
// of the names its header gives, in order.
func expectRows[T any](rows []T, err error) func(*godog.Table) error {
	return func(table *godog.Table) error {
		if err != nil {
			return err
		}
		want := len(table.Rows) - 1
		if len(rows) != want {
			return fmt.Errorf("answered %d rows %+v, want %d", len(rows), rows, want)
		}
		for i := range rows {
			if err := expectFields(rows[i], cellsOf(table, 0), cellsOf(table, i+1)); err != nil {
				return fmt.Errorf("row %d: %w", i+1, err)
			}
		}
		return nil
	}
}

func expectOne[T any](row *T, err error) func(*godog.Table) error {
	return func(table *godog.Table) error {
		if err != nil {
			return err
		}
		if row == nil {
			return errors.New("answered no row, want one")
		}
		return expectFields(*row, cellsOf(table, 0), cellsOf(table, 1))
	}
}

// expectFields reads each field the header names from the row object,
// never from a text of the whole row.
// cellsOf is the values of row i of table, the header being row 0.
func cellsOf(table *godog.Table, i int) []string {
	values := []string{}
	for _, cell := range table.Rows[i].Cells {
		values = append(values, cell.Value)
	}
	return values
}

func expectFields(row any, header, want []string) error {
	value := reflect.ValueOf(row)
	for i, cell := range header {
		name := cell
		field := value.FieldByNameFunc(func(field string) bool { return strings.EqualFold(field, name) })
		if !field.IsValid() {
			return fmt.Errorf("the row %T has no field %q", row, name)
		}
		if got := fmt.Sprint(field.Interface()); got != want[i] {
			return fmt.Errorf("%s is %q, want %q", name, got, want[i])
		}
	}
	return nil
}

func expectEmpty[T any](rows []T, err error) error {
	if err != nil {
		return err
	}
	if rows == nil || len(rows) != 0 {
		return fmt.Errorf("answered %+v, want an empty list", rows)
	}
	return nil
}

func expectNone[T any](row *T, err error) error {
	if err != nil {
		return err
	}
	if row != nil {
		return fmt.Errorf("answered %+v, want no row", *row)
	}
	return nil
}

func expectRefused(err error) error {
	if !errors.Is(err, postgres.ErrRefused) {
		return fmt.Errorf("answered %v, want a refusal", err)
	}
	return nil
}

func expectNoOutcome[T any](row *T, err error) error {
	if err == nil {
		return fmt.Errorf("answered %v, want a failure", row)
	}
	for name, outcome := range outcomes {
		if errors.Is(err, outcome) {
			return fmt.Errorf("the failure %v is %s, want no outcome", err, name)
		}
	}
	return nil
}
