package test

import (
	"context"
	"os"
	"path/filepath"
	"strings"
	"testing"

	"github.com/cucumber/godog"
)

// TestFeatures runs taskbox-conformance.feature — copied verbatim from
// solution-taskbox — once per store this service's TaskBox supports. A store
// whose connection setting is missing fails the run; it is never skipped.
func TestFeatures(t *testing.T) {
	ctx := context.Background()
	dsn := os.Getenv("TEST_DATABASE_DSN")
	if dsn == "" {
		t.Fatal("TEST_DATABASE_DSN is required: the TaskBox conformance feature runs against a real PostgreSQL (e.g. postgres://postgres:postgres@localhost:5432/taskbox_test)")
	}
	pg, err := newPostgresUnderTest(ctx, dsn)
	if err != nil {
		t.Fatal(err)
	}
	defer pg.close()

	for _, sut := range []storeUnderTest{pg} {
		w := newWorld(sut)
		name := "taskbox-" + sut.name()

		// Classic Cucumber JSON for the living-doc report - see cucumber-testing-in-go's
		// "Emit classic Cucumber JSON" rule. One file per suite: this package runs one per store.
		format := "pretty"
		if dir := os.Getenv("CUCUMBER_JSON_DIR"); dir != "" {
			wd, _ := os.Getwd()
			format += ",cucumber:" + filepath.Join(dir, strings.NewReplacer("/", "_", "\\", "_", ":", "_").Replace(wd)+"_"+name+".json")
		}

		suite := godog.TestSuite{
			Name: name,
			ScenarioInitializer: func(sc *godog.ScenarioContext) {
				registerWorldHooks(sc, w)
				registerSetupSteps(sc, w)
				registerRunSteps(sc, w)
				registerAssertSteps(sc, w)
			},
			Options: &godog.Options{
				Format:   format,
				Paths:    []string{"../features"},
				Tags:     "~@status/todo && ~@status/broken && ~@store-" + sut.excludedKind(),
				Strict:   true,
				TestingT: t,
			},
		}
		if suite.Run() != 0 {
			t.Fatalf("taskbox conformance failed on %s", sut.name())
		}
	}
}
