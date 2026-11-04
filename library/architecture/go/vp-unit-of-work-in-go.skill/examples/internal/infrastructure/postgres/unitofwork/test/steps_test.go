package test

import (
	"context"
	"errors"
	"fmt"
	"strings"
	"time"

	"github.com/cucumber/godog"
	"github.com/jackc/pgx/v5"
	"gorm.io/gorm"

	"github.com/example/todo/internal/infrastructure/postgres"
	"github.com/example/todo/internal/infrastructure/postgres/migrations"
	"github.com/example/todo/internal/infrastructure/postgres/postgrestest"
	"github.com/example/todo/internal/infrastructure/postgres/tables"
	"github.com/example/todo/internal/infrastructure/postgres/unitofwork"
)

func logf(format string, args ...any) { fmt.Printf(format+"\n", args...) }

// list is the list every scenario writes to.
const list = "home"

// world holds one scenario: a migrated database of its own with the unit of
// work attached, the repository of the items over it, and the context of the
// command, which carries the unit once it has begun.
type world struct {
	database  *postgrestest.Database
	db        *postgres.Database
	units     *unitofwork.UnitOfWork
	items     *postgres.Repository[tables.ItemRow]
	command   context.Context
	leave     context.CancelFunc
	rows      []tables.ItemRow
	err       error
	commitErr error
	outermost bool
}

