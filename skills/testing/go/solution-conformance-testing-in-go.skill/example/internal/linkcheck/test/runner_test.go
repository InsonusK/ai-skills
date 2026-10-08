package test

import (
	"os"
	"path/filepath"
	"strings"
	"testing"

	"github.com/cucumber/godog"
)

// TestFeatures is the single godog runner for this package - see
// cucumber-testing-in-go's "One TestFeatures runner per test package" rule.
// No other plain func TestXxx exists in this package.
func TestFeatures(t *testing.T) {
	w := newWorld()

	// Classic Cucumber JSON for the living-doc report - see cucumber-testing-in-go's
	// "Emit classic Cucumber JSON" rule. One file per package: godog runs per package.
	format := "pretty"
	if dir := os.Getenv("CUCUMBER_JSON_DIR"); dir != "" {
		wd, _ := os.Getwd()
		format += ",cucumber:" + filepath.Join(dir, strings.NewReplacer("/", "_", "\\", "_", ":", "_").Replace(wd)+".json")
	}

	suite := godog.TestSuite{
		ScenarioInitializer: func(sc *godog.ScenarioContext) {
			registerWorldHooks(sc, w)
			registerCheckSteps(sc, w)
		},
		Options: &godog.Options{
			Format:   format,
			Paths:    []string{"../features"},
			Tags:     "~@todo",
			Strict:   true,
			TestingT: t,
		},
	}

	if suite.Run() != 0 {
		t.Fatal("non-zero status returned, failed to run feature tests")
	}
}
