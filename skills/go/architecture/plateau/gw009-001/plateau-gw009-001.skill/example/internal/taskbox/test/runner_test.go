package test

import (
	"context"
	"os"
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
		suite := godog.TestSuite{
			Name: "taskbox-" + sut.name(),
			ScenarioInitializer: func(sc *godog.ScenarioContext) {
				registerWorldHooks(sc, w)
				registerSetupSteps(sc, w)
				registerRunSteps(sc, w)
				registerAssertSteps(sc, w)
			},
			Options: &godog.Options{
				Format:   "pretty",
				Paths:    []string{"../features"},
				Tags:     "~@todo && ~@store-" + sut.excludedKind(),
				Strict:   true,
				TestingT: t,
			},
		}
		if suite.Run() != 0 {
			t.Fatalf("taskbox conformance failed on %s", sut.name())
		}
	}
}
