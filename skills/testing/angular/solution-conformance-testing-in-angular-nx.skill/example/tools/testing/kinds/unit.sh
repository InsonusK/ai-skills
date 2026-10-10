#!/usr/bin/env bash
# badges: tests coverage
set -euo pipefail
source tools/testing/kind.sh
source tools/testing/nx-env.sh
kind_mode "Nx unit: all applicable projects; coverage collected; cache disabled"
if [ "$TEST_RUN_PURPOSE" = check ] && [ -n "$DELTA_BASE" ]; then
  kind_mode "Nx unit: affected projects since $DELTA_BASE; full inventory; cache disabled"
fi
coverage="$TEST_KIND_DIR/coverage-data"
reporters=(--reporter=json-summary)
if [ "$TEST_RUN_PURPOSE" = report ]; then
  coverage="$REPORT_DIR/coverage"
  reporters+=(--reporter=html)
fi
status=0
./node_modules/.bin/c8 "${reporters[@]}" --report-dir="$coverage" --temp-directory="$TEST_KIND_DIR/c8-tmp"   node tools/testing/nx-kind.mjs run unit || status=$?
reason=$(jq -r '.skipReason // empty' "$RESULT_DIR/projects.json")
if [ -n "$reason" ]; then
  # Retain the complete source inventory, but a skipped kind publishes no badge/report.
  results=$(mktemp)
  echo '[]' > "$results"
  bash tools/testing/normalize-scenarios.sh "$results"
  rm "$results"
  kind_scenarios_check
  kind_skip "$reason"
fi
node tools/testing/nx-kind.mjs unit unit || status=1
kind_badge_count tests tests "$(jq -r '.passed' "$RESULT_DIR/unit-test.json")" "$(jq -r '.total' "$RESULT_DIR/unit-test.json")"
results=$(mktemp)
trap 'rm -f "$results"' EXIT
shopt -s nullglob
messages=("$REPORT_DIR/tests/cucumber/"*.ndjson)
if [ "${#messages[@]}" -gt 0 ]; then
  jq -s --arg prefix "" -f tools/testing/messages-results.jq "${messages[@]}" > "$results"
else
  echo '[]' > "$results"
fi
bash tools/testing/normalize-scenarios.sh "$results"
kind_scenarios_check || status=1
if [ "$status" -ne 0 ]; then kind_badge tests tests "project runner failed" red; fi
if [ "$TEST_RUN_PURPOSE" = report ]; then
  pct=$(jq -r '.total.lines.pct' "$coverage/coverage-summary.json")
  printf '{"linePct":%s}' "$pct" > "$RESULT_DIR/coverage-test.json"
  kind_badge_percent coverage coverage "$pct"
  rm "$coverage/coverage-summary.json"
fi
kind_livingdoc
node tools/testing/nx-kind.mjs inventory unit
cp "$RESULT_DIR/projects.json" "$REPORT_DIR/tests/projects.json"
exit "$status"
