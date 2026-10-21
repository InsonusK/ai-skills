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

	httppipeline "github.com/example/todo/internal/api/http/pipeline"
)

func registerRequestLogSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^I POST "([^"]*)" with the body '([^']*)' and the request id "([^"]*)"$`, func(path, body, id string) error {
		w.sendWithHeaders(http.MethodPost, path, body, map[string]string{httppipeline.RequestIDHeader: id})
		return nil
	})
	sc.Step(`^one "([^"]*)" record should be logged with:$`, w.oneRecordShouldBeLoggedWith)
	sc.Step(`^the command is wrapped with logging$`, func() error { w.decorated = true; return nil })
	sc.Step(`^the records should be, in order, "([^"]*)"$`, w.theRecordsShouldBeInOrder)
	sc.Step(`^the "([^"]*)" record should hold the query "([^"]*)" = "([^"]*)" and the body '([^']*)'$`, w.theRecordShouldHoldTheRequest)
	sc.Step(`^the "([^"]*)" record should hold the response body '([^']*)'$`, w.theRecordShouldHoldTheResponse)
	sc.Step(`^the "([^"]*)" record should hold no request$`, w.theRecordShouldHoldNoRequest)
	sc.Step(`^the response should carry the request id "([^"]*)"$`, func(want string) error {
		if got := w.response.Header().Get(httppipeline.RequestIDHeader); got != want {
			return fmt.Errorf("response request id: got %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the response should carry a request id of 16 hex characters$`, func() error {
		if got := w.response.Header().Get(httppipeline.RequestIDHeader); !regexp.MustCompile(`^[0-9a-f]{16}$`).MatchString(got) {
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

func (w *World) theRecordsShouldBeInOrder(ctx context.Context, msgs string) error {
	got := []string{}
	for _, line := range strings.Split(strings.TrimSpace(w.logs.String()), "\n") {
		var rec map[string]any
		if json.Unmarshal([]byte(line), &rec) == nil {
			got = append(got, fmt.Sprint(rec["msg"]))
		}
	}
	if strings.Join(got, "|") != msgs {
		return fmt.Errorf("records in order: got %q, want %q", strings.Join(got, "|"), msgs)
	}
	return nil
}

// payloadOf is the payload of the one record msg.
func (w *World) payloadOf(msg string) (map[string]any, error) {
	recs := w.records(msg)
	if len(recs) != 1 {
		return nil, fmt.Errorf("%q records: got %d, want 1 (log %s)", msg, len(recs), w.logs.String())
	}
	payload, _ := recs[0]["payload"].(map[string]any)
	return payload, nil
}

// theRecordShouldHoldTheRequest reads what the client sent: the payload
// itself in the received record, its "request" part in the sent record.
func (w *World) theRecordShouldHoldTheRequest(ctx context.Context, msg, name, value, body string) error {
	payload, err := w.payloadOf(msg)
	if err != nil {
		return err
	}
	request := payload
	if part, ok := payload["request"].(map[string]any); ok {
		request = part
	}
	query, _ := request["query"].(map[string]any)
	values, _ := query[name].([]any)
	queryOK := (value == "" && len(values) == 0) || (len(values) == 1 && values[0] == value)
	if !queryOK || decodeBody(request) != body {
		return fmt.Errorf("payload %v, want query %s=%q and body %s", payload, name, value, body)
	}
	return nil
}

func (w *World) theRecordShouldHoldTheResponse(ctx context.Context, msg, body string) error {
	payload, err := w.payloadOf(msg)
	if err != nil {
		return err
	}
	response, _ := payload["response"].(map[string]any)
	if got := strings.TrimSpace(decodeBody(response)); got != body {
		return fmt.Errorf("response body in the payload: got %q, want %q", got, body)
	}
	return nil
}

func (w *World) theRecordShouldHoldNoRequest(ctx context.Context, msg string) error {
	payload, err := w.payloadOf(msg)
	if err != nil {
		return err
	}
	if _, has := payload["request"]; has {
		return fmt.Errorf("payload %v holds the request", payload)
	}
	return nil
}

// decodeBody is the body of a payload part: []byte is written as base64 by
// the plain JSON handler the scenarios log through.
func decodeBody(part map[string]any) string {
	raw, _ := part["body"].(string)
	decoded, _ := base64.StdEncoding.DecodeString(raw)
	return string(decoded)
}
