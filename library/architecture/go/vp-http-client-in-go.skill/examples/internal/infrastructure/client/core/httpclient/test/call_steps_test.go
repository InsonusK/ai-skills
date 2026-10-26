package test

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log/slog"
	"net/http"
	"reflect"
	"strings"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/client/core/httpclient"
	"github.com/example/todo/internal/logging"
)

func registerCallSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the test API answers (\d+) with '([^']*)'$`, func(status int, body string) error {
		w.status, w.body = status, body
		return nil
	})
	sc.Step(`^the test API takes longer than the timeout$`, func() error {
		w.delay = 300 * time.Millisecond
		return nil
	})
	sc.Step(`^the test API is not listening$`, func() error { w.down = true; return nil })
	sc.Step(`^the client of "([^"]*)" is mocked by an emulator answering '([^']*)'$`, func(_ string, body string) error {
		w.mocked = true
		w.emulator = http.HandlerFunc(func(rw http.ResponseWriter, _ *http.Request) {
			rw.WriteHeader(http.StatusOK)
			_, _ = rw.Write([]byte(body))
		})
		return nil
	})

	call := func(ctx context.Context, method, path, text string) {
		w.callErr = w.client().Call(ctx, method, path, echo{Text: text}, &w.answer)
		logf("when: %s %s -> %+v, error %v", method, path, w.answer, w.callErr)
	}
	sc.Step(`^I call (\w+) "([^"]*)" with the text "([^"]*)"$`, func(method, path, text string) error {
		call(context.Background(), method, path, text)
		return nil
	})
	sc.Step(`^I call (\w+) "([^"]*)" with the text "([^"]*)" in the request "([^"]*)"$`, func(method, path, text, id string) error {
		call(logging.WithRequestID(context.Background(), id), method, path, text)
		return nil
	})

	sc.Step(`^the call should succeed with the text "([^"]*)"$`, func(text string) error {
		if w.callErr != nil || w.answer.Text != text {
			return fmt.Errorf("got %+v, error %v; want the text %q", w.answer, w.callErr, text)
		}
		return nil
	})
	sc.Step(`^the test API should have received (\w+) "([^"]*)" with '([^']*)'$`, func(method, path, body string) error {
		if len(w.received) != 1 {
			return fmt.Errorf("received %d requests, want 1", len(w.received))
		}
		got := w.received[0]
		if got.method != method || got.path != path || got.body != body {
			return fmt.Errorf("received %s %s %s, want %s %s %s", got.method, got.path, got.body, method, path, body)
		}
		return nil
	})
	sc.Step(`^the test API should have received the request id "([^"]*)"$`, func(id string) error {
		if len(w.received) != 1 || w.received[0].requestID != id {
			return fmt.Errorf("received %+v, want the request id %q", w.received, id)
		}
		return nil
	})
	sc.Step(`^the test API should have received no request id$`, func() error {
		if len(w.received) != 1 || w.received[0].hasRequestID {
			return fmt.Errorf("received %+v, want no request id", w.received)
		}
		return nil
	})
	sc.Step(`^the call should fail with the status (\d+), the code (\d+), and the message "([^"]*)"$`, func(status, code int, message string) error {
		var got *httpclient.StatusError
		if !errors.As(w.callErr, &got) {
			return fmt.Errorf("error %v, want a status error", w.callErr)
		}
		if got.HTTPStatus != status || got.Code != code || got.Message != message {
			return fmt.Errorf("got %+v, want %d, %d, %q", got, status, code, message)
		}
		return nil
	})
	sc.Step(`^the call should fail as unavailable$`, func() error {
		if !errors.Is(w.callErr, httpclient.ErrUnavailable) {
			return fmt.Errorf("error %v, want unavailable", w.callErr)
		}
		return nil
	})
	sc.Step(`^the call should fail mentioning "([^"]*)"$`, func(text string) error {
		var status *httpclient.StatusError
		if w.callErr == nil || errors.Is(w.callErr, httpclient.ErrUnavailable) || errors.As(w.callErr, &status) || !strings.Contains(w.callErr.Error(), text) {
			return fmt.Errorf("error %v, want neither unavailable nor a status, mentioning %q", w.callErr, text)
		}
		return nil
	})

	// call_log.feature and mock.feature

	messages := func() []string {
		var got []string
		for _, record := range w.records.records {
			got = append(got, record.Message)
		}
		return got
	}
	only := func(message string) (map[string]any, error) {
		var found []map[string]any
		for _, record := range w.records.records {
			if record.Message == message {
				found = append(found, attrs(record))
			}
		}
		if len(found) != 1 {
			return nil, fmt.Errorf("%d records %q, want 1; records %v", len(found), message, messages())
		}
		return found[0], nil
	}
	payload := func(message string) (map[string]any, error) {
		record, err := only(message)
		if err != nil {
			return nil, err
		}
		p, ok := record[logging.PayloadKey].(map[string]any)
		if !ok {
			return nil, fmt.Errorf("payload %v is no map", record[logging.PayloadKey])
		}
		return p, nil
	}
	sc.Step(`^the records should be, in order, "([^"]*)"$`, func(want string) error {
		if got := strings.Join(messages(), "|"); got != want {
			return fmt.Errorf("records %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the "([^"]*)" record should hold the request text "([^"]*)"$`, func(message, text string) error {
		p, err := payload(message)
		if err != nil {
			return err
		}
		if !reflect.DeepEqual(p["request"], echo{Text: text}) {
			return fmt.Errorf("request %v, want the text %q", p["request"], text)
		}
		return nil
	})
	sc.Step(`^one "http client response received" record should be logged with the API "([^"]*)", (\w+) "([^"]*)", the status (\d+), and failed "([^"]*)"$`, func(api, method, path string, status int, failed string) error {
		record, err := only("http client response received")
		if err != nil {
			return err
		}
		got := fmt.Sprintf("%v %v %v %v %v", record["api"], record["method"], record["path"], record["status"], record[logging.FailedKey])
		if want := fmt.Sprintf("%s %s %s %d %s", api, method, path, status, failed); got != want {
			return fmt.Errorf("record %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the "([^"]*)" record should hold the response '([^']*)' and no request$`, func(message, body string) error {
		p, err := payload(message)
		if err != nil {
			return err
		}
		response, _ := p["response"].([]byte)
		var got, want any
		_ = json.Unmarshal(response, &got)
		_ = json.Unmarshal([]byte(body), &want)
		if _, has := p["request"]; has || !reflect.DeepEqual(got, want) {
			return fmt.Errorf("payload %v, want the response %s and no request", p, body)
		}
		return nil
	})
	sc.Step(`^the "api client mocked" record should be a warning naming the API "([^"]*)"$`, func(api string) error {
		for _, record := range w.records.records {
			if record.Message == "api client mocked" && record.Level == slog.LevelWarn && attrs(record)["api"] == api {
				return nil
			}
		}
		return fmt.Errorf("no WARN record naming %q: %v", api, messages())
	})
}
