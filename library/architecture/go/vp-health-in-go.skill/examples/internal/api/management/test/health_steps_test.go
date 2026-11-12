package test

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"reflect"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/api/management"
)

func registerHealthSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^a readiness check "([^"]*)" that (passes|fails|throws|is degraded)$`, w.aReadinessCheck)
	sc.Step(`^the service version is "([^"]*)"$`, w.theServiceVersionIs)
	sc.Step(`^I GET "([^"]*)" from the management port$`, w.iGET)
	sc.Step(`^I send "([^"]*)" "([^"]*)" to the management port$`, func(method, path string) error { w.send(method, path); return nil })
	sc.Step(`^the failed readiness check "([^"]*)" should be logged$`, w.theFailedCheckShouldBeLogged)
	sc.Step(`^the management status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the management response body should be '([^']*)'$`, w.theResponseBodyShouldBe)
}

func (w *World) aReadinessCheck(ctx context.Context, name, outcome string) error {
	check := func(context.Context) error {
		switch outcome {
		case "fails":
			return errors.New(name + " is down")
		case "throws":
			panic(name + ": a bug of the check")
		case "is degraded":
			return fmt.Errorf("%s works with less: %w", name, management.ErrDegraded)
		}
		return nil
	}
	w.checks = append(w.checks, management.Check{Name: name, Check: check})
	return nil
}

func (w *World) theFailedCheckShouldBeLogged(ctx context.Context, name string) error {
	for _, line := range strings.Split(strings.TrimSpace(w.logs.String()), "\n") {
		var rec map[string]any
		if json.Unmarshal([]byte(line), &rec) == nil && rec["msg"] == "readiness check failed" && rec["check"] == name {
			return nil
		}
	}
	return fmt.Errorf("no failed readiness check %q in %s", name, w.logs.String())
}

func (w *World) theServiceVersionIs(ctx context.Context, version string) error {
	w.version = version
	return nil
}

func (w *World) iGET(ctx context.Context, path string) error {
	w.send(http.MethodGet, path)
	return nil
}

func (w *World) theResponseStatusShouldBe(ctx context.Context, want int) error {
	if w.response.Code != want {
		return fmt.Errorf("status: got %d, want %d (body %s)", w.response.Code, want, w.response.Body.String())
	}
	return nil
}

func (w *World) theResponseBodyShouldBe(ctx context.Context, want string) error {
	// Compared as JSON: the order of the keys is not part of the answer.
	var got, wanted any
	if err := json.Unmarshal(w.response.Body.Bytes(), &got); err != nil {
		return fmt.Errorf("body %s is not JSON: %w", w.response.Body.String(), err)
	}
	if err := json.Unmarshal([]byte(want), &wanted); err != nil {
		return err
	}
	if !reflect.DeepEqual(got, wanted) {
		return fmt.Errorf("body: got %s, want %s", strings.TrimSpace(w.response.Body.String()), want)
	}
	return nil
}
