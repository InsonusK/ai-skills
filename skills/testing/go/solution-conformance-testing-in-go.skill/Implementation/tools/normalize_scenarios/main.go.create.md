---
description: Builds $TEST_KIND_DIR/result/scenarios.json from every .feature file plus the go test -json event stream
project_name: tools/normalize_scenarios
name: normalize_scenarios
element_kind: functions
change_kind: create
tags:
  - solution/conformance-testing-in-go
  - element/tools-normalize-scenarios-main-go
---

# Goals
- Write the normalized `$TEST_KIND_DIR/result/scenarios.json` [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|the parent solution's Scenario report]] defines: one entry per `Scenario`, or per `Examples:` block of a `Scenario Outline`, with its type tag, status, location, and `# todo:` reason.

# Core Principles
- The inventory comes from parsing every `.feature` file with `github.com/cucumber/gherkin/go/v42` — the parser godog itself uses — so `@status/todo` entries godog never runs are listed too.
- The status comes from the `go test -json` stream `test-kind-unit` already tees to `$TEST_KIND_DIR/report/tests/go-test.json`: godog runs every pickle as the subtest `TestFeatures/{pickle name}`, and `go test` rewrites spaces to `_` and suffixes repeated names with `#NN`. The tool compiles the same pickles (`gherkin.Pickles`) and looks each one up by that rewritten name.
- A scenario name is looked up across all packages; two scenarios with the same name in different packages share their status — keep scenario names unique within the module, and give a `Scenario Outline` a name with `<placeholders>` when its rows must be told apart.

# Implementation changes
Copy verbatim to `tools/normalize_scenarios/main.go`: [`assets/tools/normalize_scenarios/main.go`](../../../assets/tools/normalize_scenarios/main.go)

# Rule changes

## MUST
- Parse `.feature` files with `github.com/cucumber/gherkin/go/v42` and compile pickles with `gherkin.Pickles` — never with a hand-written line scanner.
  - Risk: a hand-written scanner diverges from godog's own pickle names for `Scenario Outline` rows, `Rule` blocks, and tag inheritance, so statuses silently fail to join and show up as `not-run`.
  - Fix: use the same parser module godog depends on; `go.mod` lists `github.com/cucumber/gherkin/go/v42` and `github.com/cucumber/messages/go/v34` as direct requirements at the versions godog pulls in.
- Report a pickle with no `pass`/`fail` event — never run, or `skip`ped — as `not-run`, never as `passed`.
  - Risk: a `.feature` file outside every runner's `Paths`, or a skipped scenario, would read as verified in the report.
  - Fix: `statusOf` sets `not-run` unless at least one `pass` exists and no `fail` does.
- Never exit non-zero because a scenario failed — only on a tool error (unreadable event stream, unparsable `.feature`).
  - Risk: competing with `go test`'s own exit code, which `make test-kind-unit` already propagates.
  - Fix: write the file and return; `test-kind-unit` exits with the test run's status.

# Check list
- [ ] `$TEST_KIND_DIR/result/scenarios.json` lists every `.feature` entry, `@status/todo` ones included, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|the parent solution's Scenario report]].
- [ ] An `Examples:` block tagged `@category/negative` under a `Scenario Outline` is its own entry with `"category": "negative"`.
- [ ] A feature tagged `@type/service` on its `Feature:` line gives every entry of it `"type": "service"`; without exactly one `@type/…` tag there, `"none"`.
- [ ] Every entry lists its own and inherited tags in `"tags"`, with `@`, sorted — a feature tagged `@type/service` gives every entry of it that tag.
- [ ] A `.feature` file no runner picks up produces `"status": "not-run"` entries.
- [ ] A scenario tagged `@status/broken` with `# broken: …` above its tags is `"status": "broken"` with that note; one tagged `@status/validated` has `"validated": true`.

# Unittest TestCases
- [ ] WHEN a scenario is tagged `@status/todo @category/error` with `# todo: needs a fake resolver` above its tags THEN its entry is `"category": "error"`, `"status": "todo"`, `"note": "needs a fake resolver"`
- [ ] WHEN a scenario carries no `@category/…` tag, two different ones, or an unknown value THEN its `category` is `none`
- [ ] WHEN one row of a tagged `Examples:` block fails THEN that block's entry is `failed`, and a sibling block whose rows all pass stays `passed`
- [ ] WHEN a scenario name contains spaces THEN it is matched to the `TestFeatures/{name_with_underscores}` subtest
