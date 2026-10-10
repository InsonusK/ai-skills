package test

import (
	"context"
	"fmt"
	"github.com/cucumber/godog"
	"github.com/example/linkcheck/internal/linkcheck"
	"github.com/example/linkcheck/internal/version"
)

func registerPackageSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the package version equals the installed distribution's version$`, func(ctx context.Context) error {
		want := version.Version
		logf("package version=%s distribution=%s", linkcheck.Version, want)
		if linkcheck.Version != want {
			return fmt.Errorf("version got %q want %q", linkcheck.Version, want)
		}
		return nil
	})

}
