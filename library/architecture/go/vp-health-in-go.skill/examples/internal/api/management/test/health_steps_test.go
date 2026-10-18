package test

import (
	"context"
	"errors"
	"fmt"
	"net/http"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/api/management"
)

func registerHealthSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a readiness check "([^"]*)" that (passes|fails)$`, w.aReadinessCheck)
	sc.Step(`^the service version is "([^"]*)"$`, w.theServiceVersionIs)
	sc.Step(`^I GET "([^"]*)"$`, w.iGET)
	sc.Step(`^the response status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the response body should be '([^']*)'$`, w.theResponseBodyShouldBe)
}

func (w *World) aReadinessCheck(ctx context.Context, name, outcome string) error {
	var result error
	if outcome == "fails" {
		result = errors.New(name + " is down")
	}
	w.checks = append(w.checks, management.Check{Name: name, Check: func(context.Context) error { return result }})
	return nil
}

func (w *World) theServiceVersionIs(ctx context.Context, version string) error {
	w.version = version
	return nil
}

func (w *World) iGET(ctx context.Context, path string) error {
	w.send(http.MethodGet, path)
	return nil
}

func (w *World) theResponseStatusShouldBe(ctx context.Context, want int) error {
	if w.response.Code != want {
		return fmt.Errorf("status: got %d, want %d (body %s)", w.response.Code, want, w.response.Body.String())
	}
	return nil
}

func (w *World) theResponseBodyShouldBe(ctx context.Context, want string) error {
	if got := strings.TrimSpace(w.response.Body.String()); got != want {
		return fmt.Errorf("body: got %s, want %s", got, want)
	}
	return nil
}
