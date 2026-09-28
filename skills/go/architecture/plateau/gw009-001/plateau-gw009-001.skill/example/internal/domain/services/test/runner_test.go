package test

import (
	"testing"

	"github.com/cucumber/godog"
)

// TestFeatures is the single godog runner for this package - see
// cucmber-testing-in-go's "One TestFeatures runner per test package" rule.
// No other plain func TestXxx exists in this package.
func TestFeatures(t *testing.T) {
	w := newWorld()

	suite := godog.TestSuite{
		ScenarioInitializer: func(sc *godog.ScenarioContext) {
			registerWorldHooks(sc, w)
			registerCheckSteps(sc, w)
		},
		Options: &godog.Options{
			Format:   "pretty",
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
