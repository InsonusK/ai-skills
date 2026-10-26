package test

import (
	"fmt"
	"os"
	"strconv"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/client/core/grpcclient"
)

func registerConfigSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, func(name, value string) error {
		return os.Setenv(name, value)
	})
	sc.Step(`^the settings of "([^"]*)" are loaded$`, func(prefix string) error {
		w.loaded, w.loadErr = grpcclient.LoadConfig(prefix)
		logf("when: LoadConfig(%s) -> %+v, error %v", prefix, w.loaded, w.loadErr)
		return nil
	})
	sc.Step(`^the target should be "([^"]*)", TLS "([^"]*)", and the timeout "([^"]*)"$`, func(target, tls, timeout string) error {
		if w.loadErr != nil {
			return w.loadErr
		}
		got := fmt.Sprintf("%s %s %s", w.loaded.Target(), strconv.FormatBool(w.loaded.TLS), w.loaded.Timeout)
		if want := fmt.Sprintf("%s %s %s", target, tls, timeout); got != want {
			return fmt.Errorf("got %s, want %s", got, want)
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
