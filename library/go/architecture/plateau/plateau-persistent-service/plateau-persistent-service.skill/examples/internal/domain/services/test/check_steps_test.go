package test

import (
	"context"
	"fmt"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck-service/internal/domain/interfaces"
)

func registerCheckSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the URL "([^"]*)"$`, w.theURL)
	sc.Step(`^I check the URL$`, w.iCheckTheURL)
	sc.Step(`^the check should be "(valid|invalid)"$`, w.theCheckShouldBe)
	sc.Step(`^the normalized URL should be "([^"]*)"$`, w.theNormalizedURLShouldBe)
	sc.Step(`^the reputation service reports the URL as flagged with reason "([^"]*)"$`, w.theReputationServiceReportsFlagged)
	sc.Step(`^the reputation service is unavailable$`, w.theReputationServiceIsUnavailable)
	sc.Step(`^the URL should be flagged with reason "([^"]*)"$`, w.theURLShouldBeFlaggedWithReason)
	sc.Step(`^the URL should not be flagged$`, w.theURLShouldNotBeFlagged)
	sc.Step(`^the reputation service is never called$`, w.theReputationServiceIsNeverCalled)
	sc.Step(`^the reputation cache already has "([^"]*)" flagged with reason "([^"]*)"$`, w.theReputationCacheAlreadyHasFlagged)
	sc.Step(`^the reputation cache should have been written to$`, w.theReputationCacheShouldHaveBeenWrittenTo)
	sc.Step(`^the link history should have (\d+) entry$`, w.theLinkHistoryShouldHaveEntries)
	sc.Step(`^the link history is unavailable$`, w.theLinkHistoryIsUnavailable)
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

func (w *World) theReputationServiceReportsFlagged(ctx context.Context, reason string) error {
	logf("given: reputation service will report flagged=true reason=%q", reason)
	w.reputation.reputation = interfaces.Reputation{Flagged: true, Reason: reason}
	return nil
}

func (w *World) theReputationServiceIsUnavailable(ctx context.Context) error {
	logf("given: reputation service will report ErrUnavailable")
	w.reputation.err = interfaces.ErrUnavailable
	return nil
}

func (w *World) theURLShouldBeFlaggedWithReason(ctx context.Context, reason string) error {
	logf("then: flagged=%v reason=%q want reason=%q", w.result.Flagged, w.result.Reason, reason)
	if !w.result.Flagged {
		return fmt.Errorf("expected the result to be flagged, it was not")
	}
	if w.result.Reason != reason {
		return fmt.Errorf("flag reason: got %q, want %q", w.result.Reason, reason)
	}
	return nil
}

func (w *World) theURLShouldNotBeFlagged(ctx context.Context) error {
	logf("then: flagged=%v", w.result.Flagged)
	if w.result.Flagged {
		return fmt.Errorf("expected the result not to be flagged, but it was: %q", w.result.Reason)
	}
	return nil
}

func (w *World) theReputationServiceIsNeverCalled(ctx context.Context) error {
	logf("then: reputation service called=%v", w.reputation.called)
	if w.reputation.called {
		return fmt.Errorf("expected the reputation service never to be called, but it was")
	}
	return nil
}

func (w *World) theReputationCacheAlreadyHasFlagged(ctx context.Context, url, reason string) error {
	logf("given: cache[%q] = flagged=true reason=%q", url, reason)
	w.cache.store[url] = interfaces.Reputation{Flagged: true, Reason: reason}
	return nil
}

func (w *World) theReputationCacheShouldHaveBeenWrittenTo(ctx context.Context) error {
	logf("then: cache.set=%v", w.cache.set)
	if !w.cache.set {
		return fmt.Errorf("expected the reputation cache to have been written to, it was not")
	}
	return nil
}

func (w *World) theLinkHistoryShouldHaveEntries(ctx context.Context, want int) error {
	logf("then: history entries=%d want=%d", len(w.history.entries), want)
	if len(w.history.entries) != want {
		return fmt.Errorf("history entries: got %d, want %d", len(w.history.entries), want)
	}
	return nil
}

func (w *World) theLinkHistoryIsUnavailable(ctx context.Context) error {
	logf("given: link history will report an error on Record")
	w.history.recordErr = fmt.Errorf("linkstore: unavailable")
	return nil
}
