package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/domain/entity"
	"github.com/example/todo/internal/infrastructure/memory"
	"github.com/example/todo/internal/service"
)

func logf(format string, args ...any) { fmt.Printf(format+"\n", args...) }

// fakeStore is the infrastructure below the service, faked.
type fakeStore struct {
	rows map[string][]memory.Row
	err  error
}

func (f *fakeStore) Insert(_ context.Context, listID string, row memory.Row) error {
	if f.err != nil {
		return f.err
	}
	f.rows[listID] = append(f.rows[listID], row)
	return nil
}

func (f *fakeStore) Select(_ context.Context, listID string) ([]memory.Row, error) {
	return f.rows[listID], f.err
}

type world struct {
	store *fakeStore
	items []entity.Item
	err   error
}

var kinds = map[string]error{"conflict": pipeline.ErrConflict, "not found": pipeline.ErrNotFound}

func registerSteps(sc *godog.ScenarioContext) {
	w := &world{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = world{store: &fakeStore{rows: map[string][]memory.Row{}}}
		return ctx, nil
	})
	sc.Step(`^the store answers a duplicate key$`, func() error { w.store.err = memory.ErrDuplicateKey; return nil })
	sc.Step(`^the store answers no rows$`, func() error { w.store.err = memory.ErrNoRows; return nil })
	sc.Step(`^the store fails with "([^"]*)"$`, func(message string) error { w.store.err = errors.New(message); return nil })
	sc.Step(`^I add "([^"]*)" to the list "([^"]*)"$`, func(ctx context.Context, title, listID string) error {
		item, err := entity.NewItem(title)
		if err != nil {
			return err
		}
		w.err = service.NewItemLists(w.store).Add(ctx, listID, item)
		logf("when: Add(%q, %q) -> err=%v", listID, title, w.err)
		return nil
	})
	sc.Step(`^I read the list "([^"]*)"$`, func(ctx context.Context, listID string) error {
		w.items, w.err = service.NewItemLists(w.store).Items(ctx, listID)
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
