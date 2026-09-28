---
description: godog runner: taskbox-conformance.feature once per supported store, failing when a store is unreachable
project_name: internal/taskbox/test
name: runner_test
element_kind: functions
change_kind: create
verbatim_of: internal/taskbox/test/runner_test.go
tags:
  - solution/taskbox-in-go
  - element/internal-taskbox-test-runner-test-go
---

# Goals
- Run the shared conformance feature against every store this service's TaskBox supports.

# Core Principles
- A missing `TEST_DATABASE_DSN` fails the run; it never skips (solution-taskbox's feature rules).

# Implementation changes
Create `internal/taskbox/test/runner_test.go` exactly as below (`{module-path}` = the service's Go module path, `{store}` = the package owning the service's migrations). The code is proven by the conformance feature on PostgreSQL in this catalog's plateau built with this solution.

```go
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
```

# Rule changes

## MUST

### Fail when the store is unreachable
Fail the test run when `TEST_DATABASE_DSN` is empty; never skip the conformance feature.
- Risk: a skipped feature reports green without having tested the store.
- Fix: `t.Fatal` with the setting's name and an example value.
