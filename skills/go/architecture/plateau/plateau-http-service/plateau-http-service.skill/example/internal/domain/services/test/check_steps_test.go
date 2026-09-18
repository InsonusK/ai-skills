package test

import (
	"context"
	"fmt"

	"github.com/cucumber/godog"
)

func registerCheckSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the URL "([^"]*)"$`, w.theURL)
	sc.Step(`^I check the URL$`, w.iCheckTheURL)
	sc.Step(`^the check should be "(valid|invalid)"$`, w.theCheckShouldBe)
	sc.Step(`^the normalized URL should be "([^"]*)"$`, w.theNormalizedURLShouldBe)
}

func (w *World) theURL(ctx context.Context, input string) error {
	logf("given: url=%q", input)
	w.input = input
	return nil
}

func (w *World) iCheckTheURL(ctx context.Context) error {
	w.result, w.err = w.svc.Check(ctx, w.input)
	logf("when: check(%q) -> result=%+v err=%v", w.input, w.result, w.err)
	return nil
}

func (w *World) theCheckShouldBe(ctx context.Context, outcome string) error {
	got := "valid"
	if w.err != nil {
		got = "invalid"
	}
	logf("then: outcome=%s want=%s", got, outcome)
	if got != outcome {
		return fmt.Errorf("check outcome: got %s, want %s (err=%v)", got, outcome, w.err)
	}
	return nil
}

func (w *World) theNormalizedURLShouldBe(ctx context.Context, want string) error {
	logf("then: normalized=%q want=%q", w.result.Normalized, want)
	if want == "" {
		return nil // invalid-outcome rows leave this column blank
	}
	if w.result.Normalized != want {
		return fmt.Errorf("normalized URL: got %q, want %q", w.result.Normalized, want)
	}
	return nil
}
