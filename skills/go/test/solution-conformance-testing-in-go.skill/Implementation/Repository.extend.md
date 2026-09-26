---
description: Add the unit-test/mutation-test/test-report/test-and-report Makefile targets, report-template/index.html, and the gherkin parser requirement in go.mod
element_kind: repository
change_kind: extend
tags:
  - solution/conformance-testing-in-go
  - element/repo-root
---

# Structure

## Repository Structure
```
Makefile            (extended)
go.mod              (extended)
report-template/
  index.html
tools/
  normalize_unittest/
  normalize_scenarios/
  normalize_mutation/
  test_report/
```

## Directory and class skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| tools/normalize_unittest | main.go | `go test -json` → `tmp/result/unit-test.json` |
| tools/normalize_scenarios | main.go | `.feature` files + `go test -json` → `tmp/result/scenarios.json` |
| tools/normalize_mutation | main.go | `gremlins` report → `tmp/result/mutation-test.json` |
| tools/test_report | main.go | `tmp/result/*.json` → `public/` (badges, report copies, `scenarios/`) |
| report-template | index.html | Static landing page, copied verbatim into `public/` |

# Implementation changes

`Makefile` (added to the `build`/`run`/`lint` targets [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/Repository.create.md|solution-go-repository-structure]] already created):
```makefile
.PHONY: unit-test mutation-test test-report test-and-report

# --- solution-conformance-testing-in-go: unified test/coverage/mutation contract ---
# See solution-conformance-testing.skill.md. Caller-facing flags are
# WITH_CODE_COVERAGE, ONLY_DELTA and DELTA_BASE only.

GREMLINS_VERSION := v0.6.0
GREMLINS := $(shell go env GOPATH)/bin/gremlins

# Packages coverage is measured/mutated against - generated code (gen/) and
# the reporting tools themselves (tools/) are excluded, neither has product
# logic of its own to test.
COVERPKG := $(shell go list ./... | grep -Ev '/(gen|tools)(/|$$)' | tr '\n' ',' | sed 's/,$$//')

# unit-test runs every test - Cucumber scenarios (godog) and plain Go tests -
# in one `go test ./...` invocation. Coverage is always gathered; it is only
# normalized/reported (tmp/result/coverage-test.json, tmp/report/coverage/)
# when WITH_CODE_COVERAGE=true.
unit-test:
	@mkdir -p tmp/result tmp/report/tests tmp/report/coverage
	@status=0; \
	set -o pipefail; go test -json -coverpkg=$(COVERPKG) -coverprofile=tmp/report/coverage/coverage.out ./... \
		| tee tmp/report/tests/go-test.json \
		| go run ./tools/normalize_unittest || status=$$?; \
	go run ./tools/normalize_scenarios tmp/report/tests/go-test.json || status=$$?; \
	if [ "$(WITH_CODE_COVERAGE)" = "true" ]; then \
		go tool cover -html=tmp/report/coverage/coverage.out -o tmp/report/coverage/index.html; \
		pct=$$(go tool cover -func=tmp/report/coverage/coverage.out | tail -1 | awk '{print $$3}' | tr -d '%'); \
		echo "{\"linePct\": $$pct}" > tmp/result/coverage-test.json; \
	fi; \
	exit $$status

# mutation-test runs gremlins over the whole module (or, with
# ONLY_DELTA=true, only files changed since DELTA_BASE) and exits with
# gremlins' own exit code after writing the normalized result.
mutation-test:
	@mkdir -p tmp/report/mutation
	@test -x "$(GREMLINS)" || go install github.com/go-gremlins/gremlins/cmd/gremlins@$(GREMLINS_VERSION)
	@diff_flag=""; \
	if [ "$(ONLY_DELTA)" = "true" ]; then diff_flag="--diff $(DELTA_BASE)"; fi; \
	"$(GREMLINS)" unleash --coverpkg=$(COVERPKG) --exclude-files='gen/.*' --exclude-files='tools/.*' $$diff_flag \
		--output tmp/report/mutation/gremlins.json . ; \
	code=$$?; \
	go run ./tools/normalize_mutation tmp/report/mutation/gremlins.json; \
	exit $$code

# test-report reads only tmp/result/*.json (never a tool's native report
# format) and assembles public/.
test-report:
	go run ./tools/test_report

test-and-report:
	$(MAKE) unit-test WITH_CODE_COVERAGE=true
	@mut_status=0; $(MAKE) mutation-test ONLY_DELTA=$(ONLY_DELTA) DELTA_BASE=$(DELTA_BASE) || mut_status=$$?; \
	$(MAKE) test-report; \
	exit $$mut_status
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
  <title>{service} — test report</title>
</head>
<body>
  <h1>{service} — test report</h1>
  <ul>
    <li><a href="scenarios/">Scenarios</a></li>
    <li><a href="tests/">Tests</a></li>
    <li><a href="coverage/">Coverage</a></li>
    <li><a href="mutation/">Mutation</a></li>
  </ul>
</body>
</html>
```

# Rule changes

## MUST
- `unit-test` must gather coverage on every run, and normalize/report it only behind `WITH_CODE_COVERAGE=true` — never make gathering itself conditional.
  - Risk: making coverage collection itself conditional (rather than just its reporting) means a delta-scoped mutation run has no coverage data to scope against on a run where the flag was left off.
  - Fix: always pass `-coverpkg`/`-coverprofile` to `go test`; gate only the `go tool cover`/`tmp/result/coverage-test.json` steps on the flag.
- `unit-test` must run `tools/normalize_scenarios` after `go test` whether or not a test failed, and exit with the test run's own status afterwards.
  - Risk: with `set -o pipefail` a failing `go test` ends the recipe line, so `tmp/result/scenarios.json` would be missing or stale exactly on the red run it should describe.
  - Fix: capture the pipeline's status with `|| status=$$?`, run the scenario normalizer and the coverage step in the same shell, then `exit $$status`.
- `mutation-test` must `exit $$code` with `gremlins`' own exit status after `tools/normalize_mutation` has written its normalized result — never swallow it.
  - Risk: swallowing the exit code turns a real mutation-testing failure into a silently green CI step.
  - Fix: capture `gremlins`' exit code before running the normalizer, and `exit` with it at the end of the target.
- `COVERPKG` must exclude `gen/` and `tools/` from both `unit-test` and `mutation-test`.
  - Risk: mutating generated protobuf/gRPC code or this solution's own reporting tools produces meaningless surviving-mutant noise with no product logic behind it.
  - Fix: keep the `grep -Ev '/(gen|tools)(/|$$)'` filter on `COVERPKG` and pass it to both `go test -coverpkg` and `gremlins --coverpkg`.

# Check list
- [ ] `make unit-test` produces `tmp/result/unit-test.json` and `tmp/result/scenarios.json` on every run, green or red, and exits non-zero when a test failed.
- [ ] `make unit-test WITH_CODE_COVERAGE=true` additionally produces `tmp/result/coverage-test.json` and `tmp/report/coverage/index.html`.
- [ ] `make mutation-test` installs `gremlins` on first use and exits non-zero when a mutant survives.
- [ ] `make test-report` produces `public/index.html`, `public/{tests,coverage,mutation,scenarios}/`, and the three `*-badge.json` files.
