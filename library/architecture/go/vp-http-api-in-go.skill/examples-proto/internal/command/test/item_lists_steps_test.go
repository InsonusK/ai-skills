package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/domain/entity"
)

// fakeLists is the item lists service, faked: it records what it was given
// and answers what the scenario set.
type fakeLists struct {
	held   []entity.Item
	err    error
	asked  bool
	listID string
	given  []string
}

func (f *fakeLists) Add(_ context.Context, listID string, item entity.Item) error {
	f.asked, f.listID = true, listID
	f.given = append(f.given, item.Title())
	return f.err
}

func (f *fakeLists) Items(_ context.Context, listID string) ([]entity.Item, error) {
	f.asked, f.listID = true, listID
	return f.held, f.err
}

// listsWorld is the state of the item-list scenarios, reset before each.
type listsWorld struct {
	lists  *fakeLists
	added  command.AddItemResult
	listed command.ListItemsResult
	err    error
}

var kinds = map[string]error{"invalid": pipeline.ErrInvalid, "conflict": pipeline.ErrConflict, "not found": pipeline.ErrNotFound}

func registerItemListsSteps(sc *godog.ScenarioContext) {
	w := &listsWorld{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = listsWorld{lists: &fakeLists{}}
		return ctx, nil
	})
	sc.Step(`^the item lists answer the kind "([^"]*)"$`, func(kind string) error {
		w.lists.err = fmt.Errorf("%w: from the item lists", kinds[kind])
		return nil
	})
	sc.Step(`^the item lists hold "([^"]*)" for the list "([^"]*)"$`, func(titles, listID string) error {
		for _, title := range strings.Split(titles, "|") {
			item, err := entity.NewItem(title)
			if err != nil {
				return err
			}
			w.lists.held = append(w.lists.held, item)
		}
		return nil
	})
	sc.Step(`^I send AddItem with the list "([^"]*)" and the title "([^"]*)"$`, func(ctx context.Context, listID, title string) error {
		w.added, w.err = command.NewAddItemHandler(w.lists).Handle(ctx, command.AddItem{ListID: listID, Title: title})
		logf("when: AddItem(%q, %q) -> %+v err=%v", listID, title, w.added, w.err)
		return nil
	})
	sc.Step(`^I send ListItems with the list "([^"]*)"$`, func(ctx context.Context, listID string) error {
		w.listed, w.err = command.NewListItemsHandler(w.lists).Handle(ctx, command.ListItems{ListID: listID})
		logf("when: ListItems(%q) -> %+v err=%v", listID, w.listed, w.err)
		return nil
	})
	sc.Step(`^the added item should be the open item "([^"]*)"$`, func(title string) error {
		if w.err != nil || w.added.Item != (command.ItemView{Title: title}) {
			return fmt.Errorf("added %+v err=%v, want the open item %q", w.added, w.err, title)
		}
		return nil
	})
	sc.Step(`^the item lists should have been given "([^"]*)" for the list "([^"]*)"$`, func(title, listID string) error {
		if w.lists.listID != listID || strings.Join(w.lists.given, "|") != title {
			return fmt.Errorf("item lists got %q for %q, want %q for %q", w.lists.given, w.lists.listID, title, listID)
		}
		return nil
	})
	sc.Step(`^the item lists should not have been asked$`, func() error {
		if w.lists.asked {
			return errors.New("the item lists were asked")
		}
		return nil
	})
	sc.Step(`^the listed items should be "([^"]*)"$`, func(titles string) error {
		got := []string{}
		for _, item := range w.listed.Items {
			got = append(got, item.Title)
		}
		if w.err != nil || strings.Join(got, "|") != titles {
			return fmt.Errorf("listed %q err=%v, want %q", got, w.err, titles)
		}
		return nil
	})
	sc.Step(`^the command should fail with the kind "([^"]*)"$`, func(kind string) error {
		if !errors.Is(w.err, kinds[kind]) {
			return fmt.Errorf("error %v, want the kind %q", w.err, kind)
		}
		return nil
	})
}
