package test

import (
	"context"
	"fmt"
	"os"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/config"
)

func registerConfigSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, w.theEnvironmentVariableIs)
	sc.Step(`^the configuration is loaded$`, w.theConfigurationIsLoaded)
	sc.Step(`^the log level should be "([^"]*)"$`, w.theLogLevelShouldBe)
	sc.Step(`^the startup titles should be "([^"]*)"$`, w.theStartupTitlesShouldBe)
	sc.Step(`^there should be no startup titles$`, w.thereShouldBeNoStartupTitles)
	sc.Step(`^loading should fail mentioning "([^"]*)"$`, w.loadingShouldFailMentioning)
}

func (w *World) theEnvironmentVariableIs(ctx context.Context, name, value string) error {
	return os.Setenv(name, value)
}

func (w *World) theConfigurationIsLoaded(ctx context.Context) error {
	w.cfg, w.err = config.Load()
	logf("when: Load() -> %+v err=%v", w.cfg, w.err)
	return nil
}

func (w *World) theLogLevelShouldBe(ctx context.Context, want string) error {
	if w.err != nil {
		return fmt.Errorf("load failed: %v", w.err)
	}
	if w.cfg.LogLevel.String() != want {
		return fmt.Errorf("log level: got %s, want %s", w.cfg.LogLevel, want)
	}
	return nil
}

func (w *World) theStartupTitlesShouldBe(ctx context.Context, want string) error {
	if got := strings.Join(w.cfg.StartupTitles, "|"); got != want {
		return fmt.Errorf("startup titles: got %q, want %q", got, want)
	}
	return nil
}

func (w *World) thereShouldBeNoStartupTitles(ctx context.Context) error {
	if len(w.cfg.StartupTitles) != 0 {
		return fmt.Errorf("startup titles: got %q, want none", w.cfg.StartupTitles)
	}
	return nil
}

func (w *World) loadingShouldFailMentioning(ctx context.Context, text string) error {
	if w.err == nil || !strings.Contains(w.err.Error(), text) {
		return fmt.Errorf("error: got %v, want one mentioning %q", w.err, text)
	}
	return nil
}
