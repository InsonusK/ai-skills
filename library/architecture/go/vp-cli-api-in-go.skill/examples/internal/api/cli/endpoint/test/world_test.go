package test

import (
	"bytes"
	"context"
	"fmt"
	"log/slog"
	"strings"

	"github.com/cucumber/godog"

	cliapi "github.com/example/todo/internal/api/cli"
	"github.com/example/todo/internal/api/cli/endpoint"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario. The generic
// behaviour of every action (usage errors, kinds, output formats, help) is
// tested once in internal/api/cli; here each resource is tested for its own
// flags and output.
type World struct {
	planResult command.PlanItemsResult
	planGot    command.PlanItems

	stdout, stderr bytes.Buffer
	logs           bytes.Buffer // what the run logged, every attribute kept
	err            error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
		return ctx, nil
	})
}

// handlers are the fake handlers of every resource; a new resource adds its fakes here.
func (w *World) handlers() endpoint.Handlers {
	return endpoint.Handlers{
		Items: endpoint.ItemHandlers{
			PlanItems: pipeline.HandlerFunc[command.PlanItems, command.PlanItemsResult](
				func(_ context.Context, cmd command.PlanItems) (command.PlanItemsResult, error) {
					w.planGot = cmd
					return w.planResult, nil
				}),
		},
	}
}

func (w *World) run(ctx context.Context, line string) error {
	w.err = cliapi.Run(ctx, slog.New(slog.NewJSONHandler(&w.logs, nil)), "todo", "0.0.0-test", strings.Fields(line), endpoint.Resources(w.handlers()), &w.stdout, &w.stderr)
	logf("when: todo %s -> err=%v\nstdout: %s\nstderr: %s", line, w.err, w.stdout.String(), w.stderr.String())
	return w.err
}
