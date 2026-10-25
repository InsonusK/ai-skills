package test

import (
	"encoding/json"
	"fmt"
	"net/http/httptest"
	"os"
	"strings"

	"github.com/cucumber/godog"

	httpapi "github.com/example/todo/internal/api/http"
	"github.com/example/todo/internal/api/http/middleware/authentication"
	httppipeline "github.com/example/todo/internal/api/http/pipeline"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/middleware/access"
	"github.com/example/todo/internal/command/middleware/batchlimit"
	commandlogging "github.com/example/todo/internal/command/middleware/logging"
	"github.com/example/todo/internal/command/pipeline"
)

func registerAccessSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the callers "([^"]*)" and the batch limit (\d+)$`, w.theCallers)
	sc.Step(`^"([^"]*)" POSTs "([^"]*)" with the body '([^']*)'$`, w.post)
	sc.Step(`^nobody POSTs "([^"]*)" with the body '([^']*)'$`, func(path, body string) error { return w.post("", path, body) })
	sc.Step(`^the response status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the error body should have code (\d+) and the message "([^"]*)"$`, w.theErrorBodyShouldBe)
	sc.Step(`^the records should be, in order, "([^"]*)"$`, w.theRecordsShouldBeInOrder)
}

// theCallers builds the application as cmd/todo/main.go does with the access
// variation point attached - the same command and HTTP middleware in the same
// order, over the real command registry and routes - and serves it in memory.
func (w *World) theCallers(tokens string, limit int) error {
	if err := os.Setenv("ACCESS_TOKENS", tokens); err != nil {
		return err
	}
	cfg, err := access.LoadConfig()
	if err != nil {
		return err
	}
	logger := w.logger()
	handlers := command.NewHandlers([]pipeline.Middleware{
		commandlogging.Middleware(logger),
		access.Middleware(),
		batchlimit.Middleware(limit),
	})
	w.server = httppipeline.NewServer(httppipeline.Config{}, httpapi.Routes(httpapi.NewEndpoints(handlers)), logger,
		[]httppipeline.Middleware{authentication.Middleware(cfg.Tokens)}).Handler()
	return nil
}

// post sends the request with the bearer token, or with no Authorization
// header when token is empty.
func (w *World) post(token, path, body string) error {
	req := httptest.NewRequest("POST", path, strings.NewReader(body))
	if token != "" {
		req.Header.Set("Authorization", "Bearer "+token)
	}
	rec := httptest.NewRecorder()
	w.server.ServeHTTP(rec, req)
	w.status, w.body = rec.Code, rec.Body.Bytes()
	logf("when: POST %s %s as %q -> %d %s", path, body, token, w.status, strings.TrimSpace(string(w.body)))
	return nil
}

func (w *World) theResponseStatusShouldBe(want int) error {
	if w.status != want {
		return fmt.Errorf("status: got %d, want %d (body %s)", w.status, want, w.body)
	}
	return nil
}

func (w *World) theErrorBodyShouldBe(code int, message string) error {
	var status struct {
		Code    int    `json:"code"`
		Message string `json:"message"`
	}
	if err := json.Unmarshal(w.body, &status); err != nil {
		return fmt.Errorf("body %s is not google.rpc.Status: %w", w.body, err)
	}
	if status.Code != code || status.Message != message {
		return fmt.Errorf("status: got %d %q, want %d %q", status.Code, status.Message, code, message)
	}
	return nil
}

func (w *World) theRecordsShouldBeInOrder(want string) error {
	var got []string
	for _, line := range strings.Split(strings.TrimSpace(w.logs.String()), "\n") {
		var record map[string]any
		if json.Unmarshal([]byte(line), &record) == nil {
			got = append(got, fmt.Sprint(record["msg"]))
		}
	}
	if strings.Join(got, "|") != want {
		return fmt.Errorf("records in order: got %q, want %q", strings.Join(got, "|"), want)
	}
	return nil
}
