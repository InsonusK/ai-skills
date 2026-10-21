package test

import (
	"context"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"net/http"
	"regexp"
	"strings"

	"github.com/cucumber/godog"

	httpapi "github.com/example/todo/internal/api/http"
)

func registerRequestLogSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^I POST "([^"]*)" with the body '([^']*)' and the request id "([^"]*)"$`, func(path, body, id string) error {
		w.sendWithHeaders(http.MethodPost, path, body, map[string]string{httpapi.RequestIDHeader: id})
		return nil
	})
	sc.Step(`^one "([^"]*)" record should be logged with:$`, w.oneRecordShouldBeLoggedWith)
	sc.Step(`^its payload should hold the query "([^"]*)" = "([^"]*)" and the body '([^']*)'$`, w.itsPayloadShouldHold)
	sc.Step(`^the response should carry the request id "([^"]*)"$`, func(want string) error {
		if got := w.response.Header().Get(httpapi.RequestIDHeader); got != want {
			return fmt.Errorf("response request id: got %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the response should carry a request id of 16 hex characters$`, func() error {
		if got := w.response.Header().Get(httpapi.RequestIDHeader); !regexp.MustCompile(`^[0-9a-f]{16}$`).MatchString(got) {
			return fmt.Errorf("response request id: got %q", got)
		}
		return nil
	})
	sc.Step(`^the context of the command should carry the request id "([^"]*)"$`, func(want string) error {
		if w.echoReqID != want {
			return fmt.Errorf("request id in the command's context: got %q, want %q", w.echoReqID, want)
		}
		return nil
	})
}

// records are the logged records of the given msg.
func (w *World) records(msg string) []map[string]any {
	found := []map[string]any{}
	for _, line := range strings.Split(strings.TrimSpace(w.logs.String()), "\n") {
		var rec map[string]any
		if json.Unmarshal([]byte(line), &rec) == nil && rec["msg"] == msg {
			found = append(found, rec)
		}
	}
	return found
}

func (w *World) oneRecordShouldBeLoggedWith(ctx context.Context, msg string, table *godog.Table) error {
	recs := w.records(msg)
	if len(recs) != 1 {
		return fmt.Errorf("%q records: got %d, want 1 (log %s)", msg, len(recs), w.logs.String())
	}
	for j, cell := range table.Rows[1].Cells {
		key := table.Rows[0].Cells[j].Value
		if got := fmt.Sprint(recs[0][key]); got != cell.Value {
			return fmt.Errorf("record %s=%s, want %s", key, got, cell.Value)
		}
	}
	return nil
}

func (w *World) itsPayloadShouldHold(ctx context.Context, name, value, body string) error {
	recs := w.records("http request")
	if len(recs) != 1 {
		return fmt.Errorf("http request records: got %d, want 1", len(recs))
	}
	payload, _ := recs[0]["payload"].(map[string]any)
	query, _ := payload["query"].(map[string]any)
	values, _ := query[name].([]any)
	raw, _ := payload["body"].(string) // []byte is written as base64 by the JSON handler
	decoded, err := base64.StdEncoding.DecodeString(raw)
	if len(values) != 1 || values[0] != value || err != nil || string(decoded) != body {
		return fmt.Errorf("payload %v, want query %s=%s and body %s", payload, name, value, body)
	}
	return nil
}
