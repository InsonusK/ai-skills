package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/domain/entity"
	"github.com/example/todo/internal/infrastructure/postgres"
	"github.com/example/todo/internal/infrastructure/postgres/migrations"
	"github.com/example/todo/internal/infrastructure/postgres/postgrestest"
	"github.com/example/todo/internal/infrastructure/postgres/tables"
	"github.com/example/todo/internal/service"
)

func logf(format string, args ...any) { fmt.Printf(format+"\n", args...) }

// world holds one scenario: the service over a migrated database of its own,
// or over a repository that answers the failure a step chose.
type world struct {
	database *postgrestest.Database
	db       *postgres.Database
	lists    *service.ItemLists
	items    []entity.KeptItem
	// kept is the item the last add, read, or save answered.
	kept entity.KeptItem
	err  error
}

var kinds = map[string]error{
	"conflict":          pipeline.ErrConflict,
	"not found":         pipeline.ErrNotFound,
	"unavailable":       pipeline.ErrUnavailable,
	"concurrent change": pipeline.ErrConcurrentChange,
}

func registerSteps(sc *godog.ScenarioContext) {
	w := &world{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = world{}
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
		w.lists = service.NewItemLists(postgres.NewRepository[tables.ItemRow](w.db))
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
	failWith := func(err error) {
		w.lists = service.NewItemLists(postgrestest.Failing[tables.ItemRow]{Err: err})
	}
	sc.Step(`^the store answers a duplicate key$`, func() error {
		failWith(fmt.Errorf("insert into list_items: %w", postgres.ErrDuplicateKey))
		return nil
	})
	sc.Step(`^the store answers that the database is unavailable$`, func() error {
		failWith(fmt.Errorf("select list_items: %w", postgres.ErrUnavailable))
		return nil
	})
	sc.Step(`^the store fails with "([^"]*)"$`, func(message string) error {
		failWith(errors.New(message))
		return nil
	})
	sc.Step(`^I add "([^"]*)" to the list "([^"]*)"$`, func(ctx context.Context, title, listID string) error {
		item, err := entity.NewItem(title)
		if err != nil {
			return err
		}
		w.kept, w.err = w.lists.Add(ctx, listID, item)
		logf("when: Add(%q, %q) -> %+v err=%v", listID, title, w.kept, w.err)
		return nil
	})
	sc.Step(`^the added item should carry an id and a version$`, func() error {
		if w.err != nil || w.kept.ID == 0 || w.kept.Version == "" {
			return fmt.Errorf("added %+v err=%v, want an id and a version", w.kept, w.err)
		}
		return nil
	})
	sc.Step(`^I read that item$`, func(ctx context.Context) error {
		w.kept, w.err = w.lists.Item(ctx, "home", w.kept.ID)
		logf("when: Item(home, %d) -> %+v err=%v", w.kept.ID, w.kept, w.err)
		return nil
	})
	sc.Step(`^I read the item (\d+) of the list "([^"]*)"$`, func(ctx context.Context, id int64, listID string) error {
		w.kept, w.err = w.lists.Item(ctx, listID, id)
		return nil
	})
	sc.Step(`^another caller completes that item meanwhile$`, func(ctx context.Context) error {
		other, err := w.lists.Item(ctx, "home", w.kept.ID)
		if err != nil {
			return err
		}
		if err := other.Complete(); err != nil {
			return err
		}
		_, err = w.lists.Save(ctx, "home", other)
		return err
	})
	sc.Step(`^I complete it and save it under the version I read$`, func(ctx context.Context) error {
		read := w.kept
		if err := read.Complete(); err != nil {
			return err
		}
		w.kept, w.err = w.lists.Save(ctx, "home", read)
		logf("when: Save(home, %d at %q) -> %+v err=%v", read.ID, read.Version, w.kept, w.err)
		if w.err == nil && w.kept.Version == read.Version {
			return fmt.Errorf("the saved item kept the version %q it was read with", read.Version)
		}
		return nil
	})
	sc.Step(`^the item read back should be done, at the version the save answered$`, func(ctx context.Context) error {
		if w.err != nil {
			return w.err
		}
		saved := w.kept
		read, err := w.lists.Item(ctx, "home", saved.ID)
		if err != nil || !read.Done() || read.Version != saved.Version {
			return fmt.Errorf("read %+v err=%v, want done at the version %q", read, err, saved.Version)
		}
		return nil
	})
	sc.Step(`^I read the list "([^"]*)"$`, func(ctx context.Context, listID string) error {
		w.items, w.err = w.lists.Items(ctx, listID)
		logf("when: Items(%q) -> %d items err=%v", listID, len(w.items), w.err)
		return nil
	})
	sc.Step(`^the items should be "([^"]*)"$`, func(titles string) error {
		got := []string{}
		for _, item := range w.items {
			got = append(got, item.Title())
		}
		if w.err != nil || strings.Join(got, "|") != titles {
			return fmt.Errorf("items %q err=%v, want %q", got, w.err, titles)
		}
		return nil
	})
	sc.Step(`^the service should fail with the kind "([^"]*)" mentioning "([^"]*)"$`, func(kind, text string) error {
		if !errors.Is(w.err, kinds[kind]) || !strings.Contains(fmt.Sprint(w.err), text) {
			return fmt.Errorf("error %v, want the kind %q mentioning %q", w.err, kind, text)
		}
		return nil
	})
	sc.Step(`^the service should fail with no kind$`, func() error {
		if w.err == nil || pipeline.KindOf(w.err) != nil {
			return fmt.Errorf("error %v, want one with no kind", w.err)
		}
		return nil
	})
}
