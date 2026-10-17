package test

import (
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/domain"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	item    domain.Item
	itemErr error

	completeErr error

	planned []domain.Item
	planErr error
}

func (w *World) reset() { *w = World{} }

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}

// reasons names each business-rule error the way the features word it.
var reasons = map[string]error{
	"empty title":     domain.ErrEmptyTitle,
	"title too long":  domain.ErrTitleTooLong,
	"already done":    domain.ErrAlreadyDone,
	"no items":        domain.ErrNoItems,
	"duplicate title": domain.ErrDuplicateTitle,
}

func expectReason(got error, reason string) error {
	want, ok := reasons[reason]
	if !ok {
		return fmt.Errorf("unknown reason %q in the feature", reason)
	}
	if got != want {
		return fmt.Errorf("error: got %v, want %v", got, want)
	}
	return nil
}
