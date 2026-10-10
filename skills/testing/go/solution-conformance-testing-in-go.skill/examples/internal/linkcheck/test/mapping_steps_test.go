package test

import (
	"context"
	"fmt"
	"github.com/cucumber/godog"
	"github.com/example/linkcheck/internal/linkcheck"
)

func registerMappingSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^I map the result to a record and back$`, func(ctx context.Context) error {
		w.record = linkcheck.ToRecord(w.result)
		w.mapped, w.mappingErr = linkcheck.FromRecord(w.record)
		logf("round trip=%+v error=%v", w.mapped, w.mappingErr)
		return w.mappingErr
	})
	sc.Step(`^the result is unchanged$`, func(ctx context.Context) error {
		logf("mapped=%+v original=%+v", w.mapped, w.result)
		if w.mapped != w.result {
			return fmt.Errorf("round trip changed result")
		}
		return nil
	})
	sc.Step(`^the record:$`, func(ctx context.Context, table *godog.Table) error {
		w.record = map[string]any{}
		for _, r := range table.Rows[1:] {
			w.record[r.Cells[0].Value] = r.Cells[1].Value
		}
		logf("record=%v", w.record)
		return nil
	})
	sc.Step(`^I map the record to a result$`, func(ctx context.Context) error {
		w.mapped, w.mappingErr = linkcheck.FromRecord(w.record)
		logf("mapping error=%v", w.mappingErr)
		return nil
	})
	sc.Step(`^the mapping fails with error "([^"]*)"$`, func(ctx context.Context, want string) error {
		logf("mapping error=%v want=%s", w.mappingErr, want)
		if w.mappingErr == nil || w.mappingErr.Error() != want {
			return fmt.Errorf("mapping error got %v want %s", w.mappingErr, want)
		}
		return nil
	})

}
