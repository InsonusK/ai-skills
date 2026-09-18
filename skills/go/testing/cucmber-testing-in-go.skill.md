---
name: cucmber-testing-in-go
description: Go/godog-specific rules for Cucumber testing — the single TestFeatures runner, stdout step logging, step-file layout, and VSCode glue configuration
whenToUse: when writing or reviewing godog scenarios or step definitions in a Go project
updated: 20260917
tags:
  - stack/go
  - concern/testing/bdd
  - concern/testing
  - cucumber
  - godog

---

# Goal
- A single `TestFeatures` function per test package that runs godog against that package's `.feature` files, with no other plain `func TestXxx` test in the same package.
- Every step-definition function logging via `fmt.Printf`-based output, never `godog.T(ctx).Logf`.
- `Options.Tags="~@todo"`, `Strict=true`, and `TestingT=t` set on every godog runner.

# Scope
This skill adds Go/godog-specific mechanics on top of [cucmber-testing](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md) — apply both together; this skill only covers what godog and Go add.

# Core Principle
- **The runner is the one exception** - `TestFeatures` is the single `func TestXxx` godog needs to execute the suite; every other case is a scenario, per [One scenario, one runner](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#one-scenario-one-runner).
- **stdout keeps logs attached to their step** - godog's pretty output interleaves whatever a step writes to stdout with that step's line; going through Go's `testing.T` instead detaches the log from its step.

# Rule

## MUST

### Log through stdout, not testing.T
Write step logs with a shared `logf` helper that calls `fmt.Printf` directly, never `godog.T(ctx).Logf`.
- Violation: using `godog.T(ctx).Logf(...)` for a step's log line.
- Risk: `godog.T(ctx).Logf` attributes the line to `testingt.go:NN` inside godog's internals instead of the calling step, so VSCode's test explorer and the terminal both lose the line's real location; the reader can no longer tell which step produced it.
- Fix: log via `fmt.Printf`-based `logf(format, args...)`, called directly from the step function, matching [Steps log action and observation](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#steps-log-action-and-observation).

### Run with -v to see logs on green scenarios
Keep `"go.testFlags": ["-v"]` in `.vscode/settings.json`, and use `go test ... -v` from the terminal.
- Risk: without `-v`, `go test` shows a passing test's stdout only when it fails, hiding step logs on the common case and defeating their purpose.
- Fix: set `go.testFlags: ["-v"]` in the repository's `.vscode/settings.json`, and pass `-v` when running from the terminal (e.g. `go test ./client/eaxmi/test/ -v -run 'TestFeatures/<name>'` to target one scenario).

### One TestFeatures runner per test package
Wire exactly one `func TestFeatures(t *testing.T)` per test package, configured with `godog.Options{Format: "pretty", Paths: []string{"../features"}, Tags: "~@todo", Strict: true, TestingT: t}`, and no other `func TestXxx` in that package.
- Violation: omitting `Format` from `godog.Options`.
- Risk: godog has no default formatter — an omitted `Format` fails every run with `unregistered formatter name: ""` before a single step executes, regardless of whether the scenarios themselves are correct (verified against a real `godog v0.16.0` run, not assumed).
- Fix: always set `Format: "pretty"` explicitly (or another registered formatter — `cucumber`, `events`, `junit`, `progress` — if the suite specifically needs one of those).
- Risk: a second plain Go test in the same package duplicates what a scenario should express, and a missing `Tags: "~@todo"` runs scenarios meant to stay excluded per [Tag unrunnable scenarios @todo and verify exclusion](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#tag-unrunnable-scenarios-todo-and-verify-exclusion).
- Fix: keep `TestFeatures` as the package's only test function; set `Strict: true` so an undefined/pending step fails the build instead of passing silently.

### godog ErrSkip is not exclusion
Never rely on returning godog's `ErrSkip` from a step to exclude a scenario — `go test` counts an `ErrSkip`'d scenario as a pass.
- Violation: a not-yet-implemented step returning `godog.ErrSkip` instead of the scenario being tagged `@todo`.
- Risk: the scenario shows green in `go test` output while verifying nothing, exactly the fake-green outcome [Tag unrunnable scenarios @todo and verify exclusion](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#tag-unrunnable-scenarios-todo-and-verify-exclusion) forbids.
- Fix: tag the scenario `@todo` and rely on `Tags: "~@todo"` to exclude it from the run.

### Step functions take context.Context first
Give every step function `ctx context.Context` as its first parameter, even when unused, so a future step can add tracing/cancellation without changing every call site's signature style.

### Organize step files by concept and, for larger suites, split plumbing from actions
Name step files by domain concept (`connection_steps_test.go`, `query_steps_test.go`, `result_steps_test.go`, ...); once a suite has enough steps to need it, put plumbing and generic comparators in a plain `common` package (`World`, `Logf`, `FixturePath`, and comparator helpers like `ToRows`/`MatchTable`) and keep only per-operation action steps (`I create`, `I relate`, `I add ...`) in `test/*_steps_test.go`.
- Risk: without this split, a growing suite mixes plumbing, comparators, and action steps in one flat package with no clear place to add the next concept.
- Fix: for a small suite, one flat `test/` package with a `world_test.go` is enough (matches [Organize step files by domain concept](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#organize-step-files-by-domain-concept)); for a larger one, `internal/service/<svc>/test/common/` (package `common`, exported `World`) plus `test/<operation>_steps_test.go` files that take a `*common.World`.

### Generate the step index via ShowStepDefinitions
When a step index is needed, generate it with `godog.Options{ShowStepDefinitions: true}` behind an env flag (e.g. `GODOG_STEPS=1`), per [Prefer a generated step index over a manual one](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#prefer-a-generated-step-index-over-a-manual-one).

### Never quote spy call strings
Never rely on an escaped double quote (`\"`) inside a Gherkin step's text or in a spy's recorded method-call string — godog does not unescape it.
- Violation: asserting a spy recorded `Element("Model/Pkg/Goal1")` when the step text needed an escaped quote to express it.
- Risk: the scenario is unparseable or the assertion never matches because the escape is not processed.
- Fix: format spy call strings without quotes (`Element(Model/Pkg/Goal1)`).

## SHOULD

### Configure the VSCode Cucumber glue for Go
When applying [Configure the Cucumber editor extension](../../common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md#configure-the-cucumber-editor-extension), use:
```json
{
  "cucumber.glue": ["**/*_steps_test.go", "**/*_test.go"],
  "cucumber.features": ["**/*.feature"]
}
```

### Use round-trip scenarios for codecs
For a codec or serializer, write the scenario in-memory, `WriteFile`, reopen, and assert against the reopened document, plus a check that the raw output has no dangling id references.

# Check list
- [ ] Exactly one `TestFeatures` per test package; no other `func TestXxx` alongside it.
- [ ] `godog.Options` sets `Format: "pretty"` (or another registered formatter), `Tags: "~@todo"`, `Strict: true`, `TestingT: t`.
- [ ] No step returns `godog.ErrSkip` to mean "not implemented yet" — such scenarios are tagged `@todo` instead.
- [ ] Every step log goes through a `fmt.Printf`-based helper, never `godog.T(ctx).Logf`.
- [ ] `.vscode/settings.json` sets `"go.testFlags": ["-v"]`.
- [ ] Every step function's first parameter is `ctx context.Context`.
- [ ] Step files are organized by domain concept; a suite big enough to need it splits plumbing/comparators (`common` package) from per-operation action steps.
- [ ] No Gherkin step text or spy call string relies on an escaped double quote.
- [ ] `cucumber.glue` in `.vscode/settings.json` matches this skill's Go glob when proposed to the user.
