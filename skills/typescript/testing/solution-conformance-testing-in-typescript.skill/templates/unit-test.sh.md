# scripts/unit-test.sh

Runs `cucumber-js` (`@todo` scenarios excluded; wrapped with `c8` for coverage when `WITH_CODE_COVERAGE=true`), then normalizes the result into `tmp/result/*.json` — `scenarios.json` included, on a red run too — and exits with `cucumber-js`'s own code, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]]. Verified with `@cucumber/cucumber` 13, `ts-node` 10 + TypeScript 5 (ts-node does not load under TypeScript 6+).

```bash
#!/usr/bin/env bash
# Runs the Cucumber/Gherkin conformance suite via cucumber-js and normalizes the
# results into tmp/result/unit-test.json (+ coverage-test.json when
# WITH_CODE_COVERAGE=true), keeping the native HTML report(s) under tmp/report/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

WITH_CODE_COVERAGE="${WITH_CODE_COVERAGE:-false}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report"

rm -rf "$REPORT_DIR/tests" "$REPORT_DIR/coverage"
mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests"

CUCUMBER_JSON="$(mktemp)"
CUCUMBER_MESSAGES="$(mktemp)"
SCENARIO_RESULTS="$(mktemp)"
trap 'rm -f "$CUCUMBER_JSON" "$CUCUMBER_MESSAGES" "$SCENARIO_RESULTS"' EXIT

CUCUMBER_ARGS=(
  'features/**/*.feature'
  --require-module ts-node/register
  --require 'features/step-definitions/**/*.ts'
  --tags 'not @todo'
  --format progress
  --format "html:$REPORT_DIR/tests/index.html"
  --format "json:$CUCUMBER_JSON"
  --format "message:$CUCUMBER_MESSAGES"
)

# The exit code is kept, not acted on yet, so the normalized results below are
# written on a red run too.
set +e
if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  npx c8 --reporter=html --reporter=json-summary --report-dir="$REPORT_DIR/coverage" -- \
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

# Scenario report: cucumber-js's Cucumber Messages -> [{uri, line, status}]; its uris
# are already relative to the repository root.
jq -s --arg prefix "" -f scripts/messages-results.jq "$CUCUMBER_MESSAGES" > "$SCENARIO_RESULTS"
scripts/normalize-scenarios.sh "$SCENARIO_RESULTS"

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  LINE_PCT=$(jq '.total.lines.pct' "$REPORT_DIR/coverage/coverage-summary.json")
  rm "$REPORT_DIR/coverage/coverage-summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi

exit "$CUCUMBER_EXIT"
```
