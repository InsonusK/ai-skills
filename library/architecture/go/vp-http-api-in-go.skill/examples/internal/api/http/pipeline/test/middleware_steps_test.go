package test

import (
	"fmt"
	"net/http"

	"github.com/cucumber/godog"

	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command/pipeline"
)

func registerMiddlewareSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the middlewares "([^"]*)" and "([^"]*)", each logging a record with its name$`, w.theMiddlewaresLogging)
	sc.Step(`^a middleware that rejects every request as unauthenticated with "([^"]*)"$`, w.aRejectingMiddleware)
}

// theMiddlewaresLogging gives the server two middlewares, in this order, each
// writing the record "middleware {name}" before it passes the request on.
func (w *World) theMiddlewaresLogging(outer, inner string) error {
	for _, name := range []string{outer, inner} {
		w.middlewares = append(w.middlewares, func(next http.Handler) http.Handler {
			return http.HandlerFunc(func(rw http.ResponseWriter, r *http.Request) {
				w.logger().InfoContext(r.Context(), "middleware "+name)
				next.ServeHTTP(rw, r)
			})
		})
	}
	return nil
}

// aRejectingMiddleware gives the server a middleware that answers every
// request with an unauthenticated error, as an authentication middleware
// answers a request without credentials.
func (w *World) aRejectingMiddleware(message string) error {
	w.middlewares = append(w.middlewares, func(http.Handler) http.Handler {
		return http.HandlerFunc(func(rw http.ResponseWriter, r *http.Request) {
			httppipeline.WriteError(rw, fmt.Errorf("%w: %s", pipeline.ErrUnauthenticated, message))
		})
	})
	return nil
}
