#!/usr/bin/env bash
# badges: tests coverage
# The unit test kind for Python: one pytest run - pytest-bdd scenarios and the plain test/
# suite together - under coverage.py, normalized into result/*.json with the native reports
# under report/. Coverage is always gathered; it is reported only in a report run.
set -euo pipefail
source tools/testing/kind.sh

if [ "$TEST_RUN_PURPOSE" = report ]; then kind_mode "every test with coverage reported"
else kind_mode "every test - coverage not reported"; fi

pip install --quiet -e ".[dev]"

mkdir -p "$REPORT_DIR/tests/cucumber"
JUNIT="$REPORT_DIR/tests/junit.xml"
SCENARIO_RESULTS="$(mktemp)"
trap 'rm -f "$SCENARIO_RESULTS"' EXIT
echo '[]' > "$SCENARIO_RESULTS"   # stays empty when pytest stops before its session ends
export COVERAGE_FILE="$TEST_KIND_DIR/.coverage"

# One run, one exit code. @status/todo and @status/broken scenarios are excluded through
# pytest-bdd's tag-to-marker mapping. --cucumberjson is the standard report tools/livingdoc renders; the unit_scenarios
# plugin beside this script records each scenario's own line for result/scenarios.json. The
# exit code is kept, not acted on yet, so the normalized results are written on a red run too.
status=0
SCENARIO_RESULTS_FILE="$SCENARIO_RESULTS" PYTHONPATH="tools/testing/kinds${PYTHONPATH:+:$PYTHONPATH}" \
  coverage run -m pytest -p unit_scenarios -p no:cacheprovider -m "not status/todo and not status/broken" \
    --cucumberjson="$REPORT_DIR/tests/cucumber/pytest-bdd.json" \
    --junitxml="$JUNIT" || status=$?

# Counts from the JUnit report, so plain tests are counted beside the scenarios; a test
# pytest skipped is not counted. All 0 when pytest stopped before writing the report.
SUITE=$(grep -o '<testsuite [^>]*>' "$JUNIT" 2>/dev/null | head -1 || true)
count() { grep -oP " $1=\"\K[0-9]+" <<<"$SUITE" || echo 0; }
TOTAL=$(( $(count tests) - $(count skipped) ))
FAILED=$(( $(count failures) + $(count errors) ))
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$((TOTAL - FAILED))" "$FAILED" > "$RESULT_DIR/unit-test.json"
kind_badge_count tests tests "$((TOTAL - FAILED))" "$TOTAL"

bash tools/testing/normalize-scenarios.sh "$SCENARIO_RESULTS"
kind_scenarios_check || status=1   # an untagged scenario or feature is a failed check

if [ "$TEST_RUN_PURPOSE" = report ]; then
  # --fail-under=0: a threshold in pyproject.toml must not fail a report run over a score.
  coverage html --fail-under=0 -d "$REPORT_DIR/coverage"
  coverage json --fail-under=0 -o "$TEST_KIND_DIR/coverage.json"
  LINE_PCT=$(jq '.totals.percent_covered * 10 | round / 10' "$TEST_KIND_DIR/coverage.json")
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
  kind_badge_percent coverage coverage "$LINE_PCT"
fi

kind_livingdoc
exit "$status"
