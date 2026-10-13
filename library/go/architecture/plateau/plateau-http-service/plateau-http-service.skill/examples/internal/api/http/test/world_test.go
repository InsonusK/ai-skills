package test

import (
	"context"
	"fmt"
	"net/http"

	"github.com/cucumber/godog"

	apihttp "github.com/example/linkcheck-service/internal/api/http"
	"github.com/example/linkcheck-service/internal/domain/services"
)

// logf logs a step's action/observation via stdout, matching godog's pretty
// output so the line stays attached to the step that produced it (see
// cucumber-testing-in-go's "Log through stdout, not testing.T" rule).
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario runs.
type World struct {
	server *apihttp.Server

	status   int
	location string
	body     string
}

func newWorld() *World {
	w := &World{}
	w.reset()
	return w
}

// reset starts the adapter as a deployment with no variable set does: docs off,
// no base path.
func (w *World) reset() {
	*w = World{server: newServer(apihttp.Options{})}
}

func newServer(options apihttp.Options) *apihttp.Server {
	return apihttp.New(services.NewLinkCheckService(), options)
}

func (w *World) handler() http.Handler {
	return w.server.Handler()
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, s *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}
