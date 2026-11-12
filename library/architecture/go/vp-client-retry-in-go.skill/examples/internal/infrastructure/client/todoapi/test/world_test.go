package test

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"net/url"
	"os"
	"strconv"
	"strings"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/client/core/httpclient"
	"github.com/example/todo/internal/infrastructure/client/todoapi"
)

func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	answer  string
	mocked  bool
	server  *httptest.Server
	method  string
	path    string
	body    string
	items   []todoapi.Item
	callErr error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
		return ctx, nil
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		if w.server != nil {
			w.server.Close()
		}
		return ctx, nil
	})
}

// client is the adapter against an emulator of the todo API serving the
// scenario's answer, or, mocked, against the adapter's own emulator.
func (w *World) client() *todoapi.Client {
	logger := slog.New(slog.NewTextHandler(io.Discard, nil))
	if w.mocked {
		return todoapi.NewClient(httpclient.Config{Name: "todo-api", Host: httpclient.Mock, Timeout: time.Second}, logger, nil)
	}
	w.server = httptest.NewServer(http.HandlerFunc(func(rw http.ResponseWriter, r *http.Request) {
		body, _ := io.ReadAll(r.Body)
		w.method, w.path, w.body = r.Method, r.URL.Path, string(body)
		rw.Header().Set("Content-Type", "application/json")
		_, _ = io.WriteString(rw, w.answer)
	}))
	address, _ := url.Parse(w.server.URL)
	port, _ := strconv.Atoi(address.Port())
	return todoapi.NewClient(httpclient.Config{Name: "todo-api", Host: address.Hostname(), Port: port, Timeout: time.Second}, logger, nil)
}

func registerSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the todo API answers '([^']*)'$`, func(answer string) error { w.answer = answer; return nil })
	sc.Step(`^the todo API is mocked$`, func() error { w.mocked = true; return nil })
	sc.Step(`^I call PlanItems with the titles "([^"]*)"$`, func(titles string) error {
		w.items, w.callErr = w.client().PlanItems(context.Background(), strings.Split(titles, "|"))
		logf("when: PlanItems %s -> %+v, error %v", titles, w.items, w.callErr)
		return nil
	})
	sc.Step(`^the todo API should have received (\w+) "([^"]*)" with '([^']*)'$`, func(method, path, body string) error {
		if w.method != method || w.path != path || w.body != body {
			return fmt.Errorf("received %s %s %s, want %s %s %s", w.method, w.path, w.body, method, path, body)
		}
		return nil
	})
	sc.Step(`^the items should be "([^"]*)"$`, func(want string) error {
		var got []string
		for _, item := range w.items {
			state := "open"
			if item.Done {
				state = "done"
			}
			got = append(got, item.Title+":"+state)
		}
		if w.callErr != nil || strings.Join(got, "|") != want {
			return fmt.Errorf("items %v, error %v; want %s", got, w.callErr, want)
		}
		return nil
	})
	sc.Step(`^the adapter should call exactly these operations, each in the contract:$`, func(table *godog.Table) error {
		raw, err := os.ReadFile("../../../../../docs/integration/client/todo/openapi.json")
		if err != nil {
			return err
		}
		var contract struct {
			Paths map[string]map[string]json.RawMessage `json:"paths"`
		}
		if err := json.Unmarshal(raw, &contract); err != nil {
			return err
		}
		var want []httpclient.Operation
		for _, row := range table.Rows[1:] {
			want = append(want, httpclient.Operation{Method: row.Cells[0].Value, Path: row.Cells[1].Value, Retryable: row.Cells[2].Value == "yes"})
		}
		if fmt.Sprint(todoapi.Operations) != fmt.Sprint(want) {
			return fmt.Errorf("the adapter calls %v, want %v", todoapi.Operations, want)
		}
		for _, op := range todoapi.Operations {
			if _, ok := contract.Paths[op.Path][strings.ToLower(op.Method)]; !ok {
				return fmt.Errorf("%s %s is no operation of the contract", op.Method, op.Path)
			}
		}
		return nil
	})
}