func registerSteps(sc *godog.ScenarioContext) {
	w := &world{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		command, leave := context.WithCancel(context.Background())
		*w = world{command: command, leave: leave}
		database, err := postgrestest.Create(ctx)
		if err != nil {
			return ctx, err
		}
		w.database = database
		if w.db, err = postgres.Open(database.Config); err != nil {
			return ctx, err
		}
		if err := postgres.Migrate(ctx, w.db, migrations.Files); err != nil {
			return ctx, err
		}
		return ctx, nil
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		if w.units != nil {
			w.units.Rollback(w.command)
		}
		if w.db != nil {
			_ = w.db.Close()
		}
		if w.database != nil {
			return ctx, w.database.Drop(ctx)
		}
		return ctx, nil
	})

	sc.Step(`^a command scope with the unit of work$`, func() error {
		var err error
		if w.units, err = unitofwork.Attach(w.db); err != nil {
			return err
		}
		w.items = postgres.NewRepository[tables.ItemRow](w.db)
		return nil
	})
	sc.Step(`^the unit of work has begun$`, func() error {
		w.command, w.outermost = w.units.Begin(w.command)
		if !w.outermost {
			return errors.New("the unit of work had begun already")
		}
		return nil
	})
	sc.Step(`^the list already holds "([^"]*)"$`, func(ctx context.Context, title string) error {
		return w.other(ctx, func(conn *pgx.Conn) error {
			_, err := conn.Exec(ctx, "INSERT INTO list_items (list_id, title, done) VALUES ($1, $2, false)", list, title)
			return err
		})
	})
	sc.Step(`^the command stores "([^"]*)"$`, func(title string) error {
		w.err = w.items.Insert(w.command, &tables.ItemRow{ListID: list, Title: title})
		logf("when: Insert(%q) -> err=%v", title, w.err)
		return nil
	})
	sc.Step(`^the command has stored "([^"]*)"$`, func(title string) error {
		return w.items.Insert(w.command, &tables.ItemRow{ListID: list, Title: title})
	})
	sc.Step(`^the command reads the list$`, func() error {
		w.rows, w.err = w.items.SelectBy(tables.ItemsOfList{ListID: list}).OrderBy(tables.ItemPosition).All(w.command)
		logf("when: All -> %d rows err=%v", len(w.rows), w.err)
		return w.err
	})
	sc.Step(`^the command marks the rows of the list as done$`, func() error {
		changed, err := w.items.SelectBy(tables.ItemsOfList{ListID: list}).Update(w.command, func(row *tables.ItemRow) { row.Done = true })
		logf("when: Update -> %d rows err=%v", changed, err)
		return err
	})
	sc.Step(`^the command deletes the rows of the list$`, func() error {
		deleted, err := w.items.SelectBy(tables.ItemsOfList{ListID: list}).Delete(w.command)
		logf("when: Delete -> %d rows err=%v", deleted, err)
		return err
	})
	sc.Step(`^the command renames "([^"]*)" to "([^"]*)" in one statement$`, func(from, to string) error {
		_, w.err = w.items.Run(w.command, "rename in list_items", func(session *gorm.DB) *gorm.DB {
			return session.Exec("UPDATE list_items SET title = ? WHERE list_id = ? AND title = ?", to, list, from)
		})
		logf("when: Run(rename %q to %q) -> err=%v", from, to, w.err)
		return nil
	})
	sc.Step(`^the unit of work commits$`, func() error {
		w.commitErr = w.units.Commit(w.command)
		logf("when: Commit -> err=%v", w.commitErr)
		return nil
	})
	sc.Step(`^the unit of work rolls back$`, func() error {
		w.units.Rollback(w.command)
		return nil
	})
	sc.Step(`^a nested command asks to begin$`, func() error {
		_, w.outermost = w.units.Begin(w.command)
		return nil
	})
	sc.Step(`^the database drops the connection of the command$`, func(ctx context.Context) error {
		// The backend of the unit's transaction: a read of the command runs
		// there once the transaction is open.
		var pid int
		if _, err := w.items.Run(w.command, "backend", func(session *gorm.DB) *gorm.DB {
			return session.Raw("SELECT pg_backend_pid()").Scan(&pid)
		}); err != nil {
			return err
		}
		return w.other(ctx, func(conn *pgx.Conn) error {
			if _, err := conn.Exec(ctx, "SELECT pg_terminate_backend($1)", pid); err != nil {
				return err
			}
			for range 50 {
				var alive bool
				if err := conn.QueryRow(ctx, "SELECT EXISTS (SELECT 1 FROM pg_stat_activity WHERE pid = $1)", pid).Scan(&alive); err != nil || !alive {
					return err
				}
				time.Sleep(100 * time.Millisecond)
			}
			return fmt.Errorf("the backend %d is still there", pid)
		})
	})

	sc.Step(`^the caller of the command leaves$`, func() error {
		w.leave()
		return nil
	})
	sc.Step(`^the command renames every row of the list to "([^"]*)"$`, func(title string) error {
		changed, err := w.items.SelectBy(tables.ItemsOfList{ListID: list}).Update(w.command, func(row *tables.ItemRow) { row.Title = title })
		w.err = err
		logf("when: Update(title = %q) -> %d rows err=%v", title, changed, err)
		return nil
	})
	sc.Step(`^the command marks the row "([^"]*)" as done$`, func(ctx context.Context, title string) error {
		// The row is read outside the unit, so the update is the unit's first write.
		row, err := w.items.SelectBy(tables.ItemsOfList{ListID: list}).Single(ctx)
		if err != nil || row.Title != title {
			return fmt.Errorf("the row %q: %v, %v", title, row, err)
		}
		row.Done = true
		return w.items.Update(w.command, row)
	})
	sc.Step(`^the database of the command does not answer$`, func() error {
		down, err := postgres.Open(postgrestest.NotAnswering())
		if err != nil {
			return err
		}
		_ = w.db.Close()
		w.db = down
		if w.units, err = unitofwork.Attach(w.db); err != nil {
			return err
		}
		w.items = postgres.NewRepository[tables.ItemRow](w.db)
		return nil
	})
	sc.Step(`^the command should have been told the database is unavailable$`, func() error {
		if !errors.Is(w.err, postgres.ErrUnavailable) {
			return fmt.Errorf("the command was told %v, want unavailable", w.err)
		}
		return nil
	})
	sc.Step(`^the commit should have succeeded$`, func() error {
		return w.commitErr
	})
	sc.Step(`^the command should have been refused: its unit has ended$`, func() error {
		if w.err == nil || !strings.Contains(w.err.Error(), "has ended its unit") {
			return fmt.Errorf("the command was told %v, want the refusal of an ended unit", w.err)
		}
		return nil
	})

	sc.Step(`^the table should hold (\d+) rows for any other connection$`, func(ctx context.Context, want int) error {
		return w.expectCount(ctx, "SELECT count(*) FROM list_items", want)
	})
	sc.Step(`^the table should hold (\d+) done rows for any other connection$`, func(ctx context.Context, want int) error {
		return w.expectCount(ctx, "SELECT count(*) FROM list_items WHERE done", want)
	})
	sc.Step(`^the table should hold no row titled "([^"]*)" for any other connection$`, func(ctx context.Context, title string) error {
		return w.expectCount(ctx, fmt.Sprintf("SELECT count(*) FROM list_items WHERE title = '%s'", title), 0)
	})
	sc.Step(`^the command should see the rows:$`, func(table *godog.Table) error {
		header := table.Rows[0].Cells
		if len(w.rows) != len(table.Rows)-1 {
			return fmt.Errorf("the command saw %d rows, want %d", len(w.rows), len(table.Rows)-1)
		}
		for i, row := range w.rows {
			got := map[string]string{"title": row.Title, "done": fmt.Sprint(row.Done)}
			for j, cell := range table.Rows[i+1].Cells {
				if got[header[j].Value] != cell.Value {
					return fmt.Errorf("row %d: %s = %q, want %q", i, header[j].Value, got[header[j].Value], cell.Value)
				}
			}
		}
		return nil
	})
	sc.Step(`^the command should have been told a duplicate key$`, func() error {
		if !errors.Is(w.err, postgres.ErrDuplicateKey) {
			return fmt.Errorf("the command was told %v, want a duplicate key", w.err)
		}
		return nil
	})
	sc.Step(`^the command should hold no transaction$`, func(ctx context.Context) error {
		// A connection of the pool idle in a transaction is the command's.
		return w.expectCount(ctx, fmt.Sprintf(
			"SELECT count(*) FROM pg_stat_activity WHERE datname = '%s' AND state LIKE 'idle in transaction%%'", w.database.Name), 0)
	})
	sc.Step(`^the nested command should have joined$`, func() error {
		if w.outermost {
			return errors.New("the nested command began a unit of its own")
		}
		return nil
	})
	sc.Step(`^the commit should fail as unavailable$`, func() error {
		if !errors.Is(w.commitErr, postgres.ErrUnavailable) {
			return fmt.Errorf("the commit answered %v, want unavailable", w.commitErr)
		}
		return nil
	})
}

// other runs f on a connection of its own, outside the command's unit.
func (w *world) other(ctx context.Context, f func(conn *pgx.Conn) error) error {
	conn, err := pgx.Connect(ctx, w.database.DSN)
	if err != nil {
		return err
	}
	defer conn.Close(ctx)
	return f(conn)
}

func (w *world) expectCount(ctx context.Context, query string, want int) error {
	return w.other(ctx, func(conn *pgx.Conn) error {
		var got int
		if err := conn.QueryRow(ctx, query).Scan(&got); err != nil {
			return err
		}
		if got != want {
			return fmt.Errorf("%s: %d, want %d", query, got, want)
		}
		return nil
	})
}
