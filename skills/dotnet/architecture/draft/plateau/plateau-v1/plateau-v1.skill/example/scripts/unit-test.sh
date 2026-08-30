#!/usr/bin/env bash
# Runs every test project in the solution, applying the shared-rules tag filters per
# project, and normalizes the combined results into tmp/result/*.json, keeping a
# merged, browsable native report under tmp/report/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

SOLUTION="V1.sln"
WITH_CODE_COVERAGE="${WITH_CODE_COVERAGE:-false}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report"
TEST_RESULTS_DIR="tmp/TestResults"

rm -rf "$TEST_RESULTS_DIR"
mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests"

# Build once, then run each test project with the filter that matches its layer contract:
#   - Domain.Tests: @format scenarios only (exclude @semantic and @domain).
#   - Application.Tests: @semantic scenarios only (exclude @format).
#   - Domain.Rules.Tests: all shared Gherkin scenarios, no filter.
dotnet build "$SOLUTION" --configuration Release

PROJECTS=(
  "src/BuildingBlocks.Tests/BuildingBlocks.Tests.csproj"
  "src/Shared.Tests/Shared.Tests.csproj"
  "src/Modules/Sample/Sample.Interfaces.Tests/Sample.Interfaces.Tests.csproj"
  "src/Modules/Sample/Sample.Domain.Tests/Sample.Domain.Tests.csproj"
  "src/Modules/Sample/Sample.Domain.Rules.Tests/Sample.Domain.Rules.Tests.csproj"
  "src/Modules/Sample/Sample.Application.Tests/Sample.Application.Tests.csproj"
)
FILTERS=(
  ""
  ""
  ""
  "Category!=semantic&Category!=domain"
  ""
  "Category!=format"
)

COLLECT_ARGS=()
if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  COLLECT_ARGS=(--collect:"XPlat Code Coverage")
fi

mkdir -p "$REPORT_DIR/tests"

TOTAL=0
PASSED=0
FAILED=0

for i in "${!PROJECTS[@]}"; do
  PROJECT="${PROJECTS[$i]}"
  FILTER="${FILTERS[$i]}"
  PROJECT_NAME="$(basename "$(dirname "$PROJECT")")"

  FILTER_ARGS=()
  if [ -n "$FILTER" ]; then
    FILTER_ARGS=(--filter "$FILTER")
  fi

  dotnet test "$PROJECT" \
    --no-build --configuration Release \
    --results-directory "$TEST_RESULTS_DIR" \
    --logger "console;verbosity=detailed" \
    --logger "trx;LogFileName=${PROJECT_NAME}.trx" \
    "${FILTER_ARGS[@]}" \
    "${COLLECT_ARGS[@]}"

  # Reqnroll produces one HTML report per test project; copy it into the merged report tree.
  REPORT_PATH="$(dirname "$PROJECT")/bin/Release/net10.0/reqnroll_report.html"
  if [ -f "$REPORT_PATH" ]; then
    mkdir -p "$REPORT_DIR/tests/$PROJECT_NAME"
    cp "$REPORT_PATH" "$REPORT_DIR/tests/$PROJECT_NAME/index.html"
  fi
done

while IFS= read -r -d '' trx; do
  COUNTERS=$(grep -o '<Counters[^/]*/>' "$trx")
  TOTAL=$((TOTAL + $(echo "$COUNTERS" | grep -oP 'total="\K[0-9]+')))
  PASSED=$((PASSED + $(echo "$COUNTERS" | grep -oP 'passed="\K[0-9]+')))
  FAILED=$((FAILED + $(echo "$COUNTERS" | grep -oP 'failed="\K[0-9]+')))
done < <(find "$TEST_RESULTS_DIR" -name '*.trx' -print0)
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
