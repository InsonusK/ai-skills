#!/usr/bin/env bash
# badges: tests coverage
# The unit test kind for TypeScript: the Cucumber suite via cucumber-js, normalized into
# result/*.json with the native HTML report under report/. Coverage is collected and reported
# only in a report run.
set -euo pipefail
source tools/testing/kind.sh

WITH_CODE_COVERAGE=false
if [ "$TEST_RUN_PURPOSE" = report ]; then
  WITH_CODE_COVERAGE=true
  kind_mode "every test with coverage reported"
else
  kind_mode "every test - coverage not reported"
fi

npm install

rm -rf "$REPORT_DIR/tests" "$REPORT_DIR/coverage"
mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests/cucumber"

CUCUMBER_JSON="$(mktemp)"
# The standard report (Cucumber Messages, per cucumber-testing-in-typescript) is kept:
# tools/livingdoc renders it into report/tests/livingdoc/.
CUCUMBER_MESSAGES="$REPORT_DIR/tests/cucumber/messages.ndjson"
SCENARIO_RESULTS="$(mktemp)"
trap 'rm -f "$CUCUMBER_JSON" "$SCENARIO_RESULTS"' EXIT

CUCUMBER_ARGS=(
  'features/**/*.feature'
  --require-module tsx/cjs
  --require 'features/step-definitions/**/*.ts'
  --tags 'not @status/todo and not @status/broken'
  --format progress
  --format "html:$REPORT_DIR/tests/index.html"
  --format "json:$CUCUMBER_JSON"
  --format "message:$CUCUMBER_MESSAGES"
)

# The exit code is kept, not acted on yet, so the normalized results below are
# written on a red run too.
set +e
if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  npx c8 --reporter=html --reporter=json-summary --report-dir="$REPORT_DIR/coverage" \
    --temp-directory="$TEST_KIND_DIR/c8-tmp" -- \
    npx cucumber-js "${CUCUMBER_ARGS[@]}"
else
  npx cucumber-js "${CUCUMBER_ARGS[@]}"
fi
CUCUMBER_EXIT=$?
set -e

# -s keeps the counts at 0 (not empty) when cucumber-js crashed before writing its report.
TOTAL=$(jq -s '[.[][]?.elements[]?] | length' "$CUCUMBER_JSON")
PASSED=$(jq -s '[.[][]?.elements[]? | select(all(.steps[]; .result.status == "passed"))] | length' "$CUCUMBER_JSON")
FAILED=$((TOTAL - PASSED))
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/unit-test.json"
kind_badge_count tests tests "$PASSED" "$TOTAL"

# Scenario report: cucumber-js's Cucumber Messages -> [{uri, line, status}]; its uris
# are already relative to the repository root.
jq -s --arg prefix "" -f tools/testing/messages-results.jq "$CUCUMBER_MESSAGES" > "$SCENARIO_RESULTS"
bash tools/testing/normalize-scenarios.sh "$SCENARIO_RESULTS"
kind_scenarios_check || CUCUMBER_EXIT=1   # an untagged scenario or feature is a failed check

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  LINE_PCT=$(jq '.total.lines.pct' "$REPORT_DIR/coverage/coverage-summary.json")
  rm "$REPORT_DIR/coverage/coverage-summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
  kind_badge_percent coverage coverage "$LINE_PCT"
fi

# Living-doc report from the standard Cucumber report.
kind_livingdoc

exit "$CUCUMBER_EXIT"
