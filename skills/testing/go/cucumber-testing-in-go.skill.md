---
version: 20261010120000
name: cucumber-testing-in-go
description: Go/godog-specific rules for Cucumber testing — the single TestFeatures runner, stdout step logging, step-file layout, and VSCode glue configuration
whenToUse: when writing or reviewing godog scenarios or step definitions in a Go project
updated: 20261010
tags:
  - stack/go
  - concern/testing/bdd
  - concern/testing
  - cucumber
  - godog

---

# Goal
- A single `TestFeatures` function per test package that runs godog against that package's `.feature` files; any other `func TestXxx` beside it carries the reason it is not a scenario.
- Every step-definition function logging via `fmt.Printf`-based output, never `godog.T(ctx).Logf`.
- `Options.Tags="~@status/todo && ~@status/broken"`, `Strict=true`, and `TestingT=t` set on every godog runner.

# Scope
This skill adds Go/godog-specific mechanics on top of [cucumber-testing](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md) — apply both together; this skill only covers what godog and Go add.

# Core Principle
- **The runner is the one test function godog needs** - `TestFeatures` executes the suite; every other case is a scenario, and a plain `func TestXxx` exists only under the exception of [One scenario, one runner](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#one-scenario-one-runner).
- **stdout keeps logs attached to their step** - godog's pretty output interleaves whatever a step writes to stdout with that step's line; going through Go's `testing.T` instead detaches the log from its step.

# Rule

## MUST

### Log through stdout, not testing.T
Write step logs with a shared `logf` helper that calls `fmt.Printf` directly, never `godog.T(ctx).Logf`.
- Violation: using `godog.T(ctx).Logf(...)` for a step's log line.
- Risk: `godog.T(ctx).Logf` attributes the line to `testingt.go:NN` inside godog's internals instead of the calling step, so VSCode's test explorer and the terminal both lose the line's real location; the reader can no longer tell which step produced it.
- Fix: log via `fmt.Printf`-based `logf(format, args...)`, called directly from the step function, matching [Steps log action and observation](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#steps-log-action-and-observation).

### Run with -v to see logs on green scenarios
Keep `"go.testFlags": ["-v"]` in `.vscode/settings.json`, and use `go test ... -v` from the terminal.
- Risk: without `-v`, `go test` shows a passing test's stdout only when it fails, hiding step logs on the common case and defeating their purpose.
- Fix: set `go.testFlags: ["-v"]` in the repository's `.vscode/settings.json`, and pass `-v` when running from the terminal (e.g. `go test ./client/eaxmi/test/ -v -run 'TestFeatures/<name>'` to target one scenario).

### One TestFeatures runner per test package
Wire exactly one `func TestFeatures(t *testing.T)` per test package, configured with `godog.Options{Format: "pretty", Paths: []string{"../features"}, Tags: "~@status/todo && ~@status/broken", Strict: true, TestingT: t}`. Another `func TestXxx` in that package is allowed only for a case whose scenario would be unjustifiably complex, with the reason in a comment above it, per [One scenario, one runner](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#one-scenario-one-runner). `Format` starts from `"pretty"` and gains a `cucumber:` output per [Emit classic Cucumber JSON](#emit-classic-cucumber-json).
- Violation: omitting `Format` from `godog.Options`.
- Risk: godog has no default formatter — an omitted `Format` fails every run with `unregistered formatter name: ""` before a single step executes, regardless of whether the scenarios themselves are correct (verified against a real `godog v0.16.0` run, not assumed).
- Fix: always set `Format: "pretty"` explicitly (or another registered formatter — `cucumber`, `events`, `junit`, `progress` — if the suite specifically needs one of those).
- Risk: a plain Go test with no stated reason duplicates what a scenario should express and is missing from the living doc, and a missing `Tags: "~@status/todo && ~@status/broken"` runs scenarios meant to stay excluded per [Exclude an unrunnable scenario with a status tag and its reason](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#exclude-an-unrunnable-scenario-with-a-status-tag-and-its-reason).
- Fix: keep `TestFeatures` as the package's only godog runner and move a plain test without a reason into a scenario; set `Strict: true` so an undefined/pending step fails the build instead of passing silently.

### godog ErrSkip is not exclusion
Never rely on returning godog's `ErrSkip` from a step to exclude a scenario — `go test` counts an `ErrSkip`'d scenario as a pass.
- Violation: a not-yet-implemented step returning `godog.ErrSkip` instead of the scenario being tagged `@status/todo`.
- Risk: the scenario shows green in `go test` output while verifying nothing, exactly the fake-green outcome [Exclude an unrunnable scenario with a status tag and its reason](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#exclude-an-unrunnable-scenario-with-a-status-tag-and-its-reason) forbids.
- Fix: tag the scenario `@status/todo` and rely on `Tags: "~@status/todo && ~@status/broken"` to exclude it from the run.

### Features beside the code, steps in its test package
Put a package's `.feature` files in a `features/` folder inside that package, and the godog runner with its step files in a `test/` folder beside it — never in a `features/` tree at the repository root.
```
{package}/
  {file}.go
  features/
    {rule}.feature
  test/
    runner_test.go              TestFeatures, Paths: "../features"
    world_test.go               the scenario state
    {concept}_steps_test.go     step definitions
```
- Violation: `features/check.feature` at the repository root for code in `internal/domain/services/`.
- Risk: a reader of the code does not find its specification, and a failing scenario does not say which package it belongs to; one root runner compiles every package's steps into a single test binary.
- Fix: move the feature next to the package it specifies and give that package its own `test/` runner.

### Enforce the tag scheme through the unit kind
Apply the core tag rules to every feature, scenario and Examples block and run `make test-kind-unit` before accepting them.
- Risk: an untagged case passes godog but cannot be classified by the report.
- Fix: the unit kind normalizes the inventory and calls `kind_scenarios_check`; missing, conflicting and unknown type/category values fail with the offender location.

### Step functions take context.Context first
Give every step function `ctx context.Context` as its first parameter, even when unused, so a future step can add tracing/cancellation without changing every call site's signature style.

### Organize step files by concept and, for larger suites, split plumbing from actions
Name step files by domain concept (`connection_steps_test.go`, `query_steps_test.go`, `result_steps_test.go`, ...); once a suite has enough steps to need it, put plumbing and generic comparators in a plain `common` package (`World`, `Logf`, `FixturePath`, and comparator helpers like `ToRows`/`MatchTable`) and keep only per-operation action steps (`I create`, `I relate`, `I add ...`) in `test/*_steps_test.go`.
- Risk: without this split, a growing suite mixes plumbing, comparators, and action steps in one flat package with no clear place to add the next concept.
- Fix: for a small suite, one flat `test/` package with a `world_test.go` is enough (matches [Organize step files by domain concept](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#organize-step-files-by-domain-concept)); for a larger one, `internal/service/<svc>/test/common/` (package `common`, exported `World`) plus `test/<operation>_steps_test.go` files that take a `*common.World`.

### Generate the step index via ShowStepDefinitions
When a step index is needed, generate it with `godog.Options{ShowStepDefinitions: true}` behind an env flag (e.g. `GODOG_STEPS=1`), per [Prefer a generated step index over a manual one](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#prefer-a-generated-step-index-over-a-manual-one).

### Never quote spy call strings
Never rely on an escaped double quote (`\"`) inside a Gherkin step's text or in a spy's recorded method-call string — godog does not unescape it.
- Violation: asserting a spy recorded `Element("Model/Pkg/Goal1")` when the step text needed an escaped quote to express it.
- Risk: the scenario is unparseable or the assertion never matches because the escape is not processed.
- Fix: format spy call strings without quotes (`Element(Model/Pkg/Goal1)`).

### Emit classic Cucumber JSON
godog emits **classic Cucumber JSON**: when the `CUCUMBER_JSON_DIR` environment variable is set, add a `cucumber:` output next to `pretty`, one file per godog run — a shared file name would be overwritten. A runner with one `godog.TestSuite` names the file after its package:
```go
format := "pretty"
if dir := os.Getenv("CUCUMBER_JSON_DIR"); dir != "" {
	wd, _ := os.Getwd() // the test package's directory - unique per package
	format += ",cucumber:" + filepath.Join(dir, strings.NewReplacer("/", "_", "\\", "_", ":", "_").Replace(wd)+".json")
}
// godog.Options{Format: format, ...}
```
A runner that runs several suites in one `TestFeatures` (one per store, per transport) gives each `godog.TestSuite` its own `Name` and appends it to the file name — `...Replace(wd)+"_"+name+".json"` — building `format` inside the loop.
- Violation: `Format: "pretty"` hard-coded with no `cucumber:` output, one fixed file name shared by every package, or one name shared by several suites of the same package.
- Risk: no standard report reaches the living-doc renderer, or packages or suites overwrite each other's report — only the last run reaches the living doc.
- Fix: build `Format` as above; `make test-kind-unit` sets `CUCUMBER_JSON_DIR`.

## SHOULD

### Configure the VSCode Cucumber glue for Go
When applying [Configure the Cucumber editor extension](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md#configure-the-cucumber-editor-extension), use:
```json
{
  "cucumber.glue": ["**/*_steps_test.go", "**/*_test.go"],
  "cucumber.features": ["**/*.feature"]
}
```

### Use round-trip scenarios for codecs
For a codec or serializer, write the scenario in-memory, `WriteFile`, reopen, and assert against the reopened document, plus a check that the raw output has no dangling id references.

# Check list
- [ ] Every feature has exactly one allowed `@type/…`; every scenario or Examples block inherits exactly one allowed `@category/…`.
- [ ] Every `.feature` file sits in `{package}/features/`, its runner and steps in `{package}/test/`; the repository root has no `features/` tree.
- [ ] Exactly one `TestFeatures` per test package; any other `func TestXxx` alongside it has its reason in a comment above it.
- [ ] `godog.Options` sets `Format: "pretty"` (or another registered formatter), `Tags: "~@status/todo && ~@status/broken"`, `Strict: true`, `TestingT: t`.
- [ ] With `CUCUMBER_JSON_DIR` set, `Format` adds `cucumber:<dir>/<package-unique-name>.json`, with the suite `Name` appended when the package runs several suites.
- [ ] No step returns `godog.ErrSkip` to mean "not implemented yet" — such scenarios are tagged `@status/todo` instead.
- [ ] Every step log goes through a `fmt.Printf`-based helper, never `godog.T(ctx).Logf`.
- [ ] `.vscode/settings.json` sets `"go.testFlags": ["-v"]`.
- [ ] Every step function's first parameter is `ctx context.Context`.
- [ ] Step files are organized by domain concept; a suite big enough to need it splits plumbing/comparators (`common` package) from per-operation action steps.
- [ ] No Gherkin step text or spy call string relies on an escaped double quote.
- [ ] `cucumber.glue` in `.vscode/settings.json` matches this skill's Go glob when proposed to the user.
