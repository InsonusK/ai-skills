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
	"github.com/example/todo/internal/shared/errorkind"
)

// fakeLists is the item lists service, faked: it records what it was given
// and answers what the scenario set.
type fakeLists struct {
	held   []entity.KeptItem
	err    error
	asked  bool
	listID string
	given  []string
	// saveErr is what Save answers; saved is the item it was given.
	saveErr error
	saved   *entity.KeptItem
}

// keptVersion is the version the fake answers for an item it keeps or saves.
const keptVersion = "8"

func (f *fakeLists) Add(_ context.Context, listID string, item entity.Item) (entity.KeptItem, error) {
	f.asked, f.listID = true, listID
	f.given = append(f.given, item.Title())
	return entity.KeptItem{ID: int64(len(f.given)), Version: keptVersion, Item: item}, f.err
}

func (f *fakeLists) Items(_ context.Context, listID string) ([]entity.KeptItem, error) {
	f.asked, f.listID = true, listID
	return f.held, f.err
}

func (f *fakeLists) Item(_ context.Context, listID string, id int64) (entity.KeptItem, error) {
	f.asked, f.listID = true, listID
	for _, item := range f.held {
		if item.ID == id {
			return item, f.err
		}
	}
	return entity.KeptItem{}, errorkind.New(errorkind.ErrNotFound, fmt.Sprintf("no item %d", id))
}

func (f *fakeLists) Save(_ context.Context, listID string, item entity.KeptItem) (entity.KeptItem, error) {
	given := item // what the command gave, before the answer's new version
	f.saved = &given
	if f.saveErr != nil {
		return entity.KeptItem{}, f.saveErr
	}
	item.Version = keptVersion
	return item, nil
}

// listsWorld is the state of the item-list scenarios, reset before each.
type listsWorld struct {
	lists     *fakeLists
	added     command.AddItemResult
	listed    command.ListItemsResult
	completed command.CompleteItemResult
	err       error
}

var kinds = map[string]error{
	"invalid": pipeline.ErrInvalid, "conflict": pipeline.ErrConflict, "not found": pipeline.ErrNotFound,
	"concurrent change": pipeline.ErrConcurrentChange, "failed precondition": pipeline.ErrFailedPrecondition,
}

func registerItemListsSteps(sc *godog.ScenarioContext) {
	w := &listsWorld{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = listsWorld{lists: &fakeLists{}}
		return ctx, nil
	})
	sc.Step(`^the item lists answer the kind "([^"]*)"$`, func(kind string) error {
		w.lists.err = errorkind.New(kinds[kind], "from the item lists")
		return nil
	})
	sc.Step(`^the item lists hold "([^"]*)" for the list "([^"]*)"$`, func(titles, listID string) error {
		for _, title := range strings.Split(titles, "|") {
			item, err := entity.NewItem(title)
			if err != nil {
				return err
			}
			w.lists.held = append(w.lists.held, entity.KeptItem{ID: int64(len(w.lists.held) + 1), Version: "7", Item: item})
		}
		return nil
	})
	sc.Step(`^the item lists hold the item (\d+) "([^"]*)" at the version "([^"]*)"$`, func(id int64, title, version string) error {
		item, err := entity.NewItem(title)
		if err != nil {
			return err
		}
		w.lists.held = append(w.lists.held, entity.KeptItem{ID: id, Version: version, Item: item})
		return nil
	})
	sc.Step(`^the item (\d+) is done already$`, func(id int64) error {
		for i := range w.lists.held {
			if w.lists.held[i].ID == id {
				return w.lists.held[i].Complete()
			}
		}
		return fmt.Errorf("no item %d held", id)
	})
	sc.Step(`^the item lists answer the kind "([^"]*)" to a save$`, func(kind string) error {
		w.lists.saveErr = errorkind.New(kinds[kind], "from the item lists")
		return nil
	})
	sc.Step(`^I send CompleteItem with the list "([^"]*)", the item (\d+), and the version "([^"]*)"$`, func(ctx context.Context, listID string, id int64, version string) error {
		w.completed, w.err = command.NewCompleteItemHandler(w.lists).Handle(ctx, command.CompleteItemCommand{ListID: listID, ItemID: id, Version: version})
		logf("when: CompleteItem(%q, %d, %q) -> %+v err=%v", listID, id, version, w.completed, w.err)
		return nil
	})
	sc.Step(`^the item lists should have saved the item (\d+) done, under the version "([^"]*)"$`, func(id int64, version string) error {
		saved := w.lists.saved
		if saved == nil || saved.ID != id || !saved.Done() || saved.Version != version {
			return fmt.Errorf("saved %+v, want the item %d done under the version %q", saved, id, version)
		}
		return nil
	})
	sc.Step(`^the completed item should be done, with the version "([^"]*)"$`, func(version string) error {
		if w.err != nil || !w.completed.Item.Done || w.completed.Item.Version != version {
			return fmt.Errorf("completed %+v err=%v, want done at the version %q", w.completed, w.err, version)
		}
		return nil
	})
	sc.Step(`^the item lists should not have saved$`, func() error {
		if w.lists.saved != nil {
			return fmt.Errorf("the item lists saved %+v", w.lists.saved)
		}
		return nil
	})
	sc.Step(`^I send AddItem with the list "([^"]*)" and the title "([^"]*)"$`, func(ctx context.Context, listID, title string) error {
		w.added, w.err = command.NewAddItemHandler(w.lists).Handle(ctx, command.AddItemCommand{ListID: listID, Title: title})
		logf("when: AddItem(%q, %q) -> %+v err=%v", listID, title, w.added, w.err)
		return nil
	})
	sc.Step(`^I send ListItems with the list "([^"]*)"$`, func(ctx context.Context, listID string) error {
		w.listed, w.err = command.NewListItemsHandler(w.lists).Handle(ctx, command.ListItemsCommand{ListID: listID})
		logf("when: ListItems(%q) -> %+v err=%v", listID, w.listed, w.err)
		return nil
	})
	sc.Step(`^the added item should be the open item "([^"]*)"$`, func(title string) error {
		if w.err != nil || w.added.Item.Title != title || w.added.Item.Done {
			return fmt.Errorf("added %+v err=%v, want the open item %q", w.added, w.err, title)
		}
		return nil
	})
	sc.Step(`^the added item should carry the id and the version the item lists answered$`, func() error {
		if w.added.Item.ID != 1 || w.added.Item.Version != keptVersion {
			return fmt.Errorf("added %+v, want the id 1 and the version %q", w.added.Item, keptVersion)
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
