#!/usr/bin/env bash
# badges: mutation
# The mutation test kind for .NET: Stryker.NET over the whole solution in a report run, where
# the score never fails the run (--break-at 0); in a check run only over code changed since
# DELTA_BASE with the configured thresholds, and skipped without one.
set -euo pipefail
source tools/testing/kind.sh

MUTATION_DIR="$REPORT_DIR/mutation"
STRYKER_ARGS=(-r html -r json -r cleartext -O "$MUTATION_DIR" --break-on-initial-test-failure)
if [ "$TEST_RUN_PURPOSE" = check ]; then
  [ -n "$DELTA_BASE" ] || kind_skip "a check run mutates only changed code and no DELTA_BASE was given"
  STRYKER_ARGS+=(--since:"$DELTA_BASE")
  kind_mode "mutating only code changed since $DELTA_BASE"
else
  STRYKER_ARGS+=(--break-at 0)
  kind_mode "mutating the whole solution - the score never fails the run"
fi

dotnet tool restore

set +e
dotnet tool run dotnet-stryker "${STRYKER_ARGS[@]}"
STRYKER_EXIT_CODE=$?
set -e

MUTATION_JSON="$MUTATION_DIR/reports/mutation-report.json"
if [ -f "$MUTATION_JSON" ]; then
  KILLED=$(jq '[.files[].mutants[].status] | map(select(. == "Killed")) | length' "$MUTATION_JSON")
  SURVIVED=$(jq '[.files[].mutants[].status] | map(select(. == "Survived")) | length' "$MUTATION_JSON")
  TIMEDOUT=$(jq '[.files[].mutants[].status] | map(select(. == "Timeout")) | length' "$MUTATION_JSON")
  NO_COVERAGE=$(jq '[.files[].mutants[].status] | map(select(. == "NoCoverage")) | length' "$MUTATION_JSON")
  TESTED=$((KILLED + SURVIVED + TIMEDOUT + NO_COVERAGE))
  if [ "$TESTED" -eq 0 ]; then
    SCORE="0.0"
  else
    SCORE=$(awk -v k="$KILLED" -v t="$TESTED" 'BEGIN { printf "%.1f", (k / t) * 100 }')
  fi
  printf '{"killed":%s,"survived":%s,"timedout":%s,"noCoverage":%s,"score":%s}' \
    "$KILLED" "$SURVIVED" "$TIMEDOUT" "$NO_COVERAGE" "$SCORE" > "$RESULT_DIR/mutation-test.json"
fi

exit $STRYKER_EXIT_CODE
