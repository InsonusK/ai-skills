# scripts/unit-test.sh

Runs the whole solution's test projects together — `{Module}.Domain.Tests`, `{Module}.Application.Tests`, `{Module}.Interfaces.Tests`, `Shared.Tests`, `BuildingBlocks.Tests` — and merges their Reqnroll scenarios, TRX counters, and coverage (when `WITH_CODE_COVERAGE=true`) into one normalized result, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]].

```bash
#!/usr/bin/env bash
# Runs every test project in the solution and normalizes the combined results into
# tmp/result/*.json, keeping a merged, browsable native report under tmp/report/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

SOLUTION="{Module}.slnx"
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

# Running at the solution level picks up all five test projects in one command.
# --results-directory nests each project's own trx under its own subfolder, so we glob
# for every "test-results.trx" that gets produced instead of assuming just one.
#
# verbosity=detailed prints every Feature/Scenario/step Reqnroll executed (with
# pass/fail), which is invaluable in CI logs on failure.
dotnet test "$SOLUTION" \
  --no-build --configuration Release \
  --results-directory "$TEST_RESULTS_DIR" \
  --logger "console;verbosity=detailed" \
  --logger "trx;LogFileName=test-results.trx" \
  "${COLLECT_ARGS[@]}"

# Merge every test project's own Reqnroll html report into one browsable report/ folder.
# Each *.Tests/reqnroll.json points its formatter at its own project-relative path, per
# the MUST rule in Repository.extend.md, avoiding path collisions between projects.
mkdir -p "$REPORT_DIR/tests"
for report in */bin/Release/{TargetFramework}/reqnroll_report.html; do
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
  # The glob already matches every test project's own coverage.cobertura.xml, so
  # ReportGenerator merges all five into one summary without any extra wiring.
  dotnet reportgenerator \
    "-reports:$TEST_RESULTS_DIR/**/coverage.cobertura.xml" \
    "-targetdir:$REPORT_DIR/coverage" \
    -reporttypes:"Html;JsonSummary"

  LINE_PCT=$(jq '.summary.linecoverage' "$REPORT_DIR/coverage/Summary.json")
  rm "$REPORT_DIR/coverage/Summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi
```

## {TestProject}/reqnroll.json

Each of the five test projects has its own `reqnroll.json`, configuring the html formatter to a project-specific path so five parallel runs never overwrite each other's output:

```json
{
  "formatters": {
    "html": { "outputFilePath": "reqnroll_report.html" }
  }
}
```
