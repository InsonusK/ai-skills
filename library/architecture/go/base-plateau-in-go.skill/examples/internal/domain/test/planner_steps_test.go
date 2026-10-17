package test

import (
	"context"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/domain"
)

func registerPlannerSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^I plan the titles "([^"]*)"$`, w.iPlanTheTitles)
	sc.Step(`^I plan no titles$`, w.iPlanNoTitles)
	sc.Step(`^planning should succeed with the titles "([^"]*)"$`, w.planningShouldSucceedWith)
	sc.Step(`^planning should fail with "([^"]*)"$`, w.planningShouldFailWith)
}

func (w *World) plan(titles []string) {
	w.planned, w.planErr = domain.NewPlanner().Plan(titles)
	logf("when: Plan(%q) -> %d items err=%v", titles, len(w.planned), w.planErr)
}

func (w *World) iPlanTheTitles(ctx context.Context, titles string) error {
	w.plan(strings.Split(titles, "|"))
	return nil
}

func (w *World) iPlanNoTitles(ctx context.Context) error {
	w.plan(nil)
	return nil
}

func (w *World) planningShouldSucceedWith(ctx context.Context, titles string) error {
	if w.planErr != nil {
		return fmt.Errorf("planning failed: %v", w.planErr)
	}
	want := strings.Split(titles, "|")
	got := make([]string, len(w.planned))
	for i, item := range w.planned {
		if item.Done() {
			return fmt.Errorf("item %q is done, want open", item.Title())
		}
		got[i] = item.Title()
	}
	logf("then: titles=%q want=%q", got, want)
	if strings.Join(got, "|") != strings.Join(want, "|") {
		return fmt.Errorf("titles: got %q, want %q", got, want)
	}
	return nil
}

func (w *World) planningShouldFailWith(ctx context.Context, reason string) error {
	return expectReason(w.planErr, reason)
}
