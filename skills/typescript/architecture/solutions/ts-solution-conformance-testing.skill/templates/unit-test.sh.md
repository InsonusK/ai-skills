# scripts/unit-test.sh

Runs `cucumber-js` (and, when `WITH_CODE_COVERAGE=true`, wraps it with `c8` for coverage), then normalizes the result into `tmp/result/*.json`, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]].

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
trap 'rm -f "$CUCUMBER_JSON"' EXIT

CUCUMBER_ARGS=(
  'features/**/*.feature'
  --require-module ts-node/register
  --require 'features/step-definitions/**/*.ts'
  --format progress
  --format "html:$REPORT_DIR/tests/index.html"
  --format "json:$CUCUMBER_JSON"
)

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  npx c8 --reporter=html --reporter=json-summary --report-dir="$REPORT_DIR/coverage" -- \
    npx cucumber-js "${CUCUMBER_ARGS[@]}"
else
  npx cucumber-js "${CUCUMBER_ARGS[@]}"
fi

TOTAL=$(jq '[.[].elements[]] | length' "$CUCUMBER_JSON")
PASSED=$(jq '[.[].elements[] | select(all(.steps[]; .result.status == "passed"))] | length' "$CUCUMBER_JSON")
FAILED=$((TOTAL - PASSED))
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/unit-test.json"

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  LINE_PCT=$(jq '.total.lines.pct' "$REPORT_DIR/coverage/coverage-summary.json")
  rm "$REPORT_DIR/coverage/coverage-summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi
```
