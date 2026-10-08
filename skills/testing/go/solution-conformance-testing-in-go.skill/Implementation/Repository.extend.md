---
description: Add the Go test kinds (unit, mutation) as kind scripts behind the shared testing contract, the normalizer tools they call, report-template/index.html, the README badges, and the gherkin parser requirement in go.mod
element_kind: repository
change_kind: extend
tags:
  - solution/conformance-testing-in-go
  - element/repo-root
---

# Structure

## Repository Structure
```
Makefile            (one line added: include tools/testing/testing.mk; created when missing)
README.md           (one badge per declared badge)
go.mod              (extended)
report-template/
  index.html
tools/
  normalize_unittest/
  normalize_scenarios/
  normalize_mutation/
  testing/            (copied verbatim from solution-conformance-testing)
    kinds/
      unit.sh         (this stack's unit kind)
      mutation.sh     (this stack's mutation kind)
  livingdoc/          (copied verbatim from solution-conformance-testing: package.json, package-lock.json, render.mjs)
```

## Directory and class skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| tools/normalize_unittest | main.go | `go test -json` → `$TEST_KIND_DIR/result/unit-test.json` |
| tools/normalize_scenarios | main.go | `.feature` files + `go test -json` → `$TEST_KIND_DIR/result/scenarios.json` |
| tools/normalize_mutation | main.go | `gremlins` report → `$TEST_KIND_DIR/result/mutation-test.json` |
| report-template | index.html | Static landing page, copied verbatim into `$TEST_REPORT_DIR/` |
| tools/testing | (base) | Copied verbatim from `solution-conformance-testing`: the Makefile side, the runner, the report builder |
| tools/testing/kinds | unit.sh, mutation.sh | This stack's two test kinds — run `go test` / `gremlins`, write the normalized results |
| tools/livingdoc | package.json, package-lock.json, render.mjs | Copied verbatim from `solution-conformance-testing`; renders `$TEST_KIND_DIR/report/tests/cucumber/` → `$TEST_KIND_DIR/report/tests/livingdoc/` |

# Implementation changes

Kind scripts — the only stack-specific code; the Makefile, the runner and the report builder are the base's `tools/testing/`:
- Copy verbatim to `tools/testing/kinds/unit.sh`: [`assets/tools/testing/kinds/unit.sh`](../assets/tools/testing/kinds/unit.sh)
- Copy verbatim to `tools/testing/kinds/mutation.sh`: [`assets/tools/testing/kinds/mutation.sh`](../assets/tools/testing/kinds/mutation.sh)

`Makefile` — add `include tools/testing/testing.mk` after the repository's own targets; create the file with that line when there is none.

`go.mod` — the parser `tools/normalize_scenarios` uses, promoted from godog's indirect requirements to direct ones (same versions godog pulls in; `go mod tidy` keeps them in sync):
```
require (
	github.com/cucumber/godog v0.16.0
	github.com/cucumber/gherkin/go/v42 v42.0.0
	github.com/cucumber/messages/go/v34 v34.2.0
)
```

`report-template/index.html` — fill and copy the base's template, `{project-name}` = the service name: [`templates/report-template/index.html`](skills/testing/core/solution-conformance-testing.skill/templates/report-template/index.html)

# Rule changes

## MUST
- `test-kind-unit` must gather coverage on every run, and normalize/report it only in a `report` run — never make gathering itself conditional.
  - Risk: making coverage collection itself conditional (rather than just its reporting) means a delta-scoped mutation run has no coverage data to scope against in a `check` run.
  - Fix: always pass `-coverpkg`/`-coverprofile` to `go test`; gate only the `go tool cover`/`$TEST_KIND_DIR/result/coverage-test.json` steps on `TEST_RUN_PURPOSE`, and remove `report/coverage/` in a `check` run so no coverage report is published without its badge.
- `test-kind-unit` must run `tools/normalize_scenarios` after `go test` whether or not a test failed, and exit with the test run's own status afterwards.
  - Risk: with `set -o pipefail` a failing `go test` ends the pipeline, so `$TEST_KIND_DIR/result/scenarios.json` would be missing or stale exactly on the red run it should describe.
  - Fix: capture the pipeline's status with `|| status=$$?`, run the scenario normalizer and the coverage step in the same shell, then `exit $$status`.
