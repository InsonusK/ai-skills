package test

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log/slog"
	"strings"

	"github.com/cucumber/godog"

	commandlogging "github.com/example/todo/internal/command/middleware/logging"
	"github.com/example/todo/internal/command/pipeline"
	"github.com/example/todo/internal/shared/errorkind"
)

func registerLoggingSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a handler of PlanItems that returns "([^"]*)", wrapped with logging$`, w.aSucceedingHandler)
	sc.Step(`^a handler of PlanItems that fails with "([^"]*)", wrapped with logging$`, w.aFailingHandler)
	sc.Step(`^a handler of PlanItems that fails as unavailable with "([^"]*)" caused by "([^"]*)", wrapped with logging$`, w.aHandlerFailingWithACause)
	sc.Step(`^the wrapped handler handles PlanItems for "([^"]*)"$`, w.theWrappedHandlerHandles)
	sc.Step(`^the wrapped handler should fail with "([^"]*)"$`, w.theWrappedHandlerShouldReturn)
	sc.Step(`^the records should be:$`, w.theRecordsShouldBe)
}

// wrap logs through a plain JSON handler, which keeps the payload, so the
// scenarios see every attribute the middleware writes - what the base handler
// then drops is the logging package's scenarios' concern.
// PlanItems is the command of the scenarios: one title, logged as {"title": ...}.
type PlanItems struct {
	Title string `json:"title"`
}

func (w *World) wrap(inner pipeline.HandlerFunc[PlanItems, string]) {
	logger := slog.New(slog.NewJSONHandler(&w.logs, nil))
	w.wrapped = pipeline.Decorate[PlanItems, string]("PlanItems", inner, []pipeline.Middleware{commandlogging.Middleware(logger)})
}

func (w *World) aSucceedingHandler(ctx context.Context, result string) error {
	w.wrap(func(context.Context, PlanItems) (string, error) { return result, nil })
	return nil
}

func (w *World) aFailingHandler(ctx context.Context, message string) error {
	w.wrap(func(context.Context, PlanItems) (string, error) { return "", errors.New(message) })
	return nil
}

func (w *World) aHandlerFailingWithACause(ctx context.Context, message, cause string) error {
	w.wrap(func(context.Context, PlanItems) (string, error) {
		return "", errorkind.Wrap(errorkind.ErrUnavailable, message, errors.New(cause))
	})
	return nil
}

func (w *World) theWrappedHandlerHandles(ctx context.Context, title string) error {
	_, w.err = w.wrapped.Handle(ctx, PlanItems{Title: title})
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
			text, isText := got.(string)
			if !isText {
				raw, _ := json.Marshal(got)
				text = string(raw)
			}
			if text != want {
				return fmt.Errorf("record %d: %s=%v, want %s", i, key, got, want)
			}
		}
		if w.err != nil && rec["level"] == "WARN" && rec["error"] != w.err.Error() {
			return fmt.Errorf("record %d error: got %v, want %q", i, rec["error"], w.err.Error())
		}
	}
	return nil
}
