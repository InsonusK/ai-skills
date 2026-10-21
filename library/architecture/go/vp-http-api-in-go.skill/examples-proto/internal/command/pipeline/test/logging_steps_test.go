package test

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log/slog"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/pipeline"
)

func registerLoggingSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a handler that succeeds, wrapped with logging as "([^"]*)"$`, w.aSucceedingHandler)
	sc.Step(`^a handler that fails with "([^"]*)", wrapped with logging as "([^"]*)"$`, w.aFailingHandler)
	sc.Step(`^the wrapped handler handles the command "([^"]*)"$`, w.theWrappedHandlerHandles)
	sc.Step(`^the wrapped handler should return the error "([^"]*)"$`, w.theWrappedHandlerShouldReturn)
	sc.Step(`^the records should be:$`, w.theRecordsShouldBe)
}

// wrap logs through a plain JSON handler, which keeps the payload, so the
// scenarios see every attribute the decorator writes - what the base handler
// then drops is the logging package's scenarios' concern.
func (w *World) wrap(name string, inner pipeline.HandlerFunc[string, string]) {
	logger := slog.New(slog.NewJSONHandler(&w.logs, nil))
	w.wrapped = pipeline.WithLogging[string, string](name, logger, inner)
}

func (w *World) aSucceedingHandler(ctx context.Context, name string) error {
	w.wrap(name, func(context.Context, string) (string, error) { return "ok", nil })
	return nil
}

func (w *World) aFailingHandler(ctx context.Context, message, name string) error {
	w.wrap(name, func(context.Context, string) (string, error) { return "", errors.New(message) })
	return nil
}

func (w *World) theWrappedHandlerHandles(ctx context.Context, cmd string) error {
	_, w.err = w.wrapped.Handle(ctx, cmd)
	logf("when: handle -> err=%v logs=%s", w.err, strings.TrimSpace(w.logs.String()))
	return nil
}

func (w *World) theWrappedHandlerShouldReturn(ctx context.Context, message string) error {
	if w.err == nil || w.err.Error() != message {
		return fmt.Errorf("error: got %v, want %q", w.err, message)
	}
	return nil
}

// theRecordsShouldBe compares the records, in order, with the table; an empty
// cell means the record has no such attribute.
func (w *World) theRecordsShouldBe(ctx context.Context, table *godog.Table) error {
	lines := strings.Split(strings.TrimSpace(w.logs.String()), "\n")
	if len(lines) != len(table.Rows)-1 {
		return fmt.Errorf("records: got %d, want %d", len(lines), len(table.Rows)-1)
	}
	header := table.Rows[0].Cells
	for i, row := range table.Rows[1:] {
		var rec map[string]any
		if err := json.Unmarshal([]byte(lines[i]), &rec); err != nil {
			return fmt.Errorf("record %d is not JSON: %w", i, err)
		}
		for j, cell := range row.Cells {
			key, want := header[j].Value, cell.Value
			got, has := rec[key]
			if want == "" {
				if has {
					return fmt.Errorf("record %d has %s=%v, want none", i, key, got)
				}
				continue
			}
			if fmt.Sprint(got) != want {
				return fmt.Errorf("record %d: %s=%v, want %s", i, key, got, want)
			}
		}
		if w.err != nil && rec["level"] == "WARN" && rec["error"] != w.err.Error() {
			return fmt.Errorf("record %d error: got %v, want %q", i, rec["error"], w.err.Error())
		}
	}
	return nil
}
