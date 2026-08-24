#!/usr/bin/env bash
# Runs every test project in the solution and normalizes the combined results into
# tmp/result/*.json, keeping a merged, browsable native report under tmp/report/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

SOLUTION="SharedRules.sln"
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

# Projects that link scenarios from Sample.Domain.Rules.Spec need to run only the
# subset whose bindings live in that project. Reqnroll turns scenario tags into
# xUnit traits (Category), so we filter by tag per project. Domain.Rules.Tests
# proves every scenario, so it has no filter.
run_test_project() {
  local project_path="$1"
  local filter="$2"
  local project_name
  project_name=$(basename "$project_path" .csproj)

  local filter_args=()
  [ -n "$filter" ] && filter_args=(--filter "$filter")

  dotnet test "$project_path" \
    --no-build --configuration Release \
    --results-directory "$TEST_RESULTS_DIR/$project_name" \
    --logger "console;verbosity=detailed" \
    --logger "trx;LogFileName=test-results.trx" \
    "${filter_args[@]}" \
    "${COLLECT_ARGS[@]}"
}

dotnet build "$SOLUTION" --configuration Release

run_test_project "src/Shared.Tests/Shared.Tests.csproj" ""
run_test_project "src/BuildingBlocks.Tests/BuildingBlocks.Tests.csproj" ""
run_test_project "src/Modules/Sample/Sample.Interfaces.Tests/Sample.Interfaces.Tests.csproj" ""
run_test_project "src/Modules/Sample/Sample.Domain.Rules.Tests/Sample.Domain.Rules.Tests.csproj" ""
# Domain.Tests owns @format scenarios and its own untagged scenarios.
run_test_project "src/Modules/Sample/Sample.Domain.Tests/Sample.Domain.Tests.csproj" "Category!=semantic&Category!=domain"
# Application.Tests owns @semantic/@domain scenarios and its own untagged scenarios.
run_test_project "src/Modules/Sample/Sample.Application.Tests/Sample.Application.Tests.csproj" "Category!=format"

mkdir -p "$REPORT_DIR/tests"
while IFS= read -r -d '' report; do
  PROJECT_DIR=$(dirname "$(dirname "$(dirname "$report")")")
  PROJECT_NAME=$(basename "$PROJECT_DIR")
  mkdir -p "$REPORT_DIR/tests/$PROJECT_NAME"
  cp "$report" "$REPORT_DIR/tests/$PROJECT_NAME/index.html"
done < <(find src -path '*/bin/Release/net10.0/reqnroll_report.html' -type f -print0)

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
