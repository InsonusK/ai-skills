package test

import (
	"fmt"
	"os"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/httpclient"
)

func registerConfigSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, func(name, value string) error {
		return os.Setenv(name, value)
	})
	sc.Step(`^the settings of "([^"]*)" are loaded$`, func(prefix string) error {
		w.loaded, w.loadErr = httpclient.LoadConfig(prefix)
		logf("when: LoadConfig(%s) -> %+v, error %v", prefix, w.loaded, w.loadErr)
		return nil
	})
	sc.Step(`^the base URL should be "([^"]*)" and the timeout "([^"]*)"$`, func(base, timeout string) error {
		if w.loadErr != nil {
			return w.loadErr
		}
		if w.loaded.BaseURL() != base || w.loaded.Timeout.String() != timeout {
			return fmt.Errorf("got %s and %s, want %s and %s", w.loaded.BaseURL(), w.loaded.Timeout, base, timeout)
		}
		return nil
	})
	sc.Step(`^loading should fail mentioning "([^"]*)"$`, func(text string) error {
		if w.loadErr == nil || !strings.Contains(w.loadErr.Error(), text) {
			return fmt.Errorf("error %v, want one mentioning %q", w.loadErr, text)
		}
		return nil
	})
}
