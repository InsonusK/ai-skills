package test

import (
	"encoding/json"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/pipeline"
)

var kinds = map[string]error{"invalid": pipeline.ErrInvalid, "conflict": pipeline.ErrConflict}

func registerCLISteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the command answers "([^"]*)"$`, func(text string) error { w.echoAnswer = text; return nil })
	sc.Step(`^the command is rejected with the kind "([^"]*)" and "([^"]*)"$`, func(kind, message string) error {
		w.echoErr = fmt.Errorf("%w: %s", kinds[kind], message)
		return nil
	})
	sc.Step(`^the command fails with "([^"]*)"$`, func(message string) error { w.echoErr = errors.New(message); return nil })
	sc.Step(`^I run "([^"]*)"$`, w.run)
	sc.Step(`^stdout should be the line '([^']*)'$`, func(want string) error {
		if got := w.stdout.String(); got != want+"\n" {
			return fmt.Errorf("stdout: got %q, want %q", got, want+"\n")
		}
		return nil
	})
	sc.Step(`^stdout should be indented JSON with "([^"]*)" = "([^"]*)"$`, func(key, value string) error {
		var doc map[string]string
		if err := json.Unmarshal(w.stdout.Bytes(), &doc); err != nil || doc[key] != value || !strings.Contains(w.stdout.String(), "\n  ") {
			return fmt.Errorf("stdout %q (err %v), want indented JSON with %s=%q", w.stdout.String(), err, key, value)
		}
		return nil
	})
	sc.Step(`^stdout should be empty$`, func() error {
		if w.stdout.Len() > 0 {
			return fmt.Errorf("stdout: got %q, want nothing", w.stdout.String())
		}
		return nil
	})
	sc.Step(`^stdout should mention "([^"]*)"$`, func(text string) error {
		if !strings.Contains(w.stdout.String(), text) {
			return fmt.Errorf("stdout %q does not mention %q", w.stdout.String(), text)
		}
		return nil
	})
	sc.Step(`^stderr should mention "([^"]*)"$`, func(text string) error {
		if !strings.Contains(w.stderr.String(), text) {
			return fmt.Errorf("stderr %q does not mention %q", w.stderr.String(), text)
		}
		return nil
	})
	sc.Step(`^the command should have received "([^"]*)"$`, func(text string) error {
		if !w.echoCalled || w.echoGot.Text != text {
			return fmt.Errorf("handler received %q (called=%v), want %q", w.echoGot.Text, w.echoCalled, text)
		}
		return nil
	})
	sc.Step(`^the command should not have been called$`, func() error {
		if w.echoCalled {
			return fmt.Errorf("handler was called with %+v", w.echoGot)
		}
		return nil
	})
	sc.Step(`^running should end without an error$`, func() error {
		if w.err != nil {
			return fmt.Errorf("run returned %v", w.err)
		}
		return nil
	})
	sc.Step(`^running should fail with the kind "([^"]*)"$`, func(kind string) error {
		if !errors.Is(w.err, kinds[kind]) {
			return fmt.Errorf("run returned %v, want the kind %q", w.err, kind)
		}
		return nil
	})
	sc.Step(`^running should fail with no kind$`, func() error {
		if w.err == nil || pipeline.KindOf(w.err) != nil {
			return fmt.Errorf("run returned %v, want an error with no kind", w.err)
		}
		return nil
	})
}