- `test-kind-mutation` must `exit $$code` with `gremlins`' own exit status after `tools/normalize_mutation` has written its normalized result — never swallow it.
  - Risk: swallowing the exit code turns a real mutation-testing failure into a silently green CI step.
  - Fix: capture `gremlins`' exit code before running the normalizer, and `exit` with it at the end of the target.
- `test-kind-mutation` must pass `--threshold-efficacy 0 --threshold-mcover 0` to `gremlins` in a `report` run, and leave the project's own thresholds in force in a `check` run.
  - Violation: a `.gremlins.yaml` with `unleash.threshold.efficacy: 80` and a `report` run that passes no threshold flag — `gremlins` exits `10` ("below efficacy-threshold").
  - Risk: the report workflow goes red over a score, which the parent contract forbids in a `report` run.
  - Fix: keep the two flags in `tools/testing/kinds/mutation.sh`; `0` switches a threshold off and overrides the configuration file. Without any threshold `gremlins` exits `0` however many mutants survive.
- `test-kind-mutation` must run `gremlins` with git's `diff.relative` switched on (`GIT_CONFIG_COUNT=1 GIT_CONFIG_KEY_0=diff.relative GIT_CONFIG_VALUE_0=true`).
  - Violation: `gremlins unleash --diff origin/main .` in a module at `services/api/` of a larger repository.
  - Risk: git names changed files from the repository root, `gremlins` compares them with module-relative paths — nothing matches, every mutant is `SKIPPED`, and the run is green over nothing.
  - Fix: keep the three variables on the `gremlins` line of `tools/testing/kinds/mutation.sh`; they change nothing when `go.mod` is in the repository root.
- `COVERPKG` must exclude `gen/` and `tools/` from both `test-kind-unit` and `test-kind-mutation`.
  - Risk: mutating generated protobuf/gRPC code or this solution's own reporting tools produces meaningless surviving-mutant noise with no product logic behind it.
  - Fix: keep the `grep -Ev '/(gen|tools)(/|$$)'` filter on `COVERPKG` and pass it to both `go test -coverpkg` and `gremlins --coverpkg`.
- `test-kind-unit` must run `go test` with `CUCUMBER_JSON_DIR=$TEST_KIND_DIR/report/tests/cucumber` (the runner adds its `cucumber:` output per `cucumber-testing-in-go`), then render `$TEST_KIND_DIR/report/tests/livingdoc/` with the base's `tools/livingdoc/`, exactly as `tools/testing/kinds/unit.sh` does.
  - Risk: without it the Go project has no living-doc view, or renders one with a Go-specific tool.
  - Fix: copy `tools/testing/kinds/unit.sh` and `tools/livingdoc/` unchanged.

# Check list
- [ ] `$TEST_KIND_DIR/report/tests/cucumber/*.json` exists after `make test-kind-unit`; `$TEST_KIND_DIR/report/tests/livingdoc/index.html` exists when `npm` is available, and `test-kind-unit`'s exit code is unaffected by that step.
- [ ] `make test-kind-unit` produces `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/result/scenarios.json` on every run, green or red, and exits non-zero when a test failed.
- [ ] `make test-kind-unit` (a `report` run) additionally produces `$TEST_KIND_DIR/result/coverage-test.json` and `$TEST_KIND_DIR/report/coverage/index.html`.
- [ ] `make test-kind-mutation` installs `gremlins` on first use; in a `report` run it exits `0` when mutants survive — also with a threshold in `.gremlins.yaml` — and non-zero only when `gremlins` could not run (a red test fails its coverage step).
- [ ] `make test-kind-mutation TEST_RUN_PURPOSE=check` skips itself without `DELTA_BASE` and passes `--diff` with one, also in a module below the repository root; `make test-kinds` prints `unit tests coverage` and `mutation mutation`.
- [ ] `make test-report` fills `$TEST_REPORT_DIR` with `index.html`, `reports/{tests,coverage,mutation,scenarios}/`, `badges/{tests,coverage,mutation}.json`, and `run.json`; `make test-readme-check` passes.
