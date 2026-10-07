# scripts/unit-test.sh

Runs `cucumber-js` (`@todo` scenarios excluded; wrapped with `c8` for coverage in a `report` run), then normalizes the result into `$TEST_KIND_DIR/result/*.json` — `scenarios.json` included, on a red run too — and exits with `cucumber-js`'s own code, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]]. Verified with `@cucumber/cucumber` 13, `tsx` 4, TypeScript 7. `tsx` transpiles through esbuild, so it does not depend on the TypeScript compiler's version — unlike `ts-node`, which fails to load under TypeScript 6+.

```bash
#!/usr/bin/env bash
# The unit test kind: runs the Cucumber/Gherkin conformance suite via cucumber-js and
# normalizes the results into $TEST_KIND_DIR/result/unit-test.json (+ coverage-test.json
# in a report run), keeping the native HTML report(s) under $TEST_KIND_DIR/report/.
# Called by `make test-kind-unit`, which exports:
#   TEST_KIND_DIR      the only directory this kind writes to
#   TEST_RUN_PURPOSE   report: also collect and report line coverage; check: tests only
set -euo pipefail

KIND_DIR="${TEST_KIND_DIR:?run this through make test-kind-unit}"
WITH_CODE_COVERAGE=false
if [ "${TEST_RUN_PURPOSE:-report}" = report ]; then WITH_CODE_COVERAGE=true; fi

RESULT_DIR="$KIND_DIR/result"
REPORT_DIR="$KIND_DIR/report"

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

# Living-doc report from the standard Cucumber report - shared, pinned renderer in
# tools/livingdoc (solution-conformance-testing). Skipped without npm; never changes
# the exit code.
if command -v npm >/dev/null 2>&1; then
  { npm ci --prefix tools/livingdoc --silent \
      && node tools/livingdoc/render.mjs "$REPORT_DIR/tests/cucumber" "$REPORT_DIR/tests/livingdoc"; } \
    || echo "livingdoc: render failed"
else
  echo "livingdoc: npm not found - skipping living-doc report"
fi

exit "$CUCUMBER_EXIT"
```
