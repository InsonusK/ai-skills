package test

import (
	"context"
	"fmt"
	"io"
	"log/slog"
	"net/http/httptest"
	"strings"

	"github.com/cucumber/godog"

	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/api/http/endpoint"
	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario. The generic
// behaviour of every route (decoding, proto-JSON, error mapping) is tested once
// in internal/api/http; here each resource is tested for its own mapping.
type World struct {
	// the fake handlers - the command layer below the HTTP entry point
	planResult command.PlanItemsResult
	planCalled bool
	planGot    command.PlanItemsCommand

	addResult command.AddItemResult
	addErr    error
	addGot    command.AddItemCommand

	listResult command.ListItemsResult
	listGot    command.ListItemsCommand

	response *httptest.ResponseRecorder
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
		return ctx, nil
	})
}

// handlers are the fake handlers of every resource; a new resource adds its fakes here.
func (w *World) handlers() httpapi.Endpoints {
	return httpapi.Endpoints{
		Items: endpoint.ItemsHandler{
			PlanItems: pipeline.HandlerFunc[command.PlanItemsCommand, command.PlanItemsResult](
				func(_ context.Context, cmd command.PlanItemsCommand) (command.PlanItemsResult, error) {
					w.planCalled, w.planGot = true, cmd
					return w.planResult, nil
				}),
		},
		Lists: endpoint.ListsHandler{
			AddItem: pipeline.HandlerFunc[command.AddItemCommand, command.AddItemResult](
				func(_ context.Context, cmd command.AddItemCommand) (command.AddItemResult, error) {
					w.addGot = cmd
					return w.addResult, w.addErr
				}),
			ListItems: pipeline.HandlerFunc[command.ListItemsCommand, command.ListItemsResult](
				func(_ context.Context, cmd command.ListItemsCommand) (command.ListItemsResult, error) {
					w.listGot = cmd
					return w.listResult, nil
				}),
		},
	}
}

func (w *World) send(method, path, body string) {
	logger := slog.New(slog.NewTextHandler(io.Discard, nil))
	server := httppipeline.NewServer(httppipeline.Config{}, httpapi.Routes(w.handlers()), logger, nil)
	req := httptest.NewRequest(method, path, strings.NewReader(body))
	w.response = httptest.NewRecorder()
	server.Handler().ServeHTTP(w.response, req)
	logf("when: %s %s %s -> %d %s", method, path, body, w.response.Code, strings.TrimSpace(w.response.Body.String()))
}
