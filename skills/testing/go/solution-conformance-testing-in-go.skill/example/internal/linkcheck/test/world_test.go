package test

import (
	"bytes"
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck/internal/linkcheck"
)

// logf logs a step's action/observation via stdout, matching godog's pretty
// output so the line stays attached to the step that produced it (see
// cucumber-testing-in-go's "Log through stdout, not testing.T" rule).
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario runs.
type World struct {
	input  string
	result linkcheck.Result

	out        bytes.Buffer
	errOut     bytes.Buffer
	exitCode   int
	record     map[string]any
	mapped     linkcheck.Result
	mappingErr error
	store      *linkcheck.HistoryStore
	history    []linkcheck.Result
	storeErr   error
	text       string
	links      []string
}

func newWorld() *World {
	return &World{}
}

func (w *World) reset() {
	*w = World{}
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, s *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}
