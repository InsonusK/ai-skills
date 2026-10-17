package test

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log/slog"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/application"
)

func registerLoggingSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a handler that succeeds, wrapped with logging as "([^"]*)"$`, w.aSucceedingHandler)
	sc.Step(`^a handler that fails with "([^"]*)", wrapped with logging as "([^"]*)"$`, w.aFailingHandler)
	sc.Step(`^the wrapped handler handles a command$`, w.theWrappedHandlerHandles)
	sc.Step(`^the wrapped handler should return the error "([^"]*)"$`, w.theWrappedHandlerShouldReturn)
	sc.Step(`^one "([^"]*)" record "([^"]*)" should be logged for "([^"]*)"$`, w.oneRecordShouldBeLogged)
}

func (w *World) wrap(name string, inner application.HandlerFunc[string, string]) {
	logger := slog.New(slog.NewJSONHandler(&w.logs, nil))
	w.wrapped = application.WithLogging[string, string](name, logger, inner)
}

func (w *World) aSucceedingHandler(ctx context.Context, name string) error {
	w.wrap(name, func(context.Context, string) (string, error) { return "ok", nil })
	return nil
}

func (w *World) aFailingHandler(ctx context.Context, message, name string) error {
	w.wrap(name, func(context.Context, string) (string, error) { return "", errors.New(message) })
	return nil
}

func (w *World) theWrappedHandlerHandles(ctx context.Context) error {
	_, w.err = w.wrapped.Handle(ctx, "cmd")
	logf("when: handle -> err=%v logs=%s", w.err, strings.TrimSpace(w.logs.String()))
	return nil
}

func (w *World) theWrappedHandlerShouldReturn(ctx context.Context, message string) error {
	if w.err == nil || w.err.Error() != message {
		return fmt.Errorf("error: got %v, want %q", w.err, message)
	}
	return nil
}

func (w *World) oneRecordShouldBeLogged(ctx context.Context, level, msg, command string) error {
	lines := strings.Split(strings.TrimSpace(w.logs.String()), "\n")
	if len(lines) != 1 {
		return fmt.Errorf("records: got %d, want 1", len(lines))
	}
	var rec map[string]any
	if err := json.Unmarshal([]byte(lines[0]), &rec); err != nil {
		return fmt.Errorf("record is not JSON: %w", err)
	}
	if rec["level"] != level || rec["msg"] != msg || rec["command"] != command {
		return fmt.Errorf("record: got %v, want level=%s msg=%q command=%s", rec, level, msg, command)
	}
	if w.err != nil && rec["error"] != w.err.Error() {
		return fmt.Errorf("record error: got %v, want %q", rec["error"], w.err.Error())
	}
	return nil
}
