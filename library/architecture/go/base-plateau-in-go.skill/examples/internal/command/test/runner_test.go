package test

import (
	"os"
	"path/filepath"
	"strings"
	"testing"

	"github.com/cucumber/godog"
)

// TestFeatures is the single godog runner for this package.
func TestFeatures(t *testing.T) {
	w := &World{}

	format := "pretty"
	if dir := os.Getenv("CUCUMBER_JSON_DIR"); dir != "" {
		wd, _ := os.Getwd()
		format += ",cucumber:" + filepath.Join(dir, strings.NewReplacer("/", "_", "\\", "_", ":", "_").Replace(wd)+".json")
	}

	suite := godog.TestSuite{
		ScenarioInitializer: func(sc *godog.ScenarioContext) {
			registerWorldHooks(sc, w)
			registerPlanItemsSteps(sc, w)
			registerLoggingSteps(sc, w)
		},
		Options: &godog.Options{
			Format:   format,
			Paths:    []string{"../features"},
			Tags:     "~@status/todo && ~@status/broken",
			Strict:   true,
			TestingT: t,
		},
	}

	if suite.Run() != 0 {
		t.Fatal("non-zero status returned, failed to run feature tests")
	}
}
