#!/usr/bin/env bash
# Runs every test project in the solution (Microsoft.Testing.Platform via global.json)
# and normalizes the combined results into tmp/result/*.json, keeping the merged
# Reqnroll report under tmp/report/tests/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

SOLUTION="Sample.slnx"
WITH_CODE_COVERAGE="${WITH_CODE_COVERAGE:-false}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report"

mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests"
# `dotnet test` (MTP) collects each project's --report-xunit-trx into ./TestResults at the
# solution root — clear it first so the aggregate count is not stale.
rm -rf TestResults

RUN_ARGS=(--report-xunit-trx)
if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  RUN_ARGS+=(--coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml)
fi

dotnet test "$SOLUTION" --configuration Release --no-build -- "${RUN_ARGS[@]}"

# Merge each project's Reqnroll HTML report.
while IFS= read -r -d '' report; do
  PROJECT_NAME=$(basename "$(dirname "$(dirname "$(dirname "$(dirname "$report")")")")")
  mkdir -p "$REPORT_DIR/tests/$PROJECT_NAME"
  cp "$report" "$REPORT_DIR/tests/$PROJECT_NAME/index.html"
done < <(find tests -path "*/bin/Release/net10.0/reqnroll_report.html" -print0)

# Aggregate TRX counters across every test project into one summary.
TOTAL=0; PASSED=0; FAILED=0
while IFS= read -r -d '' trx; do
  COUNTERS=$(grep -o '<Counters[^/]*/>' "$trx" || true)
  [ -z "$COUNTERS" ] && continue
  TOTAL=$((TOTAL + $(echo "$COUNTERS" | grep -oP 'total="\K[0-9]+')))
  PASSED=$((PASSED + $(echo "$COUNTERS" | grep -oP 'passed="\K[0-9]+')))
  FAILED=$((FAILED + $(echo "$COUNTERS" | grep -oP 'failed="\K[0-9]+')))
done < <(find TestResults -name '*.trx' -print0 2>/dev/null)
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/unit-test.json"

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  dotnet tool run reportgenerator \
    "-reports:TestResults/**/coverage.cobertura.xml" \
    "-targetdir:$REPORT_DIR/coverage" \
    -reporttypes:"Html;JsonSummary"
  LINE_PCT=$(jq '.summary.linecoverage' "$REPORT_DIR/coverage/Summary.json")
  rm "$REPORT_DIR/coverage/Summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi

[ "$FAILED" -eq 0 ]
