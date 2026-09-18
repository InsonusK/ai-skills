SHELL := /bin/bash

.PHONY: build run lint unit-test mutation-test test-report test-and-report

VERSION := $(shell cat VERSION 2>/dev/null || echo dev)
LDFLAGS := -X github.com/example/linkcheck-service/internal/version.Version=$(VERSION)

build:
	go build -ldflags "$(LDFLAGS)" -o bin/linkcheck ./cmd/linkcheck

run: build
	./bin/linkcheck

lint:
	go vet ./...

# --- solution-go-conformance-testing: unified test/coverage/mutation contract ---
# Caller-facing flags are WITH_CODE_COVERAGE, ONLY_DELTA and DELTA_BASE only.

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
	set -o pipefail; go test -json -coverpkg=$(COVERPKG) -coverprofile=tmp/report/coverage/coverage.out ./... \
		| tee tmp/report/tests/go-test.json \
		| go run ./tools/normalize_unittest
	@if [ "$(WITH_CODE_COVERAGE)" = "true" ]; then \
		go tool cover -html=tmp/report/coverage/coverage.out -o tmp/report/coverage/index.html; \
		pct=$$(go tool cover -func=tmp/report/coverage/coverage.out | tail -1 | awk '{print $$3}' | tr -d '%'); \
		echo "{\"linePct\": $$pct}" > tmp/result/coverage-test.json; \
	fi

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
