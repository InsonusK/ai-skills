package test

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"net/url"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/docs/integration/api/todo"
)

func registerDocsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the HTTP docs are enabled$`, w.theDocsAreEnabled)
	sc.Step(`^the service is published under "([^"]*)"$`, w.theServiceIsPublishedUnder)
	sc.Step(`^the spec's server URL should be "([^"]*)"$`, w.theSpecServerURLShouldBe)
	sc.Step(`^the HTTP response should redirect into "([^"]*)"$`, w.theResponseShouldRedirectInto)
	sc.Step(`^I open the Swagger UI of the HTTP API$`, w.iOpenTheSwaggerUI)
	sc.Step(`^the Swagger UI should load the spec from "([^"]*)"$`, w.theSwaggerUIShouldLoadTheSpecFrom)
	sc.Step(`^the HTTP response should be the embedded spec$`, w.theResponseShouldBeTheEmbeddedSpec)
}

func (w *World) theDocsAreEnabled(ctx context.Context) error {
	w.cfg.DocsEnabled = true
	return nil
}

func (w *World) theServiceIsPublishedUnder(ctx context.Context, basePath string) error {
	w.cfg.BasePath = basePath
	return nil
}

// theSpecServerURLShouldBe reads the prefix where the spec's version keeps
// it: servers[0].url in OpenAPI 3, basePath in OpenAPI 2.
func (w *World) theSpecServerURLShouldBe(ctx context.Context, want string) error {
	var doc struct {
		Swagger  string `json:"swagger"`
		BasePath string `json:"basePath"`
		Servers  []struct {
			URL string `json:"url"`
		} `json:"servers"`
	}
	if err := json.Unmarshal(w.response.Body.Bytes(), &doc); err != nil {
		return fmt.Errorf("spec is not JSON: %w", err)
	}
	if doc.Swagger != "" {
		if doc.BasePath != want {
			return fmt.Errorf("basePath: got %q, want %q", doc.BasePath, want)
		}
		return nil
	}
	if len(doc.Servers) != 1 || doc.Servers[0].URL != want {
		return fmt.Errorf("servers: got %+v, want one with url %q", doc.Servers, want)
	}
	return nil
}

// theResponseShouldRedirectInto resolves the Location against the URL the
// client called (the published prefix and the path asked for) and checks it
// lands under prefix.
func (w *World) theResponseShouldRedirectInto(ctx context.Context, prefix string) error {
	if w.response.Code < 300 || w.response.Code > 399 {
		return fmt.Errorf("status: got %d, want a redirect", w.response.Code)
	}
	called, _ := url.Parse("http://client" + w.cfg.BasePath + w.lastPath)
	location, err := url.Parse(w.response.Header().Get("Location"))
	if err != nil {
		return err
	}
	if landed := called.ResolveReference(location).Path; !strings.HasPrefix(landed, prefix) {
		return fmt.Errorf("redirect lands on %q, want under %q", landed, prefix)
	}
	return nil
}

// iOpenTheSwaggerUI loads Swagger UI's page, which tells it where the spec is.
func (w *World) iOpenTheSwaggerUI(ctx context.Context) error {
	w.send(http.MethodGet, "/swagger/", "")
	if w.response.Code != http.StatusOK {
		return fmt.Errorf("Swagger UI: status %d", w.response.Code)
	}
	return nil
}

func (w *World) theSwaggerUIShouldLoadTheSpecFrom(ctx context.Context, url string) error {
	if !strings.Contains(w.response.Body.String(), `"`+url+`"`) {
		return fmt.Errorf("Swagger UI does not load %q: %s", url, w.response.Body.String())
	}
	return nil
}

func (w *World) theResponseShouldBeTheEmbeddedSpec(ctx context.Context) error {
	if !bytes.Equal(w.response.Body.Bytes(), todo.Spec) {
		return fmt.Errorf("the served spec differs from the embedded one")
	}
	return nil
}
