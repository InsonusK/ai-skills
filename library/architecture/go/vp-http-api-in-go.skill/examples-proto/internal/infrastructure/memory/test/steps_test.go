package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/memory"
)

func registerSteps(sc *godog.ScenarioContext) {
	var lists *memory.Lists
	var err error
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		lists, err = memory.NewLists(), nil
		return ctx, nil
	})
	insert := func(ctx context.Context, title, listID string) error {
		err = lists.Insert(ctx, listID, memory.Row{Title: title})
		return nil
	}
	sc.Step(`^the list "([^"]*)" holds "([^"]*)"$`, func(ctx context.Context, listID, title string) error {
		return lists.Insert(ctx, listID, memory.Row{Title: title})
	})
	sc.Step(`^I insert "([^"]*)" into the list "([^"]*)"$`, insert)
	sc.Step(`^the list "([^"]*)" should hold "([^"]*)"$`, func(ctx context.Context, listID, titles string) error {
		rows, selectErr := lists.Select(ctx, listID)
		got := []string{}
		for _, row := range rows {
			got = append(got, row.Title)
		}
		if err != nil || selectErr != nil || strings.Join(got, "|") != titles {
			return fmt.Errorf("list %q holds %q (insert err=%v, select err=%v), want %q", listID, got, err, selectErr, titles)
		}
		return nil
	})
	sc.Step(`^the insert should fail with a duplicate key$`, func() error {
		if !errors.Is(err, memory.ErrDuplicateKey) {
			return fmt.Errorf("insert error %v, want a duplicate key", err)
		}
		return nil
	})
	sc.Step(`^selecting the list "([^"]*)" should fail with no rows$`, func(ctx context.Context, listID string) error {
		if _, selectErr := lists.Select(ctx, listID); !errors.Is(selectErr, memory.ErrNoRows) {
			return fmt.Errorf("select error %v, want no rows", selectErr)
		}
		return nil
	})
}
