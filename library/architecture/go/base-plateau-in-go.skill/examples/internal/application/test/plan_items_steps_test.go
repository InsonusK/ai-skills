package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/application"
)

func registerPlanItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^I send PlanItems with the titles "([^"]*)"$`, w.iSendPlanItems)
	sc.Step(`^the result should list the open items "([^"]*)"$`, w.theResultShouldList)
	sc.Step(`^the command should be rejected as invalid$`, w.theCommandShouldBeRejected)
}

func (w *World) iSendPlanItems(ctx context.Context, titles string) error {
	cmd := application.PlanItems{Titles: strings.Split(titles, "|")}
	w.result, w.err = application.NewPlanItemsHandler().Handle(ctx, cmd)
	logf("when: PlanItems(%q) -> %+v err=%v", cmd.Titles, w.result, w.err)
	return nil
}

func (w *World) theResultShouldList(ctx context.Context, titles string) error {
	if w.err != nil {
		return fmt.Errorf("command failed: %v", w.err)
	}
	want := strings.Split(titles, "|")
	if len(w.result.Items) != len(want) {
		return fmt.Errorf("items: got %d, want %d", len(w.result.Items), len(want))
	}
	for i, item := range w.result.Items {
		if item.Title != want[i] || item.Done {
			return fmt.Errorf("item %d: got %+v, want open %q", i, item, want[i])
		}
	}
	return nil
}

func (w *World) theCommandShouldBeRejected(ctx context.Context) error {
	logf("then: err=%v", w.err)
	if !errors.Is(w.err, application.ErrInvalid) {
		return fmt.Errorf("error: got %v, want one wrapping application.ErrInvalid", w.err)
	}
	return nil
}
