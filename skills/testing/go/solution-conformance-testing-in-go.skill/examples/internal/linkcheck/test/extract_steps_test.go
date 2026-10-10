package test

import (
	"context"
	"fmt"
	"slices"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck/internal/linkcheck"
)

func registerExtractSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the text "([^"]*)"$`, w.theText)
	sc.Step(`^I extract the links$`, w.iExtractTheLinks)
	sc.Step(`^the links are:$`, w.theLinksAre)
	sc.Step(`^the number of links is (\d+)$`, w.theNumberOfLinksIs)
}

func (w *World) theText(ctx context.Context, text string) error {
	logf("given: text=%q", text)
	w.text = text
	return nil
}

func (w *World) iExtractTheLinks(ctx context.Context) error {
	w.links = linkcheck.ExtractLinks(w.text)
	logf("when: ExtractLinks(%q) -> %q", w.text, w.links)
	return nil
}

func (w *World) theLinksAre(ctx context.Context, table *godog.Table) error {
	want := []string{}
	for _, row := range table.Rows[1:] {
		want = append(want, row.Cells[0].Value)
	}
	logf("then: links=%q want=%q", w.links, want)
	if !slices.Equal(w.links, want) {
		return fmt.Errorf("links: got %q, want %q", w.links, want)
	}
	return nil
}

func (w *World) theNumberOfLinksIs(ctx context.Context, want int) error {
	logf("then: number of links=%d want=%d", len(w.links), want)
	if len(w.links) != want {
		return fmt.Errorf("number of links: got %d (%q), want %d", len(w.links), w.links, want)
	}
	return nil
}
