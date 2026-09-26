#!/usr/bin/env bash
# Runs every test project in the solution and normalizes the combined results into
# tmp/result/*.json, keeping a merged, browsable native report under tmp/report/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

SOLUTION="Sample.slnx"
WITH_CODE_COVERAGE="${WITH_CODE_COVERAGE:-false}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report"
TEST_RESULTS_DIR="tmp/TestResults"

rm -rf "$TEST_RESULTS_DIR"
find . -name reqnroll_messages.ndjson -path "*/bin/*" -delete
mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests"

COLLECT_ARGS=()
if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  COLLECT_ARGS=(--collect:"XPlat Code Coverage")
fi

# Running at the solution level picks up every test project in one command.
# LogFilePrefix gives each project's trx its own file name (a fixed LogFileName would
# make every project overwrite the same file), so we glob for every "test-results*.trx".
#
# verbosity=detailed prints every Feature/Scenario/step Reqnroll executed (with
# pass/fail), which is invaluable in CI logs on failure. @todo scenarios are excluded
# through Reqnroll's tag-to-trait mapping (Category=todo). The exit code is kept, not
# acted on yet, so the normalized results below are written on a red run too.
set +e
dotnet test "$SOLUTION" \
  --no-build --configuration Release \
  --filter "Category!=todo" \
  --results-directory "$TEST_RESULTS_DIR" \
  --logger "console;verbosity=detailed" \
  --logger "trx;LogFilePrefix=test-results" \
  "${COLLECT_ARGS[@]}"
TEST_EXIT=$?
set -e

# Merge every test project's own Reqnroll html report into one browsable report/ folder.
# Each test project's reqnroll.json writes into its own bin/ folder, per the MUST rule
# in Repository.extend.md, avoiding path collisions between projects.
mkdir -p "$REPORT_DIR/tests"
while IFS= read -r -d '' report; do
  PROJECT_NAME=$(basename "$(dirname "$(dirname "$(dirname "$(dirname "$report")")")")")
  mkdir -p "$REPORT_DIR/tests/$PROJECT_NAME"
  cp "$report" "$REPORT_DIR/tests/$PROJECT_NAME/index.html"
done < <(find . -path "*/bin/Release/*/reqnroll_report.html" -print0)

TOTAL=0
PASSED=0
FAILED=0
while IFS= read -r -d '' trx; do
  COUNTERS=$(grep -o '<Counters[^/]*/>' "$trx")
  TOTAL=$((TOTAL + $(echo "$COUNTERS" | grep -oP 'total="\K[0-9]+')))
  PASSED=$((PASSED + $(echo "$COUNTERS" | grep -oP 'passed="\K[0-9]+')))
  FAILED=$((FAILED + $(echo "$COUNTERS" | grep -oP 'failed="\K[0-9]+')))
done < <(find "$TEST_RESULTS_DIR" -name 'test-results*.trx' -print0)
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/unit-test.json"

# Scenario report: each test project's Reqnroll "message" formatter output (Cucumber
# Messages) -> [{uri, line, status}], its project-relative uri made repo-relative.
SCENARIO_RESULTS="$(mktemp)"
trap 'rm -f "$SCENARIO_RESULTS"' EXIT
while IFS= read -r -d '' messages; do
  PROJECT_DIR=$(dirname "$(dirname "$(dirname "$(dirname "$messages")")")")
  PROJECT_DIR="${PROJECT_DIR#./}"
  jq -s --arg prefix "$PROJECT_DIR/" -f scripts/messages-results.jq "$messages"
done < <(find . -path "*/bin/Release/*/reqnroll_messages.ndjson" -print0) \
  | jq -s 'add // []' > "$SCENARIO_RESULTS"
scripts/normalize-scenarios.sh "$SCENARIO_RESULTS"

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  # The glob already matches every test project's own coverage.cobertura.xml, so
  # ReportGenerator merges them all into one summary without any extra wiring.
  dotnet reportgenerator \
    "-reports:$TEST_RESULTS_DIR/**/coverage.cobertura.xml" \
    "-targetdir:$REPORT_DIR/coverage" \
    -reporttypes:"Html;JsonSummary"

  LINE_PCT=$(jq '.summary.linecoverage' "$REPORT_DIR/coverage/Summary.json")
  rm "$REPORT_DIR/coverage/Summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi

exit "$TEST_EXIT"
