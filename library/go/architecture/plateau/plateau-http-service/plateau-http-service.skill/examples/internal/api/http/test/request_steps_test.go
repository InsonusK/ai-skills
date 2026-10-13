package test

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http/httptest"
	"strings"

	"github.com/cucumber/godog"

	apihttp "github.com/example/linkcheck-service/internal/api/http"
	"github.com/example/linkcheck-service/internal/version"
)

func registerRequestSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the service is started with docs "(on|off)" under the base path "([^"]*)"$`, w.theServiceIsStartedWith)
	sc.Step(`^I send "([A-Z]+)" "([^"]*)"$`, w.iSend)
	sc.Step(`^I send "([A-Z]+)" "([^"]*)" with the body:$`, w.iSendWithTheBody)
	sc.Step(`^the response status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the response body should have "([^"]*)" equal to "([^"]*)"$`, w.theResponseBodyShouldHave)
	sc.Step(`^the response body should have "([^"]*)" equal to the binary's version$`, w.theResponseBodyShouldHaveTheVersion)
	sc.Step(`^the response should mention "(.*)"$`, w.theResponseShouldMention)
}

func (w *World) theServiceIsStartedWith(ctx context.Context, docs, basePath string) error {
	logf("given: docs=%s basePath=%q", docs, basePath)
	w.server = newServer(apihttp.Options{DocsEnabled: docs == "on", BasePath: basePath})
	return nil
}

func (w *World) iSend(ctx context.Context, method, path string) error {
	return w.send(method, path, "")
}

func (w *World) iSendWithTheBody(ctx context.Context, method, path string, body *godog.DocString) error {
	return w.send(method, path, body.Content)
}

func (w *World) send(method, path, body string) error {
	recorder := httptest.NewRecorder()
	w.handler().ServeHTTP(recorder, httptest.NewRequest(method, path, strings.NewReader(body)))
	w.status = recorder.Code
	w.location = recorder.Header().Get("Location")
	w.body = recorder.Body.String()
	logf("when: %s %s body=%q -> status=%d location=%q body=%.200q", method, path, body, w.status, w.location, w.body)
	return nil
}

func (w *World) theResponseStatusShouldBe(ctx context.Context, want int) error {
	logf("then: status=%d want=%d", w.status, want)
	if w.status != want {
		return fmt.Errorf("response status: got %d, want %d (body=%s)", w.status, want, w.body)
	}
	return nil
}

func (w *World) theResponseBodyShouldHave(ctx context.Context, field, want string) error {
	var body map[string]any
	if err := json.Unmarshal([]byte(w.body), &body); err != nil {
		return fmt.Errorf("the response body is not a JSON object: %w (body=%s)", err, w.body)
	}
	got := fmt.Sprint(body[field])
	logf("then: body[%s]=%q want=%q", field, got, want)
	if got != want {
		return fmt.Errorf("response body field %q: got %q, want %q (body=%s)", field, got, want, w.body)
	}
	return nil
}

func (w *World) theResponseBodyShouldHaveTheVersion(ctx context.Context, field string) error {
	return w.theResponseBodyShouldHave(ctx, field, version.Version)
}

// theResponseShouldMention looks in the body and in the Location header: a
// redirect says where it points only in the header.
func (w *World) theResponseShouldMention(ctx context.Context, want string) error {
	logf("then: mention=%q location=%q", want, w.location)
	if want == "" {
		return nil // the rows of a route that does not exist leave this column blank
	}
	if !strings.Contains(w.body, want) && !strings.Contains(w.location, want) {
		return fmt.Errorf("neither the body nor the Location header mentions %q (location=%q body=%.400s)", want, w.location, w.body)
	}
	return nil
}
