---
description: Add the Go test kinds (unit, mutation) and the report builder to the Makefile behind the shared testing contract, report-template/index.html, the README badges, and the gherkin parser requirement in go.mod
element_kind: repository
change_kind: extend
tags:
  - solution/conformance-testing-in-go
  - element/repo-root
---

# Structure

## Repository Structure
```
Makefile            (extended; created when missing)
README.md           (one badge per declared badge)
go.mod              (extended)
report-template/
  index.html
tools/
  normalize_unittest/
  normalize_scenarios/
  normalize_mutation/
  test_report/
  testing/            (copied verbatim from solution-conformance-testing: testing.mk, testing.sh)
  livingdoc/          (copied verbatim from solution-conformance-testing: package.json, package-lock.json, render.mjs)
```

## Directory and class skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| tools/normalize_unittest | main.go | `go test -json` → `$TEST_KIND_DIR/result/unit-test.json` |
| tools/normalize_scenarios | main.go | `.feature` files + `go test -json` → `$TEST_KIND_DIR/result/scenarios.json` |
| tools/normalize_mutation | main.go | `gremlins` report → `$TEST_KIND_DIR/result/mutation-test.json` |
| tools/test_report | main.go | every kind's `result/*.json` and `report/{name}/` → `$TEST_REPORT_DIR` (`badges/`, `reports/`, `index.html`) |
| report-template | index.html | Static landing page, copied verbatim into `$TEST_REPORT_DIR/` |
| tools/testing | testing.mk, testing.sh | Copied verbatim from `solution-conformance-testing`; the caller-facing targets, variables, and checks |
| tools/livingdoc | package.json, package-lock.json, render.mjs | Copied verbatim from `solution-conformance-testing`; renders `$TEST_KIND_DIR/report/tests/cucumber/` → `$TEST_KIND_DIR/report/tests/livingdoc/` |

# Implementation changes

`Makefile` — added to the repository's `Makefile`, leaving its other targets as they are; created with this content when the repository has none:
```makefile
# --- solution-conformance-testing-in-go: the test kinds behind the shared contract ---
# Caller-facing targets and variables (test-kinds, test-kind-<kind>, test-report,
# test-readme-check, test-and-report; TEST_RUN_PURPOSE, DELTA_BASE, TEST_WORK_DIR,
# TEST_REPORT_DIR) come from tools/testing/testing.mk - see solution-conformance-testing.

TEST_KINDS           := unit mutation
TEST_BADGES_unit     := tests coverage
TEST_BADGES_mutation := mutation
include tools/testing/testing.mk

GREMLINS_VERSION := v0.6.0
GREMLINS := $(shell go env GOPATH)/bin/gremlins

# Packages coverage is measured/mutated against - generated code (gen/) and
# the reporting tools themselves (tools/) are excluded, neither has product
# logic of its own to test.
COVERPKG := $(shell go list ./... | grep -Ev '/(gen|tools)(/|$$)' | tr '\n' ',' | sed 's/,$$//')

# unit: every test - Cucumber scenarios (godog) and plain Go tests - in one
# `go test ./...` invocation. Coverage is always gathered; it is normalized and
# reported (result/coverage-test.json, report/coverage/) only in a `report` run.
test-kind-unit:
	@$(test-kind-begin)
	@if [ "$(TEST_RUN_PURPOSE)" = report ]; then $(call test-kind-mode,every test with coverage reported); \
	else $(call test-kind-mode,every test - coverage not reported); fi
	@K="$(abspath $(TEST_KIND_DIR))"; status=0; \
	mkdir -p "$$K/report/tests/cucumber" "$$K/report/coverage"; \
	set -o pipefail; CUCUMBER_JSON_DIR="$$K/report/tests/cucumber" go test -json -coverpkg=$(COVERPKG) -coverprofile="$$K/report/coverage/coverage.out" ./... \
		| tee "$$K/report/tests/go-test.json" \
		| go run ./tools/normalize_unittest || status=$$?; \
	go run ./tools/normalize_scenarios "$$K/report/tests/go-test.json" || status=$$?; \
	if [ "$(TEST_RUN_PURPOSE)" = report ]; then \
		go tool cover -html="$$K/report/coverage/coverage.out" -o "$$K/report/coverage/index.html"; \
		pct=$$(go tool cover -func="$$K/report/coverage/coverage.out" | tail -1 | awk '{print $$3}' | tr -d '%'); \
		echo "{\"linePct\": $$pct}" > "$$K/result/coverage-test.json"; \
	else rm -rf "$$K/report/coverage"; fi; \
	if command -v npm >/dev/null 2>&1; then \
		{ npm ci --prefix tools/livingdoc --silent && node tools/livingdoc/render.mjs "$$K/report/tests/cucumber" "$$K/report/tests/livingdoc"; } \
			|| echo "livingdoc: render failed"; \
	else echo "livingdoc: npm not found - skipping living-doc report"; fi; \
	exit $$status

# mutation: gremlins over the whole module in a `report` run; in a `check`
# run only over files changed since DELTA_BASE, and skipped without one. Exits
# with gremlins' own exit code after writing the normalized result.
test-kind-mutation:
	@$(test-kind-begin)
	@if [ "$(TEST_RUN_PURPOSE)" = check ] && [ -z "$(DELTA_BASE)" ]; then \
		$(call test-kind-skip,a check run mutates only changed files and no DELTA_BASE was given); fi; \
	K="$(abspath $(TEST_KIND_DIR))"; diff_flag=""; \
	if [ "$(TEST_RUN_PURPOSE)" = check ]; then diff_flag="--diff $(DELTA_BASE)"; \
		$(call test-kind-mode,mutating only files changed since $(DELTA_BASE)); \
	else $(call test-kind-mode,mutating the whole module); fi; \
	test -x "$(GREMLINS)" || go install github.com/go-gremlins/gremlins/cmd/gremlins@$(GREMLINS_VERSION); \
	mkdir -p "$$K/report/mutation"; \
	"$(GREMLINS)" unleash --coverpkg=$(COVERPKG) --exclude-files='gen/.*' --exclude-files='tools/.*' $$diff_flag \
		--output "$$K/report/mutation/gremlins.json" . ; \
	code=$$?; \
	go run ./tools/normalize_mutation "$$K/report/mutation/gremlins.json"; \
	exit $$code

# test-report-build reads only the kinds' result/*.json (never a tool's native
# report format) and fills $(TEST_REPORT_DIR); `make test-report` calls it.
test-report-build:
	@go run ./tools/test_report
```

