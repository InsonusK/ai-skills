package test

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/todo/docs/integration/api/todo"
)

func registerDocsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the docs are enabled$`, w.theDocsAreEnabled)
	sc.Step(`^the service is published under "([^"]*)"$`, w.theServiceIsPublishedUnder)
	sc.Step(`^the spec's server URL should be "([^"]*)"$`, w.theSpecServerURLShouldBe)
	sc.Step(`^the response should redirect to "([^"]*)"$`, w.theResponseShouldRedirectTo)
	sc.Step(`^the response should be the embedded spec$`, w.theResponseShouldBeTheEmbeddedSpec)
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
