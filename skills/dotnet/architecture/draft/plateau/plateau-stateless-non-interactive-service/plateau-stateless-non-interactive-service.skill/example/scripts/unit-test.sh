#!/usr/bin/env bash
# Runs every test project in the solution and normalizes the combined results into
# tmp/result/*.json, keeping a merged, browsable native report under tmp/report/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

SOLUTION="StatelessService.sln"
WITH_CODE_COVERAGE="${WITH_CODE_COVERAGE:-false}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report"
TEST_RESULTS_DIR="tmp/TestResults"

rm -rf "$TEST_RESULTS_DIR"
mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests"

COLLECT_ARGS=()
if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  COLLECT_ARGS=(--collect:"XPlat Code Coverage")
fi

dotnet test "$SOLUTION" \
  --no-build --configuration Release \
  --results-directory "$TEST_RESULTS_DIR" \
  --logger "console;verbosity=detailed" \
  --logger "trx;LogFileName=test-results.trx" \
  "${COLLECT_ARGS[@]}"

mkdir -p "$REPORT_DIR/tests"
for report in src/*/bin/Release/net8.0/reqnroll_report.html src/*/*/bin/Release/net8.0/reqnroll_report.html; do
  [ -f "$report" ] || continue
  PROJECT_NAME=$(basename "$(dirname "$(dirname "$(dirname "$(dirname "$report")")")")")
  mkdir -p "$REPORT_DIR/tests/$PROJECT_NAME"
  cp "$report" "$REPORT_DIR/tests/$PROJECT_NAME/index.html"
done

TOTAL=0
PASSED=0
FAILED=0
while IFS= read -r -d '' trx; do
  COUNTERS=$(grep -o '<Counters[^/]*/>' "$trx")
  TOTAL=$((TOTAL + $(echo "$COUNTERS" | grep -oP 'total="\K[0-9]+')))
  PASSED=$((PASSED + $(echo "$COUNTERS" | grep -oP 'passed="\K[0-9]+')))
  FAILED=$((FAILED + $(echo "$COUNTERS" | grep -oP 'failed="\K[0-9]+')))
done < <(find "$TEST_RESULTS_DIR" -name 'test-results.trx' -print0)
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/unit-test.json"

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  dotnet reportgenerator \
    "-reports:$TEST_RESULTS_DIR/**/coverage.cobertura.xml" \
    "-targetdir:$REPORT_DIR/coverage" \
    -reporttypes:"Html;JsonSummary"

  LINE_PCT=$(jq '.summary.linecoverage' "$REPORT_DIR/coverage/Summary.json")
  rm "$REPORT_DIR/coverage/Summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi
