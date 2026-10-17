package test

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/todo/docs/integration/api/todo"
	httpapi "github.com/example/todo/internal/api/http"
)

func registerHealthDocsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a readiness check "([^"]*)" that (passes|fails)$`, w.aReadinessCheck)
	sc.Step(`^the service version is "([^"]*)"$`, w.theServiceVersionIs)
	sc.Step(`^the docs are enabled$`, w.theDocsAreEnabled)
	sc.Step(`^the service is published under "([^"]*)"$`, w.theServiceIsPublishedUnder)
	sc.Step(`^the spec's server URL should be "([^"]*)"$`, w.theSpecServerURLShouldBe)
	sc.Step(`^the response should redirect to "([^"]*)"$`, w.theResponseShouldRedirectTo)
	sc.Step(`^the response should be the embedded spec$`, w.theResponseShouldBeTheEmbeddedSpec)
}

func (w *World) aReadinessCheck(ctx context.Context, name, outcome string) error {
	var result error
	if outcome == "fails" {
		result = errors.New(name + " is down")
	}
	w.checks = append(w.checks, httpapi.ReadinessCheck{Name: name, Check: func(context.Context) error { return result }})
	return nil
}

func (w *World) theServiceVersionIs(ctx context.Context, version string) error {
	w.version = version
	return nil
}

func (w *World) theDocsAreEnabled(ctx context.Context) error {
	w.cfg.DocsEnabled = true
	return nil
}

func (w *World) theServiceIsPublishedUnder(ctx context.Context, basePath string) error {
	w.cfg.BasePath = basePath
	return nil
}

func (w *World) theSpecServerURLShouldBe(ctx context.Context, want string) error {
	var doc struct {
		Servers []struct {
			URL string `json:"url"`
		} `json:"servers"`
	}
	if err := json.Unmarshal(w.response.Body.Bytes(), &doc); err != nil {
		return fmt.Errorf("spec is not JSON: %w", err)
	}
	if len(doc.Servers) != 1 || doc.Servers[0].URL != want {
		return fmt.Errorf("servers: got %+v, want one with url %q", doc.Servers, want)
	}
	return nil
}

func (w *World) theResponseShouldRedirectTo(ctx context.Context, want string) error {
	if got := w.response.Header().Get("Location"); got != want {
		return fmt.Errorf("Location: got %q, want %q", got, want)
	}
	return nil
}

func (w *World) theResponseShouldBeTheEmbeddedSpec(ctx context.Context) error {
	if !bytes.Equal(w.response.Body.Bytes(), todo.Spec) {
		return fmt.Errorf("the served spec differs from the embedded one")
	}
	return nil
}
