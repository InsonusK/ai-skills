package test

import (
	"context"
	"errors"
	"fmt"
	"log/slog"
	"strings"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/genproto/googleapis/rpc/status"
	"google.golang.org/grpc"
	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/types/known/wrapperspb"

	"github.com/example/todo/internal/infrastructure/client/core/outcome"
	"github.com/example/todo/internal/infrastructure/client/core/pipeline"
	"github.com/example/todo/internal/logging"
)

func registerCallSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the test API answers "([^"]*)"$`, func(text string) error { w.text = text; return nil })
	sc.Step(`^the test API fails with (\w+) and the message "([^"]*)"$`, func(code, message string) (err error) {
		w.failure, err = failWith(code, message)
		return err
	})
	sc.Step(`^the test API takes longer than the timeout$`, func() error {
		w.delay = 300 * time.Millisecond
		return nil
	})
	sc.Step(`^the test API is not listening$`, func() error { w.down = true; return nil })
	sc.Step(`^the client of "([^"]*)" is mocked by an emulator answering "([^"]*)"$`, func(_, text string) error {
		w.mocked = true
		w.emulator = func(server grpc.ServiceRegistrar) {
			server.RegisterService(echoService(func(context.Context, *wrapperspb.StringValue) (*wrapperspb.StringValue, error) {
				return wrapperspb.String(text), nil
			}))
		}
		return nil
	})

	sc.Step(`^a client middleware that sends every call twice$`, func() error {
		w.middlewares = append(w.middlewares, func(ctx context.Context, call pipeline.Call, next pipeline.Attempt) error {
			w.seen = append(w.seen, call)
			if err := next(ctx); err != nil {
				return err
			}
			return next(ctx)
		})
		return nil
	})
	sc.Step(`^the middleware should have seen the call "([^"]*)" of "([^"]*)", retryable "([^"]*)"$`, func(operation, api, retryable string) error {
		want := fmt.Sprint([]pipeline.Call{{API: api, Operation: operation, Retryable: retryable == "true"}})
		if got := fmt.Sprint(w.seen); got != want {
			return fmt.Errorf("seen %s, want %s", got, want)
		}
		return nil
	})
	sc.Step(`^the test API should have received (\d+) calls$`, func(n int) error {
		if len(w.received) != n {
			return fmt.Errorf("received %d calls, want %d", len(w.received), n)
		}
		return nil
	})
	sc.Step(`^I call the unlisted method "([^"]*)"$`, func(method string) error {
		if err := w.connect(); err != nil {
			return err
		}
		w.callErr = w.client.Invoke(context.Background(), method, wrapperspb.String("hi"), &wrapperspb.StringValue{})
		return nil
	})
	sc.Step(`^the call should fail mentioning "([^"]*)"$`, func(text string) error {
		if w.callErr == nil || !strings.Contains(w.callErr.Error(), text) {
			return fmt.Errorf("error %v, want one mentioning %q", w.callErr, text)
		}
		return nil
	})

	call := func(ctx context.Context, text string) error {
		if err := w.connect(); err != nil {
			return err
		}
		w.answer = &wrapperspb.StringValue{}
		w.callErr = w.client.Invoke(ctx, echoMethod, wrapperspb.String(text), w.answer)
		logf("when: Echo %q -> %q, error %v", text, w.answer.GetValue(), w.callErr)
		return nil
	}
	sc.Step(`^I call Echo with the text "([^"]*)"$`, func(text string) error {
		return call(context.Background(), text)
	})
	sc.Step(`^I call Echo with the text "([^"]*)" in the request "([^"]*)"$`, func(text, id string) error {
		return call(logging.WithRequestID(context.Background(), id), text)
	})

	sc.Step(`^the call should succeed with the text "([^"]*)"$`, func(text string) error {
		if w.callErr != nil || w.answer.GetValue() != text {
			return fmt.Errorf("got %q, error %v; want the text %q", w.answer.GetValue(), w.callErr, text)
		}
		return nil
	})
	sc.Step(`^the test API should have received Echo with the text "([^"]*)"$`, func(text string) error {
		if len(w.received) != 1 || w.received[0].text != text {
			return fmt.Errorf("received %+v, want one Echo with %q", w.received, text)
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
	sc.Step(`^the call should fail with the code (\w+) and the message "([^"]*)"$`, func(name, message string) error {
		code, err := codeNamed(name)
		if err != nil {
			return err
		}
		var got *outcome.StatusError
		if !errors.As(w.callErr, &got) || got.Code != outcome.Code(code) || got.Message != message {
			return fmt.Errorf("error %v, want a status error %s %q", w.callErr, code, message)
		}
		return nil
	})
	sc.Step(`^the call should fail as unavailable$`, func() error {
		if !errors.Is(w.callErr, outcome.ErrUnavailable) {
			return fmt.Errorf("error %v, want unavailable", w.callErr)
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
	holdsText := func(value any, text string) bool {
		message, ok := value.(proto.Message)
		return ok && proto.Equal(message, wrapperspb.String(text))
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
		if !holdsText(p["request"], text) {
			return fmt.Errorf("request %v, want the text %q", p["request"], text)
		}
		return nil
	})
	sc.Step(`^one "grpc client call answered" record should be logged with the API "([^"]*)", the method "([^"]*)", the code "([^"]*)", and failed "([^"]*)"$`, func(api, method, code, failed string) error {
		record, err := only("grpc client call answered")
		if err != nil {
			return err
		}
		got := fmt.Sprintf("%v %v %v %v", record["api"], record["method"], record["code"], record[logging.FailedKey])
		if want := fmt.Sprintf("%s %s %s %s", api, method, code, failed); got != want {
			return fmt.Errorf("record %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the "([^"]*)" record should hold the response text "([^"]*)" and no request$`, func(message, text string) error {
		p, err := payload(message)
		if err != nil {
			return err
		}
		if _, has := p["request"]; has || !holdsText(p["response"], text) {
			return fmt.Errorf("payload %v, want the response %q and no request", p, text)
		}
		return nil
	})
	sc.Step(`^the "([^"]*)" record should hold the status message "([^"]*)"$`, func(message, text string) error {
		p, err := payload(message)
		if err != nil {
			return err
		}
		if got, ok := p["status"].(*status.Status); !ok || got.GetMessage() != text {
			return fmt.Errorf("status %v, want the message %q", p["status"], text)
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
