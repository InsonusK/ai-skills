#!/usr/bin/env bash
# badges: tests coverage
# The unit test kind for Go: every test - Cucumber scenarios (godog) and plain Go tests - in
# one `go test ./...`. Coverage is always gathered; it is normalized and reported only in a
# report run.
set -euo pipefail
source tools/testing/kind.sh

# Packages coverage is measured against - generated code (gen/) and the reporting tools
# themselves (tools/) are excluded, neither has product logic of its own to test.
COVERPKG=$(go list ./... | grep -Ev '/(gen|tools)(/|$)' | tr '\n' ',' | sed 's/,$//')

if [ "$TEST_RUN_PURPOSE" = report ]; then kind_mode "every test with coverage reported"
else kind_mode "every test - coverage not reported"; fi

mkdir -p "$REPORT_DIR/tests/cucumber" "$REPORT_DIR/coverage"

# The exit status is kept, not acted on yet, so the normalized results are written on a red
# run too. CUCUMBER_JSON_DIR makes the godog runner write classic Cucumber JSON.
status=0
CUCUMBER_JSON_DIR="$REPORT_DIR/tests/cucumber" \
  go test -json -coverpkg="$COVERPKG" -coverprofile="$REPORT_DIR/coverage/coverage.out" ./... \
  | tee "$REPORT_DIR/tests/go-test.json" \
  | go run ./tools/normalize_unittest || status=$?
go run ./tools/normalize_scenarios "$REPORT_DIR/tests/go-test.json" || status=$?

if [ "$TEST_RUN_PURPOSE" = report ]; then
  go tool cover -html="$REPORT_DIR/coverage/coverage.out" -o "$REPORT_DIR/coverage/index.html"
  pct=$(go tool cover -func="$REPORT_DIR/coverage/coverage.out" | tail -1 | awk '{print $3}' | tr -d '%')
  echo "{\"linePct\": $pct}" > "$RESULT_DIR/coverage-test.json"
else
  rm -rf "$REPORT_DIR/coverage"
fi

kind_livingdoc
exit "$status"
