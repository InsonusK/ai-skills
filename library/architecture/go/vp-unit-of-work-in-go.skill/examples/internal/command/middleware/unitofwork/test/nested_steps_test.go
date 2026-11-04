package test

import (
	"context"
	"errors"
	"fmt"
	"strings"
	"time"

	"github.com/cucumber/godog"
	"github.com/jackc/pgx/v5"

	"github.com/example/todo/internal/command"
	commandunitofwork "github.com/example/todo/internal/command/middleware/unitofwork"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/infrastructure/postgres"
	"github.com/example/todo/internal/infrastructure/postgres/migrations"
	"github.com/example/todo/internal/infrastructure/postgres/postgrestest"
	"github.com/example/todo/internal/infrastructure/postgres/tables"
	"github.com/example/todo/internal/infrastructure/postgres/unitofwork"
	"github.com/example/todo/internal/service"
)

// nestedWorld holds one scenario: the command registry of the composition
// root, the unit of work its only middleware, over a migrated database of
// its own.
type nestedWorld struct {
	database *postgrestest.Database
	db       *postgres.Database
	handlers command.Handlers
	err      error
}

func registerNestedCommandSteps(sc *godog.ScenarioContext) {
	w := &nestedWorld{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = nestedWorld{}
		return ctx, nil
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		if w.db != nil {
			_ = w.db.Close()
		}
		if w.database != nil {
			return ctx, w.database.Drop(ctx)
		}
		return ctx, nil
	})

	sc.Step(`^the commands over a migrated database$`, func(ctx context.Context) error {
		database, err := postgrestest.Create(ctx)
		if err != nil {
			return err
		}
		w.database = database
		if w.db, err = postgres.Open(database.Config); err != nil {
			return err
		}
		units, err := unitofwork.Attach(w.db)
		if err != nil {
			return err
		}
		if err := postgres.Migrate(ctx, w.db, migrations.Files); err != nil {
			return err
		}
		itemLists := service.NewItemLists(postgres.NewRepository[tables.ItemRow](w.db))
		w.handlers = command.NewHandlers([]pipeline.Middleware{commandunitofwork.Middleware(units)}, itemLists)
		return nil
	})
	sc.Step(`^the pool holds at most (\d+) connections$`, func(limit int) error {
		pool, err := w.db.SQL()
		if err != nil {
			return err
		}
		pool.SetMaxOpenConns(limit)
		return nil
	})
	sc.Step(`^the list "([^"]*)" already holds "([^"]*)"$`, func(ctx context.Context, listID, title string) error {
		_, err := w.handlers.AddItem.Handle(ctx, command.AddItemCommand{ListID: listID, Title: title})
		return err
	})
	sc.Step(`^I import "([^"]*)" into the list "([^"]*)"$`, func(ctx context.Context, titles, listID string) error {
		_, w.err = w.handlers.ImportItems.Handle(ctx, command.ImportItemsCommand{ListID: listID, Titles: strings.Split(titles, "|")})
		logf("when: ImportItems(%q, %q) -> err=%v", listID, titles, w.err)
		return nil
	})
	sc.Step(`^(\d+) commands run one after another$`, func(ctx context.Context, count int) error {
		for i := range count {
			// A unit that kept its connection would leave the next command
			// waiting for one: the deadline ends the wait.
			bounded, cancel := context.WithTimeout(ctx, 5*time.Second)
			_, err := w.handlers.AddItem.Handle(bounded, command.AddItemCommand{ListID: "home", Title: fmt.Sprintf("item %d", i)})
			cancel()
			if err != nil {
				return fmt.Errorf("command %d: %w", i, err)
			}
		}
		return nil
	})

	sc.Step(`^the list "([^"]*)" should hold exactly "([^"]*)"$`, func(ctx context.Context, listID, titles string) error {
		listed, err := w.handlers.ListItems.Handle(ctx, command.ListItemsCommand{ListID: listID})
		if err != nil {
			return err
		}
		got := []string{}
		for _, item := range listed.Items {
			got = append(got, item.Title)
		}
		if strings.Join(got, "|") != titles {
			return fmt.Errorf("the list holds %q, want %q", got, titles)
		}
		return nil
	})
	sc.Step(`^the import should have failed as a conflict$`, func() error {
		if !errors.Is(w.err, pipeline.ErrConflict) {
			return fmt.Errorf("the import answered %v, want a conflict", w.err)
		}
		return nil
	})
	sc.Step(`^the database should hold (\d+) items$`, func(ctx context.Context, want int) error {
		return w.expectCount(ctx, "SELECT count(*) FROM list_items", want)
	})
	sc.Step(`^no transaction should be open$`, func(ctx context.Context) error {
		return w.expectCount(ctx, fmt.Sprintf(
			"SELECT count(*) FROM pg_stat_activity WHERE datname = '%s' AND state LIKE 'idle in transaction%%'", w.database.Name), 0)
	})
}

func (w *nestedWorld) expectCount(ctx context.Context, query string, want int) error {
	conn, err := pgx.Connect(ctx, w.database.DSN)
	if err != nil {
		return err
	}
	defer conn.Close(ctx)
	var got int
	if err := conn.QueryRow(ctx, query).Scan(&got); err != nil {
		return err
	}
	if got != want {
		return fmt.Errorf("%s: %d, want %d", query, got, want)
	}
	return nil
}
