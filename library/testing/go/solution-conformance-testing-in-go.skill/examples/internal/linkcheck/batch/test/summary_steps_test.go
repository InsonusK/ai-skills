package test

import (
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck/internal/linkcheck/batch"
)

func registerSummarySteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the URLs:$`, w.theURLs)
	sc.Step(`^no URLs$`, w.noURLs)
	sc.Step(`^I summarize the batch$`, w.iSummarizeTheBatch)
	sc.Step(`^the batch has (\d+) valid and (\d+) invalid URLs$`, w.theBatchHas)
	sc.Step(`^the count of "([^"]*)" errors is (\d+)$`, w.theCountOfErrorsIs)
}

func (w *World) theURLs(ctx context.Context, table *godog.Table) error {
	for _, row := range table.Rows[1:] {
		w.urls = append(w.urls, row.Cells[0].Value)
	}
	logf("given: urls=%q", w.urls)
	return nil
}

func (w *World) noURLs(ctx context.Context) error {
	logf("given: urls=[]")
	w.urls = nil
	return nil
}

func (w *World) iSummarizeTheBatch(ctx context.Context) error {
	w.summary = batch.Summarize(w.urls)
	logf("when: Summarize(%q) -> %+v", w.urls, w.summary)
	return nil
}

func (w *World) theBatchHas(ctx context.Context, valid, invalid int) error {
	logf("then: valid=%d invalid=%d want=%d/%d", w.summary.Valid, w.summary.Invalid, valid, invalid)
	if w.summary.Valid != valid || w.summary.Invalid != invalid {
		return fmt.Errorf("totals: got %d valid and %d invalid, want %d and %d", w.summary.Valid, w.summary.Invalid, valid, invalid)
	}
	return nil
}

func (w *World) theCountOfErrorsIs(ctx context.Context, errorCode string, want int) error {
	got := w.summary.Errors[errorCode]
	logf("then: errors[%q]=%d want=%d", errorCode, got, want)
	if got != want {
		return fmt.Errorf("count of %s: got %d, want %d", errorCode, got, want)
	}
	return nil
}
