package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

func registerPlanItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^I send PlanItems with the titles "([^"]*)"$`, w.iSendPlanItems)
	sc.Step(`^the result should list the open items "([^"]*)"$`, w.theResultShouldList)
	sc.Step(`^the command should be rejected as invalid$`, w.theCommandShouldBeRejected)
}

func (w *World) iSendPlanItems(ctx context.Context, titles string) error {
	cmd := command.PlanItems{Titles: strings.Split(titles, "|")}
	w.result, w.err = command.NewPlanItemsHandler().Handle(ctx, cmd)
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
	if !errors.Is(w.err, pipeline.ErrInvalid) {
		return fmt.Errorf("error: got %v, want one wrapping pipeline.ErrInvalid", w.err)
	}
	return nil
}
