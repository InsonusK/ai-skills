package test

// The steps of import-items.feature: ImportItems over a fake AddItem that
// records the titles it ran for and rejects the one a scenario names.

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/shared/errorkind"
)

type importWorld struct {
	ran      []string
	rejected string
	result   command.ImportItemsResult
	err      error
}

func registerImportItemsSteps(sc *godog.ScenarioContext) {
	w := &importWorld{}
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = importWorld{}
		return ctx, nil
	})
	addItem := pipeline.HandlerFunc[command.AddItemCommand, command.AddItemResult](
		func(_ context.Context, cmd command.AddItemCommand) (command.AddItemResult, error) {
			if cmd.Title == w.rejected {
				return command.AddItemResult{}, errorkind.New(errorkind.ErrConflict, cmd.Title+" is taken")
			}
			w.ran = append(w.ran, cmd.Title) // the titles AddItem added
			return command.AddItemResult{Item: command.ItemView{Title: cmd.Title}}, nil
		})
	sc.Step(`^the title "([^"]*)" is rejected$`, func(title string) error { w.rejected = title; return nil })
	sc.Step(`^I send ImportItems to the list "([^"]*)" with the titles "([^"]*)"$`, func(ctx context.Context, listID, titles string) error {
		w.result, w.err = command.NewImportItemsHandler(addItem).Handle(ctx, command.ImportItemsCommand{ListID: listID, Titles: strings.Split(titles, "|")})
		logf("when: ImportItems(%q, %q) -> %+v err=%v", listID, titles, w.result, w.err)
		return nil
	})
	sc.Step(`^AddItem should have run for "([^"]*)"$`, func(titles string) error {
		if got := strings.Join(w.ran, "|"); got != titles {
			return fmt.Errorf("AddItem ran for %q, want %q", got, titles)
		}
		return nil
	})
	sc.Step(`^the import should answer the items "([^"]*)"$`, func(titles string) error {
		got := []string{}
		for _, item := range w.result.Items {
			got = append(got, item.Title)
		}
		if w.err != nil || strings.Join(got, "|") != titles {
			return fmt.Errorf("import answered %q err=%v, want %q", got, w.err, titles)
		}
		return nil
	})
	sc.Step(`^the import should fail with the kind of the rejection$`, func() error {
		if !errors.Is(w.err, errorkind.ErrConflict) {
			return fmt.Errorf("import failed with %v, want the rejection's kind", w.err)
		}
		return nil
	})
}
