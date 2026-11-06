package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/domain/entity"
)

// keptWorld is the state of the scenarios on the lists kept in the process, reset before each.
type keptWorld struct {
	lists *command.KeptLists
	err   error
}

func registerKeptListsSteps(sc *godog.ScenarioContext) {
	w := &keptWorld{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = keptWorld{lists: command.NewKeptLists()}
		return ctx, nil
	})
	add := func(ctx context.Context, title, listID string) error {
		item, err := entity.NewItem(title)
		if err != nil {
			return err
		}
		w.err = w.lists.Add(ctx, listID, item)
		logf("when: Add(%s, %s) -> %v", listID, title, w.err)
		return nil
	}
	sc.Step(`^I add "([^"]*)" to the kept list "([^"]*)"$`, add)
	sc.Step(`^I read the kept list "([^"]*)"$`, func(ctx context.Context, listID string) error {
		_, w.err = w.lists.Items(ctx, listID)
		return nil
	})
	sc.Step(`^the kept list "([^"]*)" should hold "([^"]*)"$`, func(ctx context.Context, listID, titles string) error {
		items, err := w.lists.Items(ctx, listID)
		if err != nil {
			return err
		}
		got := make([]string, len(items))
		for i, item := range items {
			got[i] = item.Title()
		}
		if strings.Join(got, "|") != titles {
			return fmt.Errorf("the list holds %q, want %q", strings.Join(got, "|"), titles)
		}
		return nil
	})
	sc.Step(`^keeping should fail with the kind "([^"]*)" mentioning '([^']*)'$`, func(kind, text string) error {
		if w.err == nil || !errors.Is(w.err, kinds[kind]) {
			return fmt.Errorf("the error is %v, want the kind %s", w.err, kind)
		}
		if !strings.Contains(w.err.Error(), text) {
			return fmt.Errorf("the error %q does not mention %q", w.err.Error(), text)
		}
		return nil
	})
}
