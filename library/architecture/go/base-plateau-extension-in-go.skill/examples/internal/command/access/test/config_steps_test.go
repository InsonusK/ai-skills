package test

import (
	"fmt"
	"os"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command/access"
)

func registerConfigSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, os.Setenv)
	sc.Step(`^the access settings are loaded$`, func() error {
		w.cfg, w.cfgError = access.LoadConfig()
		logf("when: LoadConfig -> %+v err=%v", w.cfg, w.cfgError)
		return nil
	})
	sc.Step(`^the token "([^"]*)" should name "([^"]*)" with the roles "([^"]*)"$`, w.theTokenShouldName)
	sc.Step(`^the batch limit should be (\d+)$`, func(want int) error {
		if w.cfg.MaxBatch != want {
			return fmt.Errorf("batch limit: got %d, want %d", w.cfg.MaxBatch, want)
		}
		return nil
	})
	sc.Step(`^loading should fail mentioning "([^"]*)"$`, func(text string) error {
		if w.cfgError == nil || !strings.Contains(w.cfgError.Error(), text) {
			return fmt.Errorf("error: got %v, want one mentioning %q", w.cfgError, text)
		}
		return nil
	})
}

func (w *World) theTokenShouldName(token, user, roles string) error {
	identity, ok := w.cfg.Tokens[token]
	if !ok {
		return fmt.Errorf("no caller for %q in %+v", token, w.cfg.Tokens)
	}
	if identity.User != user || strings.Join(identity.Roles, ",") != roles {
		return fmt.Errorf("%q names %+v, want %s with %q", token, identity, user, roles)
	}
	return nil
}
