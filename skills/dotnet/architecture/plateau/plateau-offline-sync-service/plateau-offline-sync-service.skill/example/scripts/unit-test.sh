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
rm -rf TestResults

RUN_ARGS=(--report-xunit-trx)
if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  RUN_ARGS+=(--coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml)
fi

# Capture the MTP run so the aggregate count comes from its own summary line, not from
# per-project TRX files (whose timestamp-based names can collide across parallel projects).
set +e
dotnet test "$SOLUTION" --configuration Release --no-build -- "${RUN_ARGS[@]}" | tee tmp/test-output.txt
TEST_EXIT=${PIPESTATUS[0]}
set -e

# Merge each project's Reqnroll HTML report.
while IFS= read -r -d '' report; do
  PROJECT_NAME=$(basename "$(dirname "$(dirname "$(dirname "$(dirname "$report")")")")")
  mkdir -p "$REPORT_DIR/tests/$PROJECT_NAME"
  cp "$report" "$REPORT_DIR/tests/$PROJECT_NAME/index.html"
done < <(find tests -path "*/bin/Release/net10.0/reqnroll_report.html" -print0)

# Aggregate from the MTP summary block ("  total: N / failed: N / succeeded: N").
TOTAL=$(grep -oP '^\s*total:\s*\K[0-9]+' tmp/test-output.txt | tail -1 || echo 0)
FAILED=$(grep -oP '^\s*failed:\s*\K[0-9]+' tmp/test-output.txt | tail -1 || echo 0)
PASSED=$(grep -oP '^\s*succeeded:\s*\K[0-9]+' tmp/test-output.txt | tail -1 || echo 0)
printf '{"total":%s,"passed":%s,"failed":%s}' "${TOTAL:-0}" "${PASSED:-0}" "${FAILED:-0}" > "$RESULT_DIR/unit-test.json"
rm -f tmp/test-output.txt

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  dotnet tool run reportgenerator \
    "-reports:TestResults/**/coverage.cobertura.xml" \
    "-targetdir:$REPORT_DIR/coverage" \
    -reporttypes:"Html;JsonSummary"
  LINE_PCT=$(jq '.summary.linecoverage' "$REPORT_DIR/coverage/Summary.json")
  rm "$REPORT_DIR/coverage/Summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi

exit "$TEST_EXIT"