`go.mod` — the parser `tools/normalize_scenarios` uses, promoted from godog's indirect requirements to direct ones (same versions godog pulls in; `go mod tidy` keeps them in sync):
```
require (
	github.com/cucumber/godog v0.16.0
	github.com/cucumber/gherkin/go/v42 v42.0.0
	github.com/cucumber/messages/go/v34 v34.2.0
)
```

`report-template/index.html`:
```html
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>linkcheck — test report</title>
</head>
<body>
  <h1>linkcheck — test report</h1>
  <ul>
    <li><a href="reports/scenarios/">Scenarios</a></li>
    <li><a href="reports/tests/">Tests</a></li>
    <li><a href="reports/tests/livingdoc/">Living documentation</a></li>
    <li><a href="reports/coverage/">Coverage</a></li>
    <li><a href="reports/mutation/">Mutation</a></li>
  </ul>
  <h2>This run</h2>
  <pre id="run"></pre>
  <script>
    fetch('run.json').then(r => r.json()).then(run => {
      document.getElementById('run').textContent = 'purpose: ' + run.purpose + '\n' +
        run.kinds.map(k => k.kind + ': ' + k.state + (k.note ? ' — ' + k.note : '')).join('\n');
    }).catch(() => {});
  </script>
</body>
</html>
```

# Rule changes

## MUST
- `test-kind-unit` must gather coverage on every run, and normalize/report it only in a `report` run — never make gathering itself conditional.
  - Risk: making coverage collection itself conditional (rather than just its reporting) means a delta-scoped mutation run has no coverage data to scope against in a `check` run.
  - Fix: always pass `-coverpkg`/`-coverprofile` to `go test`; gate only the `go tool cover`/`$TEST_KIND_DIR/result/coverage-test.json` steps on `TEST_RUN_PURPOSE`, and remove `report/coverage/` in a `check` run so no coverage report is published without its badge.
- `test-kind-unit` must run `tools/normalize_scenarios` after `go test` whether or not a test failed, and exit with the test run's own status afterwards.
  - Risk: with `set -o pipefail` a failing `go test` ends the recipe line, so `$TEST_KIND_DIR/result/scenarios.json` would be missing or stale exactly on the red run it should describe.
  - Fix: capture the pipeline's status with `|| status=$$?`, run the scenario normalizer and the coverage step in the same shell, then `exit $$status`.
- `test-kind-mutation` must `exit $$code` with `gremlins`' own exit status after `tools/normalize_mutation` has written its normalized result — never swallow it.
  - Risk: swallowing the exit code turns a real mutation-testing failure into a silently green CI step.
  - Fix: capture `gremlins`' exit code before running the normalizer, and `exit` with it at the end of the target.
- `COVERPKG` must exclude `gen/` and `tools/` from both `test-kind-unit` and `test-kind-mutation`.
  - Risk: mutating generated protobuf/gRPC code or this solution's own reporting tools produces meaningless surviving-mutant noise with no product logic behind it.
  - Fix: keep the `grep -Ev '/(gen|tools)(/|$$)'` filter on `COVERPKG` and pass it to both `go test -coverpkg` and `gremlins --coverpkg`.
- `test-kind-unit` must run `go test` with `CUCUMBER_JSON_DIR=$TEST_KIND_DIR/report/tests/cucumber` (the runner adds its `cucumber:` output per `cucumber-testing-in-go`), then render `$TEST_KIND_DIR/report/tests/livingdoc/` with the base's `tools/livingdoc/`, exactly as the recipe above does.
  - Risk: without it the Go project has no living-doc view, or renders one with a Go-specific tool.
  - Fix: copy the recipe lines and `tools/livingdoc/` unchanged.

# Check list
- [ ] `$TEST_KIND_DIR/report/tests/cucumber/*.json` exists after `make test-kind-unit`; `$TEST_KIND_DIR/report/tests/livingdoc/index.html` exists when `npm` is available, and `test-kind-unit`'s exit code is unaffected by that step.
- [ ] `make test-kind-unit` produces `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/result/scenarios.json` on every run, green or red, and exits non-zero when a test failed.
- [ ] `make test-kind-unit` (a `report` run) additionally produces `$TEST_KIND_DIR/result/coverage-test.json` and `$TEST_KIND_DIR/report/coverage/index.html`.
- [ ] `make test-kind-mutation` installs `gremlins` on first use and exits non-zero when a mutant survives.
- [ ] `make test-kind-mutation TEST_RUN_PURPOSE=check` skips itself without `DELTA_BASE` and passes `--diff` with one; `make test-kinds` prints `unit tests coverage` and `mutation mutation`.
- [ ] `make test-report` fills `$TEST_REPORT_DIR` with `index.html`, `reports/{tests,coverage,mutation,scenarios}/`, `badges/{tests,coverage,mutation}.json`, and `run.json`; `make test-readme-check` passes.
