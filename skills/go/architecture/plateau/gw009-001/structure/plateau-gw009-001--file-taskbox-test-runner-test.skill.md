---
name: plateau-gw009-001--file-taskbox-test-runner-test
description: internal/taskbox/test/runner_test.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when changing or debugging the TaskBox conformance run on this plateau
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/taskbox/test/runner_test.go
---

# Goal
The conformance runner's the godog runner: one suite per store; a missing TEST_DATABASE_DSN fails the run.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/runner_test.go.create.md|runner_test.go]]

# Core Principles
- Apply ONE plateau template per file.
- Verbatim from the solution's Implementation file (`{store}` = `linkstore`).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/runner_test.go.create.md|runner_test.go]]

# Implementation
```go
// Skill: file-taskbox-test-runner-test
// Plateau: plateau-gw009-001
// Version: 20260928120000

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

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/runner_test.go.create.md|runner_test.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Keep this file identical to the solution's Implementation file.
- Never skip when the database is missing.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/runner_test.go.create.md|runner_test.go]]

# Check list
- [ ] `TEST_DATABASE_DSN=… make unit-test` runs the feature on PostgreSQL.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/taskbox/test/runner_test.go.create.md|runner_test.go]]
