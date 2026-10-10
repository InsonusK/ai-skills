package test

import (
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck/internal/linkcheck"
)

func registerCheckSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the URL "([^"]*)"$`, w.theURL)
	sc.Step(`^I check the URL$`, w.iCheckTheURL)
	sc.Step(`^the check is valid$`, w.theCheckIsValid)
	sc.Step(`^the normalized URL is "([^"]*)"$`, w.theNormalizedURLIs)
	sc.Step(`^the check is invalid with error "([^"]*)"$`, w.theCheckIsInvalidWithError)
}

func (w *World) theURL(ctx context.Context, input string) error {
	logf("given: url=%q", input)
	w.input = input
	return nil
}

func (w *World) iCheckTheURL(ctx context.Context) error {
	w.result = linkcheck.Check(w.input)
	logf("when: Check(%q) -> %+v", w.input, w.result)
	return nil
}

func (w *World) theCheckIsValid(ctx context.Context) error {
	logf("then: is_valid=%t want=true", w.result.IsValid)
	if !w.result.IsValid {
		return fmt.Errorf("check: got invalid (%s), want valid", w.result.ErrorCode)
	}
	return nil
}

func (w *World) theNormalizedURLIs(ctx context.Context, want string) error {
	logf("then: normalized=%q want=%q", w.result.Normalized, want)
	if w.result.Normalized != want {
		return fmt.Errorf("normalized URL: got %q, want %q", w.result.Normalized, want)
	}
	return nil
}

func (w *World) theCheckIsInvalidWithError(ctx context.Context, want string) error {
	logf("then: error_code=%q want=%q", w.result.ErrorCode, want)
	if w.result.IsValid {
		return fmt.Errorf("check: got valid, want invalid with %s", want)
	}
	if w.result.ErrorCode != want {
		return fmt.Errorf("error code: got %q, want %q", w.result.ErrorCode, want)
	}
	return nil
}
