#!/usr/bin/env bash
# The unit test kind: runs the Cucumber/Gherkin conformance suite via behave (plus the
# plain test/ suite), both under coverage.py, and normalizes the results into
# $TEST_KIND_DIR/result/*.json, keeping the native report under $TEST_KIND_DIR/report/.
# Called by `make test-kind-unit`, which exports:
#   TEST_KIND_DIR      the only directory this kind writes to
#   TEST_RUN_PURPOSE   report: also report line coverage; check: tests only
set -euo pipefail

KIND_DIR="${TEST_KIND_DIR:?run this through make test-kind-unit}"
WITH_CODE_COVERAGE=false
if [ "${TEST_RUN_PURPOSE:-report}" = report ]; then WITH_CODE_COVERAGE=true; fi

RESULT_DIR="$KIND_DIR/result"
REPORT_DIR="$KIND_DIR/report"

rm -rf "$REPORT_DIR/tests" "$REPORT_DIR/coverage" .coverage
mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests/cucumber"

BEHAVE_JSON="$(mktemp)"
trap 'rm -f "$BEHAVE_JSON"' EXIT

# Two formatters: behave's own json.pretty feeds the counts and the scenario report
# below; behave-cucumber-formatter writes classic Cucumber JSON - the standard report
# tools/livingdoc renders (behave's own JSON is not that schema).
# behave pairs each --outfile with the --format before it, so json.pretty comes first.
# @todo scenarios are excluded (behave reports them as "skipped"). Exit codes are kept,
# not acted on yet, so the normalized results below are written on a red run too.
set +e
coverage run -m behave \
  --tags=-todo \
  --format json.pretty --outfile "$BEHAVE_JSON" \
  --format behave_cucumber_formatter:PrettyCucumberJSONFormatter --outfile "$REPORT_DIR/tests/cucumber/behave.json" \
  --format progress
BEHAVE_EXIT=$?

coverage run -a -m pytest test/
PYTEST_EXIT=$?
set -e

# behave's JSON formatter output is modeled after Cucumber's own JSON schema: a list of
# features, each with "elements" (scenarios), each with "steps" carrying a
# "result.status". Verify this against the behave version this project pins.
# Scenarios excluded by --tags=-todo appear with status "skipped" and are not counted.
TOTAL=$(jq -s '[.[][]?.elements[]? | select(.status == "passed" or .status == "failed")] | length' "$BEHAVE_JSON")
PASSED=$(jq -s '[.[][]?.elements[]? | select(.status == "passed")] | length' "$BEHAVE_JSON")
FAILED=$((TOTAL - PASSED))
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/unit-test.json"

# Scenario report: behave's "location" is "<uri>:<line>" - the Scenario line, or the
# Examples row line for a Scenario Outline row.
SCENARIO_RESULTS="$(mktemp)"
trap 'rm -f "$BEHAVE_JSON" "$SCENARIO_RESULTS"' EXIT
jq -s '[.[][]?.elements[]? | (.location | split(":")) as [$uri, $line] | {uri: $uri, line: ($line | tonumber), status}]' \
  "$BEHAVE_JSON" > "$SCENARIO_RESULTS"
scripts/normalize-scenarios.sh "$SCENARIO_RESULTS"

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  coverage html -d "$REPORT_DIR/coverage"
  coverage json -o "$REPORT_DIR/coverage/coverage.json"
  LINE_PCT=$(jq '.totals.percent_covered' "$REPORT_DIR/coverage/coverage.json")
  rm "$REPORT_DIR/coverage/coverage.json"
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

if [ "$BEHAVE_EXIT" -ne 0 ]; then exit "$BEHAVE_EXIT"; fi
exit "$PYTEST_EXIT"
