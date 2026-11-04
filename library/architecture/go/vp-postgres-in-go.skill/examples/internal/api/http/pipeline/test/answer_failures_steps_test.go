package test

import (
	"fmt"

	"github.com/cucumber/godog"
)

func registerAnswerFailuresSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the command panics$`, func() error { w.echoPanics = true; return nil })
	sc.Step(`^a "([^"]*)" record should be logged$`, func(msg string) error {
		if len(w.records(msg)) == 0 {
			return fmt.Errorf("no %q record in %s", msg, w.logs.String())
		}
		return nil
	})
}
