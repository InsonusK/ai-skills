SHELL := /bin/bash

.PHONY: build run lint proto-gen

VERSION := $(shell cat VERSION 2>/dev/null || echo dev)
LDFLAGS := -X github.com/example/linkcheck-service/internal/version.Version=$(VERSION)

build:
	go build -ldflags "$(LDFLAGS)" -o bin/linkcheck ./cmd/linkcheck

proto-gen:
	buf generate proto/linkcheck --template buf/buf.gen.yaml

run: build
	./bin/linkcheck

lint:
	go vet ./...

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

# mutation: gremlins over the whole module in a `report` run; in a `pr-check`
# run only over files changed since DELTA_BASE, and skipped without one. Exits
# with gremlins' own exit code after writing the normalized result.
test-kind-mutation:
	@$(test-kind-begin)
	@if [ "$(TEST_RUN_PURPOSE)" = pr-check ] && [ -z "$(DELTA_BASE)" ]; then \
		$(call test-kind-skip,a pr-check run mutates only changed files and no DELTA_BASE was given); fi; \
	K="$(abspath $(TEST_KIND_DIR))"; diff_flag=""; \
	if [ "$(TEST_RUN_PURPOSE)" = pr-check ]; then diff_flag="--diff $(DELTA_BASE)"; \
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
