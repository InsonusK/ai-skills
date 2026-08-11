# scripts/cucumber-test.sh

Runs the Reqnroll scenarios (and, when `WITH_CODE_COVERAGE=true`, line coverage), then normalizes the result into `tmp/result/*.json`, per [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract).

```bash
#!/usr/bin/env bash
# Runs the Cucumber/Gherkin conformance suite and normalizes the results into
# tmp/result/*.json, keeping the native browsable report under tmp/report/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

TEST_PROJECT="{Module}.Tests/{Module}.Tests.csproj"
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

# verbosity=detailed prints every Feature/Scenario/step Reqnroll executed (with
# pass/fail), which is invaluable in CI logs on failure.
dotnet test "$TEST_PROJECT" \
  --no-build --configuration Release \
  --results-directory "$TEST_RESULTS_DIR" \
  --logger "console;verbosity=detailed" \
  --logger "trx;LogFileName=test-results.trx" \
  "${COLLECT_ARGS[@]}"

# Reqnroll's html formatter (configured in {Module}.Tests/reqnroll.json - see below)
# renders Gherkin features/scenarios/steps.
cp {Module}.Tests/bin/Release/{TargetFramework}/reqnroll_report.html "$REPORT_DIR/tests/index.html"

TRX_COUNTERS=$(grep -o '<Counters[^/]*/>' "$TEST_RESULTS_DIR/test-results.trx")
TOTAL=$(echo "$TRX_COUNTERS" | grep -oP 'total="\K[0-9]+')
PASSED=$(echo "$TRX_COUNTERS" | grep -oP 'passed="\K[0-9]+')
FAILED=$(echo "$TRX_COUNTERS" | grep -oP 'failed="\K[0-9]+')
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/cucumber-test.json"

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  dotnet reportgenerator \
    "-reports:$TEST_RESULTS_DIR/**/coverage.cobertura.xml" \
    "-targetdir:$REPORT_DIR/coverage" \
    -reporttypes:"Html;JsonSummary"

  LINE_PCT=$(jq '.summary.linecoverage' "$REPORT_DIR/coverage/Summary.json")
  rm "$REPORT_DIR/coverage/Summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi
```

## {Module}.Tests/reqnroll.json

Must configure the html formatter to write to the exact path the script copies from:

```json
{
  "formatters": {
    "html": { "outputFilePath": "reqnroll_report.html" }
  }
}
```
