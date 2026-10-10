package test

import (
	"context"
	"fmt"
	"github.com/cucumber/godog"
	"github.com/example/linkcheck/internal/linkcheck"
	"os"
	"strings"
)

func registerPackageSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the package version equals the installed distribution's version$`, func(ctx context.Context) error {
		data, err := os.ReadFile("../../../VERSION")
		if err != nil {
			return err
		}
		want := strings.TrimSpace(string(data))
		logf("package version=%s distribution=%s", linkcheck.Version, want)
		if linkcheck.Version != want {
			return fmt.Errorf("version got %q want %q", linkcheck.Version, want)
		}
		return nil
	})

}
